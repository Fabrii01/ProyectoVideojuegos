using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Dirige la misión de un nivel: da el aviso inicial, cuenta los pacientes atendidos
/// y, al terminar, entrega la recompensa, desbloquea el nivel siguiente y muestra lo aprendido.
/// </summary>
public class GestorMision : MonoBehaviour
{
    public static GestorMision Instancia { get; private set; }

    public int numeroNivel = 1;
    public int recompensa = 60;
    public PlayerController jugador;

    [Header("Textos")]
    public string nombreCentral = "Central de emergencias";
    [TextArea(2, 3)] public string[] avisosIniciales;
    [TextArea(2, 3)] public string leccion;

    [Header("Interfaz")]
    public TextMeshProUGUI textoPacientes;
    public GameObject panelVictoria;
    public TextMeshProUGUI textoVictoria;

    private int pacientesTotales;
    private int pacientesAtendidos;

    private void Awake()
    {
        Instancia = this;
    }

    private void OnDestroy()
    {
        if (Instancia == this) Instancia = null;
    }

    private void Start()
    {
        int jeringas = 0;
        int equipajes = 0;

        Paciente[] pacientes = FindObjectsByType<Paciente>(FindObjectsInactive.Exclude);
        foreach (Paciente paciente in pacientes)
        {
            jeringas += paciente.jeringasRequeridas;
            equipajes += paciente.equipajesRequeridos;
        }

        pacientesTotales = pacientes.Length;
        jugador.jeringasMeta = jeringas;
        jugador.equipajesMeta = equipajes;
        jugador.ActualizarHUD();
        ActualizarContador();

        if (panelVictoria != null) panelVictoria.SetActive(false);
        StartCoroutine(MostrarAvisos());
    }

    private IEnumerator MostrarAvisos()
    {
        yield return new WaitForSeconds(0.8f);

        foreach (string aviso in avisosIniciales)
        {
            if (DialogoUI.Instancia != null) DialogoUI.Instancia.Mostrar(this, nombreCentral, aviso);
            yield return new WaitForSeconds(5.5f);
        }

        if (DialogoUI.Instancia != null) DialogoUI.Instancia.Ocultar(this);
    }

    private void ActualizarContador()
    {
        if (textoPacientes != null) textoPacientes.text = $"Pacientes  {pacientesAtendidos} / {pacientesTotales}";
    }

    public void PacienteAtendido()
    {
        pacientesAtendidos++;
        ActualizarContador();

        if (pacientesAtendidos >= pacientesTotales) Terminar();
    }

    private void Terminar()
    {
        StopAllCoroutines();
        if (DialogoUI.Instancia != null) DialogoUI.Instancia.Ocultar(this);

        DatosJugador.AgregarMonedas(recompensa);
        DatosJugador.CompletarNivel(numeroNivel);

        if (AudioManager.Instancia != null)
        {
            AudioManager.Instancia.DetenerMusica();
            AudioManager.Instancia.PlayVictoria();
        }

        if (textoVictoria != null)
        {
            textoVictoria.text = $"Atendiste a {pacientesTotales} pacientes. Recompensa: +{recompensa} monedas.\n\n<b>Recuerda:</b> {leccion}";
        }

        panelVictoria.SetActive(true);
        jugador.Detener();
        jugador.enabled = false;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        Time.timeScale = 0f;
    }

    public void VolverAlHub()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("Hub");
    }
}
