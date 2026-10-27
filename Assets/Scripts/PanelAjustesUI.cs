using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Controlador de la interfaz gráfica del panel de Ajustes (Audio, Gráficos y Controles).
/// Se encarga de sincronizar sliders, toggles y dropdowns con AjustesManager.
/// </summary>
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

    [Header("Video y Pantalla")]
    public TMP_Dropdown dropdownCalidad;
    public Toggle togglePantallaCompleta;
    public TMP_Dropdown dropdownResolucion;

    [Header("Jugabilidad y Controles")]
    public Slider sliderSensibilidad;
    public TextMeshProUGUI textoSensibilidad;
    public Toggle toggleInvertirY;

    [Header("Botones")]
    public Button botonRestablecer;
    public Button botonCerrar;

    private List<Resolution> resolucionesFiltradas = new List<Resolution>();
    private bool inicializando = false;

    private void Awake()
    {
        VincularEventosUI();
    }

    private void OnEnable()
    {
        PoblarResoluciones();
        CargarValoresEnUI();
    }

    private void VincularEventosUI()
    {
        if (sliderMaster != null)
            sliderMaster.onValueChanged.AddListener(OnMasterChanged);

        if (sliderSFX != null)
            sliderSFX.onValueChanged.AddListener(OnSFXChanged);

        if (sliderMusica != null)
            sliderMusica.onValueChanged.AddListener(OnMusicaChanged);

        if (dropdownCalidad != null)
            dropdownCalidad.onValueChanged.AddListener(OnCalidadChanged);

        if (togglePantallaCompleta != null)
            togglePantallaCompleta.onValueChanged.AddListener(OnPantallaCompletaChanged);

        if (dropdownResolucion != null)
            dropdownResolucion.onValueChanged.AddListener(OnResolucionChanged);

        if (sliderSensibilidad != null)
            sliderSensibilidad.onValueChanged.AddListener(OnSensibilidadChanged);

        if (toggleInvertirY != null)
            toggleInvertirY.onValueChanged.AddListener(OnInvertirYChanged);

        if (botonRestablecer != null)
            botonRestablecer.onClick.AddListener(RestablecerAjustes);

        if (botonCerrar != null)
            botonCerrar.onClick.AddListener(CerrarPanel);
    }

    private void PoblarResoluciones()
    {
        if (dropdownResolucion == null) return;

        dropdownResolucion.ClearOptions();
        resolucionesFiltradas.Clear();

        Resolution[] todasResoluciones = Screen.resolutions;
        List<string> opciones = new List<string>();
        int indiceActual = 0;

        for (int i = 0; i < todasResoluciones.Length; i++)
        {
            Resolution r = todasResoluciones[i];
            string textoOpcion = $"{r.width} x {r.height} ({r.refreshRateRatio.value:F0}Hz)";

            // Evitar duplicados de misma resolución
            if (!opciones.Contains(textoOpcion))
            {
                opciones.Add(textoOpcion);
                resolucionesFiltradas.Add(r);

                if (r.width == Screen.currentResolution.width && r.height == Screen.currentResolution.height)
                {
                    indiceActual = resolucionesFiltradas.Count - 1;
                }
            }
        }

        dropdownResolucion.AddOptions(opciones);
        dropdownResolucion.value = indiceActual;
        dropdownResolucion.RefreshShownValue();
    }

    public void CargarValoresEnUI()
    {
        if (AjustesManager.Instancia == null) return;

        inicializando = true;

        var aj = AjustesManager.Instancia;

        if (sliderMaster != null)
        {
            sliderMaster.value = aj.VolumenMaster;
            if (textoMaster != null) textoMaster.text = $"{Mathf.RoundToInt(aj.VolumenMaster * 100)}%";
        }

        if (sliderSFX != null)
        {
            sliderSFX.value = aj.VolumenSFX;
            if (textoSFX != null) textoSFX.text = $"{Mathf.RoundToInt(aj.VolumenSFX * 100)}%";
        }

        if (sliderMusica != null)
        {
            sliderMusica.value = aj.VolumenMusica;
            if (textoMusica != null) textoMusica.text = $"{Mathf.RoundToInt(aj.VolumenMusica * 100)}%";
        }

        if (dropdownCalidad != null)
        {
            dropdownCalidad.ClearOptions();
            List<string> nombresCalidad = new List<string>(QualitySettings.names);
            dropdownCalidad.AddOptions(nombresCalidad);
            dropdownCalidad.value = Mathf.Clamp(aj.CalidadGrafica, 0, nombresCalidad.Count - 1);
            dropdownCalidad.RefreshShownValue();
        }

        if (togglePantallaCompleta != null)
        {
            togglePantallaCompleta.isOn = aj.PantallaCompleta;
        }

        if (sliderSensibilidad != null)
        {
            sliderSensibilidad.value = aj.SensibilidadRaton;
            if (textoSensibilidad != null) textoSensibilidad.text = $"{aj.SensibilidadRaton:F1}x";
        }

        if (toggleInvertirY != null)
        {
            toggleInvertirY.isOn = aj.InvertirEjeY;
        }

        inicializando = false;
    }

    private void OnMasterChanged(float val)
    {
        if (inicializando || AjustesManager.Instancia == null) return;
        AjustesManager.Instancia.SetVolumenMaster(val);
        if (textoMaster != null) textoMaster.text = $"{Mathf.RoundToInt(val * 100)}%";
    }

    private void OnSFXChanged(float val)
    {
        if (inicializando || AjustesManager.Instancia == null) return;
        AjustesManager.Instancia.SetVolumenSFX(val);
        if (textoSFX != null) textoSFX.text = $"{Mathf.RoundToInt(val * 100)}%";
    }

    private void OnMusicaChanged(float val)
    {
        if (inicializando || AjustesManager.Instancia == null) return;
        AjustesManager.Instancia.SetVolumenMusica(val);
        if (textoMusica != null) textoMusica.text = $"{Mathf.RoundToInt(val * 100)}%";
    }

    private void OnCalidadChanged(int indice)
    {
        if (inicializando || AjustesManager.Instancia == null) return;
        AjustesManager.Instancia.SetCalidadGrafica(indice);
    }

    private void OnPantallaCompletaChanged(bool val)
    {
        if (inicializando || AjustesManager.Instancia == null) return;
        AjustesManager.Instancia.SetPantallaCompleta(val);
    }

    private void OnResolucionChanged(int indice)
    {
        if (inicializando || AjustesManager.Instancia == null) return;
        if (indice >= 0 && indice < resolucionesFiltradas.Count)
        {
            Resolution r = resolucionesFiltradas[indice];
            Screen.SetResolution(r.width, r.height, AjustesManager.Instancia.PantallaCompleta);
        }
    }

    private void OnSensibilidadChanged(float val)
    {
        if (inicializando || AjustesManager.Instancia == null) return;
        AjustesManager.Instancia.SetSensibilidadRaton(val);
        if (textoSensibilidad != null) textoSensibilidad.text = $"{val:F1}x";
    }

    private void OnInvertirYChanged(bool val)
    {
        if (inicializando || AjustesManager.Instancia == null) return;
        AjustesManager.Instancia.SetInvertirEjeY(val);
    }

    public void RestablecerAjustes()
    {
        if (AjustesManager.Instancia != null)
        {
            AjustesManager.Instancia.RestablecerPorDefecto();
            CargarValoresEnUI();
        }
    }

    public void AbrirPanel()
    {
        if (panelAjustes != null)
        {
            panelAjustes.SetActive(true);
        }
        else
        {
            gameObject.SetActive(true);
        }
        CargarValoresEnUI();
    }

    public void CerrarPanel()
    {
        if (panelAjustes != null)
        {
            panelAjustes.SetActive(false);
        }
        else
        {
            gameObject.SetActive(false);
        }

        if (AudioManager.Instancia != null)
        {
            AudioManager.Instancia.PlayClick();
        }
    }
}
