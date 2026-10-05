using UnityEngine;
using UnityEngine.SceneManagement; // Librería clave para cargar niveles

public class GestorNiveles : MonoBehaviour
{
    public void CargarNivel1()
    {
        // Carga la escena usando su nombre exacto
        SceneManager.LoadScene("Nivel1");
    }
}