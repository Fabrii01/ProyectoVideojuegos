using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>Conecta los controles del panel de ajustes con AjustesManager.</summary>
public class PanelAjustesUI : MonoBehaviour
{
    [Header("Panel Principal")]
    public GameObject panelAjustes;

    [Header("Audio")]
    public Slider sliderMaster;
    public TextMeshProUGUI textoMaster;
    public Slider sliderSFX;
    public TextMeshProUGUI textoSFX;
    public Slider sliderMusica;
    public TextMeshProUGUI textoMusica;

    [Header("Controles")]
    public Slider sliderSensibilidad;
    public TextMeshProUGUI textoSensibilidad;

    [Header("Botones")]
    public Button botonRestablecer;
    public Button botonCerrar;

    private bool inicializando;

    private void Awake()
    {
        if (sliderMaster != null) sliderMaster.onValueChanged.AddListener(OnMasterChanged);
        if (sliderSFX != null) sliderSFX.onValueChanged.AddListener(OnSFXChanged);
        if (sliderMusica != null) sliderMusica.onValueChanged.AddListener(OnMusicaChanged);
        if (sliderSensibilidad != null) sliderSensibilidad.onValueChanged.AddListener(OnSensibilidadChanged);
        if (botonRestablecer != null) botonRestablecer.onClick.AddListener(RestablecerAjustes);
        if (botonCerrar != null) botonCerrar.onClick.AddListener(CerrarPanel);
    }

    private void OnEnable()
    {
        CargarValoresEnUI();
    }

    public void CargarValoresEnUI()
    {
        AjustesManager ajustes = AjustesManager.Instancia;
        if (ajustes == null) return;

        inicializando = true;

        if (sliderMaster != null) sliderMaster.value = ajustes.VolumenMaster;
        if (sliderSFX != null) sliderSFX.value = ajustes.VolumenSFX;
        if (sliderMusica != null) sliderMusica.value = ajustes.VolumenMusica;
        if (sliderSensibilidad != null) sliderSensibilidad.value = ajustes.SensibilidadRaton;

        MostrarPorcentaje(textoMaster, ajustes.VolumenMaster);
        MostrarPorcentaje(textoSFX, ajustes.VolumenSFX);
        MostrarPorcentaje(textoMusica, ajustes.VolumenMusica);
        if (textoSensibilidad != null) textoSensibilidad.text = $"{ajustes.SensibilidadRaton:F1}x";

        inicializando = false;
    }

    private static void MostrarPorcentaje(TextMeshProUGUI texto, float valor)
    {
        if (texto != null) texto.text = $"{Mathf.RoundToInt(valor * 100)}%";
    }

    private void OnMasterChanged(float valor)
    {
        if (inicializando || AjustesManager.Instancia == null) return;
        AjustesManager.Instancia.SetVolumenMaster(valor);
        MostrarPorcentaje(textoMaster, valor);
    }

    private void OnSFXChanged(float valor)
    {
        if (inicializando || AjustesManager.Instancia == null) return;
        AjustesManager.Instancia.SetVolumenSFX(valor);
        MostrarPorcentaje(textoSFX, valor);
    }

    private void OnMusicaChanged(float valor)
    {
        if (inicializando || AjustesManager.Instancia == null) return;
        AjustesManager.Instancia.SetVolumenMusica(valor);
        MostrarPorcentaje(textoMusica, valor);
    }

    private void OnSensibilidadChanged(float valor)
    {
        if (inicializando || AjustesManager.Instancia == null) return;
        AjustesManager.Instancia.SetSensibilidadRaton(valor);
        if (textoSensibilidad != null) textoSensibilidad.text = $"{valor:F1}x";
    }

    public void RestablecerAjustes()
    {
        if (AjustesManager.Instancia == null) return;

        AjustesManager.Instancia.RestablecerPorDefecto();
        CargarValoresEnUI();
    }

    public void CerrarPanel()
    {
        GameObject panel = panelAjustes != null ? panelAjustes : gameObject;
        panel.SetActive(false);
        if (AudioManager.Instancia != null) AudioManager.Instancia.PlayClick();
    }
}
