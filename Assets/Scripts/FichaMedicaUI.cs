using System;
using TMPro;
using UnityEngine;

/// <summary>
/// Ficha que aparece al curar a un paciente. Pausa el juego para leer con calma
/// cómo reconocer y tratar su enfermedad.
/// </summary>
public class FichaMedicaUI : MonoBehaviour
{
    public static FichaMedicaUI Instancia { get; private set; }

    public GameObject panel;
    public PlayerController jugador;
    public TextMeshProUGUI textoNombre;
    public TextMeshProUGUI textoSintomas;
    public TextMeshProUGUI textoTratamiento;

    private Action alCerrar;

    public bool Visible => panel != null && panel.activeSelf;

    private void Awake()
    {
        Instancia = this;
        if (panel != null) panel.SetActive(false);
    }

    private void OnDestroy()
    {
        if (Instancia == this) Instancia = null;
    }

    public void Mostrar(Enfermedad enfermedad, Action accionAlCerrar)
    {
        alCerrar = accionAlCerrar;

        textoNombre.text = enfermedad.nombre;
        textoSintomas.text = enfermedad.sintomas;
        textoTratamiento.text = enfermedad.tratamiento;
        panel.SetActive(true);

        jugador.Detener();
        jugador.enabled = false;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        Time.timeScale = 0f;
    }

    public void Cerrar()
    {
        panel.SetActive(false);

        Time.timeScale = 1f;
        jugador.enabled = true;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        Action accion = alCerrar;
        alCerrar = null;
        accion?.Invoke();
    }
}
