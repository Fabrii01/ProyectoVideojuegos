
using UnityEngine;

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
        textoInteraccion.SetActive(false);
        panelNiveles.SetActive(false);
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

            if (!menuAbierto)
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
            textoInteraccion.SetActive(false);
        }
    }

    private void AbrirMenu()
    {
        menuAbierto = true;

        bloqueoCursorOriginal = Cursor.lockState;
        cursorVisibleOriginal = Cursor.visible;

        textoInteraccion.SetActive(false);
        panelNiveles.SetActive(true);

        if (controladorMovimiento != null)
        {
            controladorMovimiento.enabled = false;
        }

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    private void CerrarMenu()
    {
        menuAbierto = false;
        panelNiveles.SetActive(false);

        if (controladorMovimiento != null)
        {
            controladorMovimiento.enabled = true;
        }

        Cursor.lockState = bloqueoCursorOriginal;
        Cursor.visible = cursorVisibleOriginal;

        if (jugadorCerca)
        {
            textoInteraccion.SetActive(true);
        }
    }
}
