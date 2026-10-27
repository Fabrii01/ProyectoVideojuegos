using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Gestor de cambio de escenas y niveles.
/// </summary>
public class GestorNiveles : MonoBehaviour
{
    public void CargarNivel1()
    {
        if (AudioManager.Instancia != null) AudioManager.Instancia.PlayClick();
        Time.timeScale = 1f;
        SceneManager.LoadScene("Nivel1");
    }

    public void CargarHub()
    {
        if (AudioManager.Instancia != null) AudioManager.Instancia.PlayClick();
        Time.timeScale = 1f;
        SceneManager.LoadScene("Hub");
    }

    public void CargarMenuPrincipal()
    {
        if (AudioManager.Instancia != null) AudioManager.Instancia.PlayClick();
        Time.timeScale = 1f;
        SceneManager.LoadScene("MenuPrincipal");
    }

    public void CargarEscena(string nombreEscena)
    {
        if (AudioManager.Instancia != null) AudioManager.Instancia.PlayClick();
        Time.timeScale = 1f;
        SceneManager.LoadScene(nombreEscena);
    }
}
