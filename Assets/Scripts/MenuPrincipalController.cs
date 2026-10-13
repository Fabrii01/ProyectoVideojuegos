
using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuPrincipalController : MonoBehaviour
{
    public void Jugar()
    {
        SceneManager.LoadScene("Hub");
    }

    public void Salir()
    {
        Application.Quit();
    }
}
