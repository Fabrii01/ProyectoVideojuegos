using TMPro;
using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// Saqueador que patrulla entre puntos, persigue al jugador si lo ve y lo golpea si lo alcanza.
/// Vuelve a su ruta cuando el jugador se aleja o se esconde.
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
    public float fuerzaEmpuje = 7f;

    [Header("Avisos")]
    public TextMeshPro indicadorAlerta;
    public Light luzEstado;
    public Color colorCalma = new Color(1f, 0.75f, 0.2f);
    public Color colorAlerta = new Color(1f, 0.12f, 0.08f);

    public bool EstaPersiguiendo => estado == Estado.Persecucion;

    private NavMeshAgent agente;
    private Transform objetivo;
    private PlayerController jugador;
    private Estado estado = Estado.Patrulla;
    private int indicePunto;
    private float finPausa;
    private float ultimaVezVisto;
    private float proximoGolpe;

    private void Start()
    {
        agente = GetComponent<NavMeshAgent>();

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
        if (objetivo == null || !agente.isOnNavMesh) return;

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
            Vector3 direccion = (objetivo.position - transform.position).normalized;
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
