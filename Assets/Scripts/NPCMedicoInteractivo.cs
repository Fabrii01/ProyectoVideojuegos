using UnityEngine;

/// <summary>
/// Gestiona la interacción con el Médico Jefe en el Hub para desplegar misiones y niveles.
/// </summary>
public class NPCMedicoInteractivo : MonoBehaviour
{
    [Header("Elementos de la interfaz")]
    public GameObject textoInteraccion;
    public GameObject panelNiveles;

    [Header("Jugador")]
    public Behaviour controladorMovimiento;

    private bool jugadorCerca;
    private bool menuAbierto;

    private CursorLockMode bloqueoCursorOriginal;
    private bool cursorVisibleOriginal;

    private void Start()
    {
        if (textoInteraccion != null) textoInteraccion.SetActive(false);
        if (panelNiveles != null) panelNiveles.SetActive(false);
    }

    private void Update()
    {
        if (jugadorCerca && !menuAbierto && Input.GetKeyDown(KeyCode.E))
        {
            AbrirMenu();
        }

        if (menuAbierto && Input.GetKeyDown(KeyCode.Escape))
        {
            CerrarMenu();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            jugadorCerca = true;
            if (!menuAbierto && textoInteraccion != null)
            {
                textoInteraccion.SetActive(true);
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            jugadorCerca = false;
            if (textoInteraccion != null) textoInteraccion.SetActive(false);
            if (menuAbierto) CerrarMenu();
        }
    }

    public void AbrirMenu()
    {
        menuAbierto = true;

        bloqueoCursorOriginal = Cursor.lockState;
        cursorVisibleOriginal = Cursor.visible;

        if (textoInteraccion != null) textoInteraccion.SetActive(false);
        if (panelNiveles != null) panelNiveles.SetActive(true);

        if (controladorMovimiento != null)
        {
            controladorMovimiento.enabled = false;
        }

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        if (AudioManager.Instancia != null)
        {
            AudioManager.Instancia.PlayClick();
        }
    }

    public void CerrarMenu()
    {
        menuAbierto = false;
        if (panelNiveles != null) panelNiveles.SetActive(false);

        if (controladorMovimiento != null)
        {
            controladorMovimiento.enabled = true;
        }

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        if (jugadorCerca && textoInteraccion != null)
        {
            textoInteraccion.SetActive(true);
        }

        if (AudioManager.Instancia != null)
        {
            AudioManager.Instancia.PlayClick();
        }
    }
}
