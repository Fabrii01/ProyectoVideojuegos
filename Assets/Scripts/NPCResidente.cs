using TMPro;
using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// Persona del Hub: pasea, trabaja o se queda en su sitio. Si el jugador se acerca,
/// se detiene, lo mira y le da un consejo de salud en el cuadro de diálogo.
/// </summary>
[RequireComponent(typeof(NavMeshAgent))]
public class NPCResidente : MonoBehaviour
{
    public string nombre = "Vecino";
    public Transform[] puntos;
    public bool pasea = true;
    public bool trabajando;
    public Animator animador;
    public TextMeshPro marcaCharla;
    [TextArea(1, 3)] public string[] frases;
    public float radioCharla = 2.8f;
    public float pausaMinima = 1.5f;
    public float pausaMaxima = 6f;

    private NavMeshAgent agente;
    private Transform jugador;
    private float finPausa;
    private bool hablando;
    private Quaternion miradaInicial;

    private void Start()
    {
        agente = GetComponent<NavMeshAgent>();
        miradaInicial = transform.rotation;

        GameObject objetoJugador = GameObject.FindGameObjectWithTag("Player");
        if (objetoJugador != null) jugador = objetoJugador.transform;

        if (marcaCharla != null) marcaCharla.gameObject.SetActive(false);
        if (animador != null) animador.SetBool("Trabajando", trabajando);
        if (pasea) IrAOtroPunto();
    }

    private void Update()
    {
        if (animador != null) animador.SetFloat("Velocidad", agente.velocity.magnitude);
        if (jugador == null || !agente.isOnNavMesh) return;

        bool cerca = Vector3.Distance(transform.position, jugador.position) <= radioCharla;
        if (cerca != hablando) CambiarCharla(cerca);

        if (hablando)
        {
            MirarHacia(jugador.position);
            return;
        }

        if (pasea) Pasear();
        else transform.rotation = Quaternion.Slerp(transform.rotation, miradaInicial, Time.deltaTime * 3f);
    }

    private void CambiarCharla(bool iniciar)
    {
        hablando = iniciar;
        agente.isStopped = iniciar;

        if (marcaCharla != null) marcaCharla.gameObject.SetActive(iniciar);
        if (animador != null) animador.SetBool("Trabajando", trabajando && !iniciar);

        if (DialogoUI.Instancia == null) return;

        if (iniciar && frases != null && frases.Length > 0)
        {
            DialogoUI.Instancia.Mostrar(this, nombre, frases[Random.Range(0, frases.Length)]);
        }
        else
        {
            DialogoUI.Instancia.Ocultar(this);
        }
    }

    private void Pasear()
    {
        if (agente.pathPending || agente.remainingDistance > 0.5f) return;

        if (finPausa == 0f)
        {
            finPausa = Time.time + Random.Range(pausaMinima, pausaMaxima);
        }
        else if (Time.time >= finPausa)
        {
            finPausa = 0f;
            IrAOtroPunto();
        }
    }

    private void IrAOtroPunto()
    {
        if (puntos == null || puntos.Length == 0 || !agente.isOnNavMesh) return;
        agente.SetDestination(puntos[Random.Range(0, puntos.Length)].position);
    }

    private void MirarHacia(Vector3 objetivo)
    {
        Vector3 direccion = objetivo - transform.position;
        direccion.y = 0f;
        if (direccion.sqrMagnitude < 0.01f) return;

        transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(direccion), Time.deltaTime * 6f);
    }
}
