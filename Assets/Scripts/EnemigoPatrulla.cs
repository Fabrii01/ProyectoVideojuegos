using TMPro;
using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// Saqueador que patrulla entre puntos, persigue al jugador si lo ve y lo golpea si lo alcanza.
/// Vuelve a su ruta cuando el jugador se aleja o se esconde. Con un arma se le puede derrotar.
/// </summary>
[RequireComponent(typeof(NavMeshAgent))]
public class EnemigoPatrulla : MonoBehaviour
{
    private enum Estado { Patrulla, Persecucion }

    [Header("Patrulla")]
    public Transform[] puntos;
    public float velocidadPatrulla = 2.4f;
    public float pausaEnPunto = 1.2f;

    [Header("Detección")]
    public float rangoVision = 11f;
    public float anguloVision = 120f;
    public float rangoOido = 3f;
    public float rangoPerderJugador = 17f;
    public float tiempoOlvido = 2.5f;

    [Header("Ataque")]
    public float velocidadPersecucion = 5.2f;
    public float radioGolpe = 1.5f;
    public int danio = 15;
    public float esperaEntreGolpes = 1.3f;
    public float fuerzaEmpuje = 3.5f;

    [Header("Salud")]
    public int vidaMaxima = 100;
    public GameObject barraVida;
    public Transform rellenoVida;
    public GameObject premio;
    public int premiosAlCaer = 3;

    [Header("Avisos")]
    public Animator animador;
    public TextMeshPro indicadorAlerta;
    public Light luzEstado;
    public Color colorCalma = new Color(1f, 0.75f, 0.2f);
    public Color colorAlerta = new Color(1f, 0.12f, 0.08f);

    public bool EstaPersiguiendo => !EstaDerrotado && estado == Estado.Persecucion;
    public bool EstaDerrotado => vida <= 0;

    private NavMeshAgent agente;
    private Transform objetivo;
    private PlayerController jugador;
    private Estado estado = Estado.Patrulla;
    private int indicePunto;
    private float finPausa;
    private float ultimaVezVisto;
    private float proximoGolpe;
    private int vida;

    private void Start()
    {
        agente = GetComponent<NavMeshAgent>();
        vida = vidaMaxima;
        if (barraVida != null) barraVida.SetActive(false);

        GameObject objetoJugador = GameObject.FindGameObjectWithTag("Player");
        if (objetoJugador != null)
        {
            objetivo = objetoJugador.transform;
            jugador = objetoJugador.GetComponent<PlayerController>();
        }

        CambiarEstado(Estado.Patrulla, false);
        IrAlPunto();
    }

    private void Update()
    {
        if (EstaDerrotado || objetivo == null || !agente.isOnNavMesh) return;

        if (animador != null) animador.SetFloat("Velocidad", agente.velocity.magnitude);

        bool loVe = PuedeVerJugador();
        if (loVe) ultimaVezVisto = Time.time;

        if (estado == Estado.Patrulla)
        {
            ActualizarPatrulla();
            if (loVe) CambiarEstado(Estado.Persecucion, true);
        }
        else
        {
            ActualizarPersecucion();
        }
    }

    private void ActualizarPatrulla()
    {
        if (puntos == null || puntos.Length == 0) return;
        if (agente.pathPending || agente.remainingDistance > 0.4f) return;

        if (finPausa == 0f)
        {
            finPausa = Time.time + pausaEnPunto;
        }
        else if (Time.time >= finPausa)
        {
            finPausa = 0f;
            indicePunto = (indicePunto + 1) % puntos.Length;
            IrAlPunto();
        }
    }

    private void ActualizarPersecucion()
    {
        agente.SetDestination(objetivo.position);

        float distancia = Vector3.Distance(transform.position, objetivo.position);

        if (distancia <= radioGolpe && Time.time >= proximoGolpe)
        {
            proximoGolpe = Time.time + esperaEntreGolpes;
            // Solo en horizontal: el centro del jugador está más alto que los pies del enemigo.
            Vector3 direccion = objetivo.position - transform.position;
            direccion.y = 0f;
            direccion.Normalize();
            if (animador != null) animador.SetTrigger("Golpear");
            jugador.RecibirDanio(danio);
            jugador.Empujar(direccion * fuerzaEmpuje);
        }

        bool muyLejos = distancia > rangoPerderJugador;
        bool loPerdio = Time.time - ultimaVezVisto > tiempoOlvido;
        if (muyLejos || loPerdio)
        {
            CambiarEstado(Estado.Patrulla, false);
            IrAlPunto();
        }
    }

    private bool PuedeVerJugador()
    {
        Vector3 ojos = transform.position + Vector3.up * 1.5f;
        Vector3 pecho = objetivo.position + Vector3.up * 0.4f;
        Vector3 hacia = pecho - ojos;
        float distancia = hacia.magnitude;

        if (distancia <= rangoOido) return true;
        if (distancia > rangoVision) return false;
        if (Vector3.Angle(transform.forward, hacia) > anguloVision * 0.5f) return false;

        // Si algo tapa la línea de visión, el jugador puede esconderse detrás.
        if (Physics.Raycast(ojos, hacia.normalized, out RaycastHit golpe, distancia, ~0, QueryTriggerInteraction.Ignore))
        {
            return golpe.transform == objetivo || golpe.transform.IsChildOf(objetivo);
        }
        return true;
    }

    private void IrAlPunto()
    {
        if (puntos == null || puntos.Length == 0 || !agente.isOnNavMesh) return;
        agente.SetDestination(puntos[indicePunto].position);
    }

    private void CambiarEstado(Estado nuevo, bool sonar)
    {
        estado = nuevo;
        bool alerta = nuevo == Estado.Persecucion;

        agente.speed = alerta ? velocidadPersecucion : velocidadPatrulla;
        finPausa = 0f;

        if (indicadorAlerta != null) indicadorAlerta.gameObject.SetActive(alerta);
        if (luzEstado != null) luzEstado.color = alerta ? colorAlerta : colorCalma;
        if (sonar && AudioManager.Instancia != null) AudioManager.Instancia.PlayAlerta();
        if (sonar && AvisoCentral.Instancia != null) AvisoCentral.Instancia.Mostrar("¡ENEMIGO ENCONTRADO!");
    }

    /// <summary>Recibe un golpe del jugador: lo empuja, lo pone en alerta y le quita salud.</summary>
    public void RecibirGolpe(int cantidad, Vector3 direccion)
    {
        if (EstaDerrotado) return;

        vida -= cantidad;
        proximoGolpe = Time.time + 0.8f;
        ultimaVezVisto = Time.time;

        if (barraVida != null) barraVida.SetActive(true);
        if (rellenoVida != null)
        {
            float parte = Mathf.Clamp01((float)vida / vidaMaxima);
            rellenoVida.localScale = new Vector3(parte, 1f, 1f);
            rellenoVida.localPosition = new Vector3((parte - 1f) * 0.5f, 0f, -0.01f);
        }

        if (EstaDerrotado)
        {
            Caer();
            return;
        }

        if (agente.isOnNavMesh) agente.Move(direccion.normalized * 0.7f);
        if (estado != Estado.Persecucion) CambiarEstado(Estado.Persecucion, true);
    }

    private void Caer()
    {
        agente.enabled = false;
        GetComponent<Collider>().enabled = false;

        if (barraVida != null) barraVida.SetActive(false);
        if (indicadorAlerta != null) indicadorAlerta.gameObject.SetActive(false);
        if (luzEstado != null) luzEstado.enabled = false;
        if (animador != null) animador.SetTrigger("Morir");

        for (int i = 0; i < premiosAlCaer && premio != null; i++)
        {
            float angulo = i * Mathf.PI * 2f / premiosAlCaer;
            Vector3 lado = new Vector3(Mathf.Cos(angulo), 0f, Mathf.Sin(angulo)) * 1.2f;
            Instantiate(premio, transform.position + lado + Vector3.up * 0.9f, Quaternion.identity);
        }

        Destroy(gameObject, 3f);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, rangoVision);

        if (puntos == null) return;
        Gizmos.color = Color.cyan;
        for (int i = 0; i < puntos.Length; i++)
        {
            if (puntos[i] == null || puntos[(i + 1) % puntos.Length] == null) continue;
            Gizmos.DrawLine(puntos[i].position, puntos[(i + 1) % puntos.Length].position);
        }
    }
}
