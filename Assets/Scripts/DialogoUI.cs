using TMPro;
using UnityEngine;

/// <summary>Cuadro de diálogo en pantalla, con nombre y texto que aparece letra por letra.</summary>
public class DialogoUI : MonoBehaviour
{
    public static DialogoUI Instancia { get; private set; }

    public GameObject panel;
    public TextMeshProUGUI textoNombre;
    public TextMeshProUGUI textoFrase;
    public TextMeshProUGUI textoPista;
    public float letrasPorSegundo = 50f;

    private object hablante;
    private float letras;

    public bool Visible => panel != null && panel.activeSelf;
    public string FraseActual => textoFrase != null ? textoFrase.text : string.Empty;

    private void Awake()
    {
        Instancia = this;
        if (panel != null) panel.SetActive(false);
    }

    private void OnDestroy()
    {
        if (Instancia == this) Instancia = null;
    }

    public void Mostrar(object quien, string nombre, string frase, string pista = "")
    {
        hablante = quien;
        letras = 0f;
        textoNombre.text = nombre;
        textoFrase.text = frase;
        textoFrase.maxVisibleCharacters = 0;
        if (textoPista != null) textoPista.text = pista;
        panel.SetActive(true);
    }

    /// <summary>Solo cierra si sigue hablando quien lo pide, para no cortar a otro NPC.</summary>
    public void Ocultar(object quien)
    {
        if (hablante != quien) return;

        hablante = null;
        panel.SetActive(false);
    }

    private void Update()
    {
        if (!Visible) return;

        letras += letrasPorSegundo * Time.unscaledDeltaTime;
        textoFrase.maxVisibleCharacters = Mathf.FloorToInt(letras);
    }
}
