using TMPro;
using UnityEngine;

/// <summary>Muestra el dinero actual en cualquier texto de la interfaz.</summary>
[RequireComponent(typeof(TextMeshProUGUI))]
public class TextoMonedas : MonoBehaviour
{
    private TextMeshProUGUI texto;

    private void Awake()
    {
        texto = GetComponent<TextMeshProUGUI>();
    }

    private void OnEnable()
    {
        DatosJugador.OnCambio += Refrescar;
        Refrescar();
    }

    private void OnDisable()
    {
        DatosJugador.OnCambio -= Refrescar;
    }

    private void Refrescar()
    {
        texto.text = DatosJugador.Monedas.ToString();
    }
}
