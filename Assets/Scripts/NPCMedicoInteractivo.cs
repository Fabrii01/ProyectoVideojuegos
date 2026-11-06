using UnityEngine;

/// <summary>
/// NPC del Hub con el que se habla pulsando E. Abre un panel: las misiones o la tienda.
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

        if (textoInteraccion != null) textoInteraccion.SetActive(false);
        if (panelNiveles != null) panelNiveles.SetActive(true);

        if (controladorMovimiento != null)
        {
            controladorMovimiento.enabled = false;
        }

        if (controladorMovimiento is PlayerController jugador)
        {
            jugador.Detener();
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
        PlayerController.frameMenuCerrado = Time.frameCount;
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
