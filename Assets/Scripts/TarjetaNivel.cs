using TMPro;
using UnityEngine;

/// <summary>Tarjeta del menú de misiones: se abre al completar la misión anterior.</summary>
public class TarjetaNivel : MonoBehaviour
{
    public int numeroNivel = 1;
    public GameObject botonIniciar;
    public GameObject bloqueo;
    public TextMeshProUGUI textoEstado;

    private void OnEnable()
    {
        bool abierta = numeroNivel == 1 || DatosJugador.NivelCompletado(numeroNivel - 1);

        botonIniciar.SetActive(abierta);
        bloqueo.SetActive(!abierta);

        if (textoEstado != null)
        {
            textoEstado.text = DatosJugador.NivelCompletado(numeroNivel) ? "COMPLETADA" : string.Empty;
        }
    }
}
