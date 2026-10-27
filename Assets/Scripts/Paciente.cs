using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

/// <summary>
/// Representa a un paciente que requiere suministros de salud específicos (jeringas y equipajes).
/// Muestra su estado en tiempo real y desencadena la victoria cuando es atendido con éxito.
/// </summary>
public class Paciente : MonoBehaviour
{
    [Header("Requisitos Médicos")]
    public int suministrosRequeridos = 5; // Compatibilidad
    public int jeringasRequeridas = 3;
    public int equipajesRequeridos = 2;

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
                bool cumpleRequisitos = (jugador.jeringasRecolectadas >= jeringasRequeridas &&
                                         jugador.equipajesRecolectados >= equipajesRequeridos) ||
                                        (jugador.cantidadSuministros >= suministrosRequeridos);

                if (cumpleRequisitos)
                {
                    CurarPaciente(jugador);
                }
                else
                {
                    int jeringasFaltantes = Mathf.Max(0, jeringasRequeridas - jugador.jeringasRecolectadas);
                    int equipajesFaltantes = Mathf.Max(0, equipajesRequeridos - jugador.equipajesRecolectados);

                    if (textoEstado != null)
                    {
                        textoEstado.text = $"¡Faltan suministros!\nJeringas: {jeringasFaltantes} | Equipajes: {equipajesFaltantes}";
                    }

                    if (AudioManager.Instancia != null)
                    {
                        AudioManager.Instancia.PlayClick();
                    }
                }
            }
        }
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

        if (luzEstado != null)
        {
            luzEstado.color = colorCurado;
            luzEstado.intensity = 2.5f;
        }

        if (textoEstado != null)
        {
            textoEstado.text = "¡ESTABILIZADO!\nPaciente fuera de peligro";
        }

        if (AudioManager.Instancia != null)
        {
            AudioManager.Instancia.PlayVictoria();
        }

        if (panelVictoria != null)
        {
            panelVictoria.SetActive(true);
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;

            if (textoMensajeVictoria != null)
            {
                textoMensajeVictoria.text = $"¡MISIÓN CUMPLIDA DOCTOR!\n\nHas recolectado y suministrado:\n- {jugador.jeringasRecolectadas} Jeringas\n- {jugador.equipajesRecolectados} Equipajes de Salud\n\nEl paciente ha sido salvado exitosamente.";
            }
        }
    }

    private void ActualizarTextoEstado(PlayerController jugador)
    {
        if (textoEstado != null)
        {
            textoEstado.text = $"[PACIENTE EN ESPERA]\nNecesita: {jeringasRequeridas} Jeringas y {equipajesRequeridos} Equipajes";
        }
    }

    public void VolverAlHub()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("Hub");
    }
}
