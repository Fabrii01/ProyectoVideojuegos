using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

/// <summary>
/// Controlador principal del jugador con soporte para el modelo 3D del Doctor,
/// animación procedural de caminata/giro, recolección especializada de jeringas y equipajes de salud,
/// y menú de pausa integrado.
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

    [Header("Estadísticas de Salud")]
    public int vidaMaxima = 100;
    public int vidaActual = 100;
    public Slider barraDeVida;

    [Header("Inventario Médico")]
    public int cantidadSuministros = 0; // Compatibilidad con código previo
    public int jeringasRecolectadas = 0;
    public int equipajesRecolectados = 0;
    public int botiquinesRecolectados = 0;

    [Header("Interfaz de Usuario (HUD)")]
    public TextMeshProUGUI textoSuministros;
    public TextMeshProUGUI textoJeringas;
    public TextMeshProUGUI textoEquipajes;
    public TextMeshProUGUI textoObjetivo;
    public GameObject panelPausa;

    [Header("Cámara")]
    public Camera camaraJugador;

    private Rigidbody rb;
    private bool enElSuelo = true;
    private bool juegoPausado = false;
    private float tiempoInvulnerable = 0f;
    private const float TIEMPO_INVULNERABILIDAD_BASE = 0.8f;

    // Variables de animación procedural
    private float pasoProceduralTimer = 0f;
    private Vector3 escalaOriginalModelo = Vector3.one;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true; // Evitar que caiga de lado por físicas
    }

    private void Start()
    {
        vidaActual = vidaMaxima;

        if (barraDeVida != null)
        {
            barraDeVida.maxValue = vidaMaxima;
            barraDeVida.value = vidaActual;
        }

        if (panelPausa != null)
        {
            panelPausa.SetActive(false);
        }

        if (camaraJugador == null)
        {
            camaraJugador = Camera.main;
        }

        // Buscar modelo visual si no se asignó manualmente
        if (modeloVisualDoctor == null)
        {
            Transform tDoctor = transform.Find("Doctor");
            if (tDoctor != null)
            {
                modeloVisualDoctor = tDoctor;
            }
        }

        if (modeloVisualDoctor != null)
        {
            escalaOriginalModelo = modeloVisualDoctor.localScale;
        }

        ActualizarHUD();
    }

    private void Update()
    {
        ProcesarEntradaPausa();

        if (juegoPausado) return;

        ProcesarMovimiento();
        ProcesarSalto();
        ActualizarAnimacionProcedural();

        if (tiempoInvulnerable > 0f)
        {
            tiempoInvulnerable -= Time.deltaTime;
        }
    }

    private void ProcesarMovimiento()
    {
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
        float velActual = estaCorriendo ? velocidadCorrer : velocidadMovimiento;

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
        if (Input.GetButtonDown("Jump") && enElSuelo)
        {
            rb.AddForce(Vector3.up * fuerzaSalto, ForceMode.Impulse);
            enElSuelo = false;
        }
    }

    private void ActualizarAnimacionProcedural()
    {
        if (modeloVisualDoctor == null) return;

        Vector3 velHorizontal = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);
        float rapidez = velHorizontal.magnitude;

        if (rapidez > 0.5f && enElSuelo)
        {
            pasoProceduralTimer += Time.deltaTime * rapidez * 2.2f;
            float balanceo = Mathf.Sin(pasoProceduralTimer) * 0.05f;
            float inclinacion = Mathf.Cos(pasoProceduralTimer * 0.5f) * 2.5f;

            modeloVisualDoctor.localPosition = new Vector3(0f, Mathf.Abs(balanceo) * 0.1f, 0f);
            modeloVisualDoctor.localRotation = Quaternion.Euler(inclinacion, modeloVisualDoctor.localEulerAngles.y, balanceo * 10f);
        }
        else
        {
            // Respiración / Balanceo sutil en espera
            float respiracion = Mathf.Sin(Time.time * 2f) * 0.015f;
            modeloVisualDoctor.localPosition = new Vector3(0f, respiracion, 0f);
            modeloVisualDoctor.localRotation = Quaternion.Euler(0f, modeloVisualDoctor.localEulerAngles.y, 0f);
        }
    }

    private void ProcesarEntradaPausa()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
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
        vidaActual = Mathf.Clamp(vidaActual + cantidad, 0, vidaMaxima);
        if (barraDeVida != null)
        {
            barraDeVida.value = vidaActual;
        }

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

        if (barraDeVida != null)
        {
            barraDeVida.value = vidaActual;
        }

        if (AudioManager.Instancia != null)
        {
            AudioManager.Instancia.PlayDanio();
        }

        if (vidaActual <= 0)
        {
            // Derrota / Reiniciar posición
            vidaActual = vidaMaxima;
            if (barraDeVida != null) barraDeVida.value = vidaActual;
            transform.position = new Vector3(500f, 2f, 498f);
        }
    }

    public void ActualizarHUD()
    {
        if (textoSuministros != null)
        {
            textoSuministros.text = $"Suministros Totales: {cantidadSuministros}";
        }

        if (textoJeringas != null)
        {
            textoJeringas.text = $"Jeringas: {jeringasRecolectadas}";
        }

        if (textoEquipajes != null)
        {
            textoEquipajes.text = $"Equipajes de Salud: {equipajesRecolectados}";
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.name.Contains("Terrain") ||
            collision.gameObject.name.Contains("Suelo") ||
            collision.gameObject.name.Contains("asphalt") ||
            collision.gameObject.name.Contains("Floor"))
        {
            enElSuelo = true;
        }

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
