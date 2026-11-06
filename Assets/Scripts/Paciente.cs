using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

/// <summary>
/// Paciente que necesita jeringas y equipajes de salud. Muestra lo que le falta
/// y, al ser atendido, da la recompensa y termina la misión.
/// </summary>
public class Paciente : MonoBehaviour
{
    [Header("Requisitos Médicos")]
    public int jeringasRequeridas = 3;
    public int equipajesRequeridos = 2;

    [Header("Recompensa")]
    public int recompensaMonedas = 30;

    [Header("Elementos de Interfaz")]
    public TextMeshPro textoEstado;
    public GameObject panelVictoria;
    public TextMeshProUGUI textoMensajeVictoria;

    [Header("Efectos Visuales")]
    public Light luzEstado;
    public Color colorCritico = new Color(1f, 0.2f, 0.2f);
    public Color colorCurado = new Color(0.2f, 1f, 0.3f);

    private bool pacienteCurado = false;

    private void Start()
    {
        if (panelVictoria != null)
        {
            panelVictoria.SetActive(false);
        }

        if (luzEstado != null)
        {
            luzEstado.color = colorCritico;
        }

        ActualizarTextoEstado(null);
    }

    private void Update()
    {
        if (!pacienteCurado && luzEstado != null)
        {
            // Luz de emergencia pulsante
            luzEstado.intensity = 1.0f + 0.8f * Mathf.Sin(Time.time * 4f);
        }
    }

    private void OnTriggerEnter(Collider otro)
    {
        if (pacienteCurado) return;

        if (otro.CompareTag("Player"))
        {
            PlayerController jugador = otro.GetComponent<PlayerController>();
            if (jugador != null)
            {
                if (CumpleRequisitos(jugador))
                {
                    CurarPaciente(jugador);
                }
                else
                {
                    int jeringasFaltantes = Mathf.Max(0, jeringasRequeridas - jugador.jeringasRecolectadas);
                    int equipajesFaltantes = Mathf.Max(0, equipajesRequeridos - jugador.equipajesRecolectados);

                    if (textoEstado != null)
                    {
                        textoEstado.text = $"Faltan suministros\nJeringas: {jeringasFaltantes}  Equipajes: {equipajesFaltantes}";
                    }

                    if (AudioManager.Instancia != null)
                    {
                        AudioManager.Instancia.PlayClick();
                    }
                }
            }
        }
    }

    private void OnTriggerStay(Collider otro)
    {
        // Por si el jugador entró a la zona y recogió lo que faltaba sin salir de ella.
        if (pacienteCurado || !otro.CompareTag("Player")) return;

        PlayerController jugador = otro.GetComponent<PlayerController>();
        if (jugador != null && CumpleRequisitos(jugador)) CurarPaciente(jugador);
    }

    private bool CumpleRequisitos(PlayerController jugador)
    {
        return jugador.jeringasRecolectadas >= jeringasRequeridas &&
               jugador.equipajesRecolectados >= equipajesRequeridos;
    }

    private void OnTriggerExit(Collider otro)
    {
        if (!pacienteCurado && otro.CompareTag("Player"))
        {
            ActualizarTextoEstado(null);
        }
    }

    private void CurarPaciente(PlayerController jugador)
    {
        pacienteCurado = true;
        DatosJugador.AgregarMonedas(recompensaMonedas);

        if (luzEstado != null)
        {
            luzEstado.color = colorCurado;
            luzEstado.intensity = 2.5f;
        }

        if (textoEstado != null)
        {
            textoEstado.text = "ESTABILIZADO\nPaciente fuera de peligro";
        }

        if (AudioManager.Instancia != null)
        {
            AudioManager.Instancia.PlayVictoria();
        }

        if (panelVictoria != null)
        {
            panelVictoria.SetActive(true);
            jugador.Detener();
            jugador.enabled = false;
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            Time.timeScale = 0f;

            if (textoMensajeVictoria != null)
            {
                textoMensajeVictoria.text = $"El paciente está estable y listo para el traslado.\n\nEntregaste {jeringasRequeridas} jeringas y {equipajesRequeridos} equipajes de salud.\nRecompensa: +{recompensaMonedas} monedas.";
            }
        }
    }

    private void ActualizarTextoEstado(PlayerController jugador)
    {
        if (textoEstado != null)
        {
            textoEstado.text = $"PACIENTE HERIDO\nNecesita {jeringasRequeridas} jeringas y {equipajesRequeridos} equipajes";
        }
    }

    public void VolverAlHub()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("Hub");
    }
}
