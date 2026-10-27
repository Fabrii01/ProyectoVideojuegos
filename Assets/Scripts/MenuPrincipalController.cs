using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Controlador del Menú Principal.
/// Gestiona la navegación a escenas (Hub, Nivel1), despliegue de ajustes e instrucciones de misión.
/// </summary>
public class MenuPrincipalController : MonoBehaviour
{
    [Header("Paneles de Interfaz")]
    public GameObject panelMenuPrincipal;
    public GameObject panelAjustes;
    public GameObject panelInstrucciones;

    private void Start()
    {
        if (panelMenuPrincipal != null) panelMenuPrincipal.SetActive(true);
        if (panelAjustes != null) panelAjustes.SetActive(false);
        if (panelInstrucciones != null) panelInstrucciones.SetActive(false);

        // Desbloquear cursor para navegación de menú
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        if (AudioManager.Instancia != null)
        {
            AudioManager.Instancia.IniciarMusicaAmbiente();
        }
    }

    public void Jugar()
    {
        if (AudioManager.Instancia != null) AudioManager.Instancia.PlayClick();
        SceneManager.LoadScene("Hub");
    }

    public void CargarNivelDirecto(string nombreNivel)
    {
        if (AudioManager.Instancia != null) AudioManager.Instancia.PlayClick();
        SceneManager.LoadScene(nombreNivel);
    }

    public void AbrirAjustes()
    {
        if (AudioManager.Instancia != null) AudioManager.Instancia.PlayClick();
        if (panelAjustes != null)
        {
            panelAjustes.SetActive(true);
            PanelAjustesUI ui = panelAjustes.GetComponent<PanelAjustesUI>();
            if (ui != null) ui.CargarValoresEnUI();
        }
    }

    public void CerrarAjustes()
    {
        if (AudioManager.Instancia != null) AudioManager.Instancia.PlayClick();
        if (panelAjustes != null) panelAjustes.SetActive(false);
    }

    public void AbrirInstrucciones()
    {
        if (AudioManager.Instancia != null) AudioManager.Instancia.PlayClick();
        if (panelInstrucciones != null) panelInstrucciones.SetActive(true);
    }

    public void CerrarInstrucciones()
    {
        if (AudioManager.Instancia != null) AudioManager.Instancia.PlayClick();
        if (panelInstrucciones != null) panelInstrucciones.SetActive(false);
    }

    public void Salir()
    {
        if (AudioManager.Instancia != null) AudioManager.Instancia.PlayClick();
        Debug.Log("Saliendo del simulador médico...");
        Application.Quit();
    }
}
