using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

/// <summary>
/// Controlador principal del rescatista: movimiento, salto, salud, inventario del nivel,
/// animaciones y menú de pausa.
/// </summary>
[RequireComponent(typeof(Rigidbody))]
public class PlayerController : MonoBehaviour
{
    [Header("Movimiento")]
    public float velocidadMovimiento = 6f;
    public float velocidadCorrer = 9f;
    public float fuerzaSalto = 6f;
    public float velocidadGiro = 12f;

    [Header("Referencia al Modelo Visual")]
    public Transform modeloVisualDoctor;
    public Animator animador;

    [Header("Estadísticas de Salud")]
    public int vidaMaxima = 100;
    public int vidaActual = 100;
    public Slider barraDeVida;

    [Header("Inventario Médico")]
    public int cantidadSuministros = 0; // Compatibilidad con código previo
    public int jeringasRecolectadas = 0;
    public int equipajesRecolectados = 0;
    public int botiquinesRecolectados = 0;
    public int jeringasMeta = 0;
    public int equipajesMeta = 0;

    [Header("Interfaz de Usuario (HUD)")]
    public TextMeshProUGUI textoSuministros;
    public TextMeshProUGUI textoJeringas;
    public TextMeshProUGUI textoEquipajes;
    public TextMeshProUGUI textoObjetivo;
    public GameObject panelPausa;

    [Header("Cámara")]
    public Camera camaraJugador;

    private Rigidbody rb;
    private CapsuleCollider capsula;
    private readonly Collider[] contactosSuelo = new Collider[8];
    private Vector3 puntoInicio;
    private bool enElSuelo = true;
    private float esperaSalto = 0f;
    private float finEmpuje = 0f;
    private int vidaExtra = 0;
    private float velocidadExtra = 0f;
    private bool juegoPausado = false;
    private float tiempoInvulnerable = 0f;
    private const float TIEMPO_INVULNERABILIDAD_BASE = 0.8f;
    private const float ALTURA_CAIDA_LIMITE = -25f;

    /// <summary>Evita que el ESC que cierra un menú del Hub abra también la pausa.</summary>
    public static int frameMenuCerrado = -1;

    private int VidaTotal => vidaMaxima + vidaExtra;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true; // Evitar que caiga de lado por físicas
        rb.interpolation = RigidbodyInterpolation.Interpolate;

        capsula = GetComponent<CapsuleCollider>();
        if (capsula != null)
        {
            // Sin fricción para no quedar pegado a las paredes al saltar.
            capsula.material = new PhysicsMaterial("JugadorSinFriccion")
            {
                dynamicFriction = 0f,
                staticFriction = 0f,
                frictionCombine = PhysicsMaterialCombine.Minimum
            };
        }
    }

    private void Start()
    {
        puntoInicio = transform.position;
        vidaActual = VidaTotal;
        ActualizarBarraVida();

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        if (panelPausa != null)
        {
            panelPausa.SetActive(false);
        }

        if (camaraJugador == null)
        {
            camaraJugador = Camera.main;
        }

        ActualizarHUD();
    }

    private void Update()
    {
        ProcesarEntradaPausa();

        if (juegoPausado) return;

        enElSuelo = DetectarSuelo();
        ProcesarMovimiento();
        ProcesarSalto();
        ActualizarAnimador();

        if (tiempoInvulnerable > 0f)
        {
            tiempoInvulnerable -= Time.deltaTime;
        }

        if (transform.position.y < ALTURA_CAIDA_LIMITE)
        {
            VolverAlInicio();
        }
    }

    private bool DetectarSuelo()
    {
        if (capsula == null) return true;

        float radio = capsula.radius * 0.9f;
        Vector3 centro = capsula.bounds.center;
        centro.y = capsula.bounds.min.y + radio - 0.1f;

        int total = Physics.OverlapSphereNonAlloc(centro, radio, contactosSuelo, ~0, QueryTriggerInteraction.Ignore);
        for (int i = 0; i < total; i++)
        {
            if (!contactosSuelo[i].transform.IsChildOf(transform)) return true;
        }
        return false;
    }

    private void ProcesarMovimiento()
    {
        // Durante un empuje el jugador no controla al personaje.
        if (Time.time < finEmpuje) return;

        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");

        Vector3 direccionDeseada = Vector3.zero;

        if (camaraJugador != null)
        {
            Vector3 camFwd = camaraJugador.transform.forward;
            camFwd.y = 0f;
            camFwd.Normalize();

            Vector3 camRight = camaraJugador.transform.right;
            camRight.y = 0f;
            camRight.Normalize();

            direccionDeseada = (camFwd * v + camRight * h).normalized;
        }
        else
        {
            direccionDeseada = new Vector3(h, 0f, v).normalized;
        }

        bool estaCorriendo = Input.GetKey(KeyCode.LeftShift);
        float velActual = (estaCorriendo ? velocidadCorrer : velocidadMovimiento) + velocidadExtra;

        Vector3 velFisica = rb.linearVelocity;
        velFisica.x = direccionDeseada.x * velActual;
        velFisica.z = direccionDeseada.z * velActual;
        rb.linearVelocity = velFisica;

        // Girar suavemente hacia la dirección del movimiento
        if (direccionDeseada.sqrMagnitude > 0.01f)
        {
            Quaternion rotacionObjetivo = Quaternion.LookRotation(direccionDeseada, Vector3.up);
            if (modeloVisualDoctor != null)
            {
                modeloVisualDoctor.rotation = Quaternion.Slerp(modeloVisualDoctor.rotation, rotacionObjetivo, velocidadGiro * Time.deltaTime);
            }
            else
            {
                transform.rotation = Quaternion.Slerp(transform.rotation, rotacionObjetivo, velocidadGiro * Time.deltaTime);
            }
        }
    }

    private void ProcesarSalto()
    {
        esperaSalto -= Time.deltaTime;

        if (Input.GetButtonDown("Jump") && enElSuelo && esperaSalto <= 0f)
        {
            Vector3 velocidad = rb.linearVelocity;
            velocidad.y = 0f;
            rb.linearVelocity = velocidad;
            rb.AddForce(Vector3.up * fuerzaSalto, ForceMode.Impulse);
            esperaSalto = 0.25f;
            if (animador != null) animador.SetTrigger("Saltar");
        }
    }

    private void ActualizarAnimador()
    {
        if (animador == null) return;

        Vector3 velocidad = rb.linearVelocity;
        velocidad.y = 0f;
        animador.SetFloat("Velocidad", velocidad.magnitude);
        animador.SetBool("EnSuelo", enElSuelo);
    }

    private void ProcesarEntradaPausa()
    {
        if (Input.GetKeyDown(KeyCode.Escape) && Time.frameCount != frameMenuCerrado)
        {
            if (juegoPausado)
            {
                ReanudarJuego();
            }
            else
            {
                PausarJuego();
            }
        }
    }

    public void PausarJuego()
    {
        if (panelPausa != null)
        {
            panelPausa.SetActive(true);
        }

        Time.timeScale = 0f;
        juegoPausado = true;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void ReanudarJuego()
    {
        if (panelPausa != null)
        {
            panelPausa.SetActive(false);
        }

        Time.timeScale = 1f;
        juegoPausado = false;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    public void SalirJuego()
    {
        Application.Quit();
    }

    public void VolverAlHub()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("Hub");
    }

    public void RecolectarItemSalud(ItemRecolectable item)
    {
        if (item == null) return;

        switch (item.tipoItem)
        {
            case TipoItemSalud.Jeringa:
                jeringasRecolectadas += item.valorSuministro;
                break;
            case TipoItemSalud.EquipajeSalud:
                equipajesRecolectados += item.valorSuministro;
                break;
            case TipoItemSalud.Botiquin:
                botiquinesRecolectados += item.valorSuministro;
                Curar(item.cantidadCuracion);
                break;
            case TipoItemSalud.Medicina:
                Curar(item.cantidadCuracion);
                break;
        }

        cantidadSuministros = jeringasRecolectadas + equipajesRecolectados + botiquinesRecolectados;
        ActualizarHUD();
    }

    public void Curar(int cantidad)
    {
        vidaActual = Mathf.Clamp(vidaActual + cantidad, 0, VidaTotal);
        ActualizarBarraVida();

        if (AudioManager.Instancia != null)
        {
            AudioManager.Instancia.PlayCurar();
        }
    }

    public void RecibirDanio(int cantidad)
    {
        if (tiempoInvulnerable > 0f) return;

        vidaActual = Mathf.Max(0, vidaActual - cantidad);
        tiempoInvulnerable = TIEMPO_INVULNERABILIDAD_BASE;
        ActualizarBarraVida();

        if (AudioManager.Instancia != null)
        {
            AudioManager.Instancia.PlayDanio();
        }

        if (vidaActual <= 0)
        {
            VolverAlInicio();
        }
    }

    private void VolverAlInicio()
    {
        vidaActual = VidaTotal;
        ActualizarBarraVida();
        rb.linearVelocity = Vector3.zero;
        rb.position = puntoInicio;
        transform.position = puntoInicio;
    }

    /// <summary>Lanza al jugador en una dirección, por ejemplo al recibir un golpe.</summary>
    public void Empujar(Vector3 velocidad)
    {
        finEmpuje = Time.time + 0.25f;
        rb.linearVelocity = velocidad + Vector3.up * 2f;
    }

    /// <summary>Descuenta lo entregado a un paciente y ajusta lo que falta conseguir.</summary>
    public void GastarSuministros(int jeringas, int equipajes)
    {
        jeringasRecolectadas -= jeringas;
        equipajesRecolectados -= equipajes;
        jeringasMeta -= jeringas;
        equipajesMeta -= equipajes;
        ActualizarHUD();
    }

    /// <summary>Frena al personaje; se usa al abrir menús porque el Rigidbody conserva su velocidad.</summary>
    public void Detener()
    {
        if (rb == null) rb = GetComponent<Rigidbody>();
        rb.linearVelocity = Vector3.zero;
        if (animador != null) animador.SetFloat("Velocidad", 0f);
    }

    /// <summary>Recibe las bonificaciones de los objetos equipados.</summary>
    public void AplicarBonos(int vida, float velocidad)
    {
        vidaExtra = vida;
        velocidadExtra = velocidad;
        vidaActual = Mathf.Min(vidaActual, VidaTotal);
        ActualizarBarraVida();
    }

    private void ActualizarBarraVida()
    {
        if (barraDeVida == null) return;

        barraDeVida.maxValue = VidaTotal;
        barraDeVida.value = vidaActual;
    }

    public void ActualizarHUD()
    {
        if (textoSuministros != null)
        {
            textoSuministros.text = $"Suministros: {cantidadSuministros}";
        }

        if (textoJeringas != null)
        {
            textoJeringas.text = $"Jeringas  {jeringasRecolectadas} / {jeringasMeta}";
        }

        if (textoEquipajes != null)
        {
            textoEquipajes.text = $"Equipajes  {equipajesRecolectados} / {equipajesMeta}";
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Peligro"))
        {
            RecibirDanio(20);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        // Compatibilidad con objetos antiguos con tag Suministro
        if (other.CompareTag("Suministro") && other.GetComponent<ItemRecolectable>() == null)
        {
            cantidadSuministros++;
            jeringasRecolectadas++;
            ActualizarHUD();
            Destroy(other.gameObject);
            if (AudioManager.Instancia != null) AudioManager.Instancia.PlayJeringa();
        }
    }
}
