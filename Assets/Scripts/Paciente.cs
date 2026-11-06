using TMPro;
using UnityEngine;

/// <summary>
/// Paciente que necesita jeringas y equipajes de salud. Muestra lo que le falta
/// y avisa a GestorMision cuando queda atendido.
/// </summary>
public class Paciente : MonoBehaviour
{
    [Header("Requisitos Médicos")]
    public int jeringasRequeridas = 2;
    public int equipajesRequeridos = 1;

    [Header("Interfaz y efectos")]
    public TextMeshPro textoEstado;
    public Light luzEstado;
    public Color colorCritico = new Color(1f, 0.2f, 0.2f);
    public Color colorCurado = new Color(0.2f, 1f, 0.3f);

    private bool curado;

    public bool Curado => curado;

    private void Start()
    {
        if (luzEstado != null) luzEstado.color = colorCritico;
        MostrarNecesidades();
    }

    private void Update()
    {
        if (!curado && luzEstado != null)
        {
            luzEstado.intensity = 2f + 1.6f * Mathf.Sin(Time.time * 4f);
        }
    }

    private void OnTriggerEnter(Collider otro)
    {
        if (curado || !otro.CompareTag("Player")) return;

        PlayerController jugador = otro.GetComponent<PlayerController>();
        if (jugador == null) return;

        if (CumpleRequisitos(jugador))
        {
            Curar(jugador);
            return;
        }

        int jeringas = Mathf.Max(0, jeringasRequeridas - jugador.jeringasRecolectadas);
        int equipajes = Mathf.Max(0, equipajesRequeridos - jugador.equipajesRecolectados);

        if (textoEstado != null) textoEstado.text = $"Faltan suministros\nJeringas: {jeringas}  Equipajes: {equipajes}";
        if (AudioManager.Instancia != null) AudioManager.Instancia.PlayClick();
    }

    private void OnTriggerStay(Collider otro)
    {
        // Por si el jugador entró a la zona y recogió lo que faltaba sin salir de ella.
        if (curado || !otro.CompareTag("Player")) return;

        PlayerController jugador = otro.GetComponent<PlayerController>();
        if (jugador != null && CumpleRequisitos(jugador)) Curar(jugador);
    }

    private void OnTriggerExit(Collider otro)
    {
        if (!curado && otro.CompareTag("Player")) MostrarNecesidades();
    }

    private bool CumpleRequisitos(PlayerController jugador)
    {
        return jugador.jeringasRecolectadas >= jeringasRequeridas &&
               jugador.equipajesRecolectados >= equipajesRequeridos;
    }

    private void Curar(PlayerController jugador)
    {
        curado = true;
        jugador.GastarSuministros(jeringasRequeridas, equipajesRequeridos);

        if (luzEstado != null)
        {
            luzEstado.color = colorCurado;
            luzEstado.intensity = 3f;
        }

        if (textoEstado != null) textoEstado.text = "ESTABILIZADO\nPaciente fuera de peligro";
        if (AudioManager.Instancia != null) AudioManager.Instancia.PlayCurar();
        if (GestorMision.Instancia != null) GestorMision.Instancia.PacienteAtendido();
    }

    private void MostrarNecesidades()
    {
        if (textoEstado != null)
        {
            textoEstado.text = $"PACIENTE HERIDO\nNecesita {jeringasRequeridas} jeringas y {equipajesRequeridos} equipajes";
        }
    }
}
