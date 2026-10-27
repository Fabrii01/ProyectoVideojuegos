using System;
using UnityEngine;

/// <summary>
/// Gestor global y persistente de configuración del juego (Audio, Video/Gráficos y Controles).
/// Guarda y carga automáticamente todas las preferencias con PlayerPrefs.
/// </summary>
public class AjustesManager : MonoBehaviour
{
    public static AjustesManager Instancia { get; private set; }

    // Constantes de claves PlayerPrefs
    private const string PREF_VOLUMEN_MASTER = "Ajustes_VolumenMaster";
    private const string PREF_VOLUMEN_SFX = "Ajustes_VolumenSFX";
    private const string PREF_VOLUMEN_MUSICA = "Ajustes_VolumenMusica";
    private const string PREF_CALIDAD_GRAFICA = "Ajustes_CalidadGrafica";
    private const string PREF_PANTALLA_COMPLETA = "Ajustes_PantallaCompleta";
    private const string PREF_RESOLUCION_INDICE = "Ajustes_ResolucionIndice";
    private const string PREF_SENSIBILIDAD_RATON = "Ajustes_SensibilidadRaton";
    private const string PREF_INVERTIR_EJE_Y = "Ajustes_InvertirEjeY";

    // Variables en memoria
    public float VolumenMaster { get; private set; } = 0.8f;
    public float VolumenSFX { get; private set; } = 0.9f;
    public float VolumenMusica { get; private set; } = 0.7f;
    public int CalidadGrafica { get; private set; } = 2; // Medio por defecto
    public bool PantallaCompleta { get; private set; } = true;
    public int ResolucionIndice { get; private set; } = 0;
    public float SensibilidadRaton { get; private set; } = 3.0f;
    public bool InvertirEjeY { get; private set; } = false;

    // Eventos para notificación en tiempo real
    public event Action OnAjustesCambiados;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void InicializarEnArranque()
    {
        if (Instancia == null)
        {
            GameObject go = new GameObject("AjustesManager_Global");
            Instancia = go.AddComponent<AjustesManager>();
            DontDestroyOnLoad(go);
        }
    }

    private void Awake()
    {
        if (Instancia != null && Instancia != this)
        {
            Destroy(gameObject);
            return;
        }

        Instancia = this;
        DontDestroyOnLoad(gameObject);
        CargarAjustes();
        AplicarTodosLosAjustes();
    }

    public void CargarAjustes()
    {
        VolumenMaster = PlayerPrefs.GetFloat(PREF_VOLUMEN_MASTER, 0.8f);
        VolumenSFX = PlayerPrefs.GetFloat(PREF_VOLUMEN_SFX, 0.9f);
        VolumenMusica = PlayerPrefs.GetFloat(PREF_VOLUMEN_MUSICA, 0.7f);
        CalidadGrafica = PlayerPrefs.GetInt(PREF_CALIDAD_GRAFICA, Mathf.Clamp(QualitySettings.GetQualityLevel(), 0, QualitySettings.names.Length - 1));
        PantallaCompleta = PlayerPrefs.GetInt(PREF_PANTALLA_COMPLETA, Screen.fullScreen ? 1 : 0) == 1;
        ResolucionIndice = PlayerPrefs.GetInt(PREF_RESOLUCION_INDICE, -1);
        SensibilidadRaton = PlayerPrefs.GetFloat(PREF_SENSIBILIDAD_RATON, 3.0f);
        InvertirEjeY = PlayerPrefs.GetInt(PREF_INVERTIR_EJE_Y, 0) == 1;
    }

    public void GuardarAjustes()
    {
        PlayerPrefs.SetFloat(PREF_VOLUMEN_MASTER, VolumenMaster);
        PlayerPrefs.SetFloat(PREF_VOLUMEN_SFX, VolumenSFX);
        PlayerPrefs.SetFloat(PREF_VOLUMEN_MUSICA, VolumenMusica);
        PlayerPrefs.SetInt(PREF_CALIDAD_GRAFICA, CalidadGrafica);
        PlayerPrefs.SetInt(PREF_PANTALLA_COMPLETA, PantallaCompleta ? 1 : 0);
        PlayerPrefs.SetInt(PREF_RESOLUCION_INDICE, ResolucionIndice);
        PlayerPrefs.SetFloat(PREF_SENSIBILIDAD_RATON, SensibilidadRaton);
        PlayerPrefs.SetInt(PREF_INVERTIR_EJE_Y, InvertirEjeY ? 1 : 0);
        PlayerPrefs.Save();

        OnAjustesCambiados?.Invoke();
    }

    public void AplicarTodosLosAjustes()
    {
        // 1. Audio
        AudioListener.volume = VolumenMaster;

        // 2. Gráficos
        if (CalidadGrafica >= 0 && CalidadGrafica < QualitySettings.names.Length)
        {
            QualitySettings.SetQualityLevel(CalidadGrafica, true);
        }

        // 3. Resolución y Pantalla Completa
        Resolution[] resoluciones = Screen.resolutions;
        if (resoluciones != null && resoluciones.Length > 0)
        {
            if (ResolucionIndice >= 0 && ResolucionIndice < resoluciones.Length)
            {
                Resolution r = resoluciones[ResolucionIndice];
                Screen.SetResolution(r.width, r.height, PantallaCompleta);
            }
            else
            {
                Screen.fullScreen = PantallaCompleta;
            }
        }
        else
        {
            Screen.fullScreen = PantallaCompleta;
        }

        OnAjustesCambiados?.Invoke();
    }

    public void SetVolumenMaster(float valor)
    {
        VolumenMaster = Mathf.Clamp01(valor);
        AudioListener.volume = VolumenMaster;
        GuardarAjustes();
    }

    public void SetVolumenSFX(float valor)
    {
        VolumenSFX = Mathf.Clamp01(valor);
        GuardarAjustes();
    }

    public void SetVolumenMusica(float valor)
    {
        VolumenMusica = Mathf.Clamp01(valor);
        GuardarAjustes();
    }

    public void SetCalidadGrafica(int nivel)
    {
        CalidadGrafica = Mathf.Clamp(nivel, 0, QualitySettings.names.Length - 1);
        QualitySettings.SetQualityLevel(CalidadGrafica, true);
        GuardarAjustes();
    }

    public void SetPantallaCompleta(bool completa)
    {
        PantallaCompleta = completa;
        Screen.fullScreen = completa;
        GuardarAjustes();
    }

    public void SetResolucion(int indice)
    {
        Resolution[] resoluciones = Screen.resolutions;
        if (resoluciones != null && indice >= 0 && indice < resoluciones.Length)
        {
            ResolucionIndice = indice;
            Resolution r = resoluciones[indice];
            Screen.SetResolution(r.width, r.height, PantallaCompleta);
            GuardarAjustes();
        }
    }

    public void SetSensibilidadRaton(float sens)
    {
        SensibilidadRaton = Mathf.Clamp(sens, 0.5f, 10f);
        GuardarAjustes();
    }

    public void SetInvertirEjeY(bool invertir)
    {
        InvertirEjeY = invertir;
        GuardarAjustes();
    }

    public void RestablecerPorDefecto()
    {
        VolumenMaster = 0.8f;
        VolumenSFX = 0.9f;
        VolumenMusica = 0.7f;
        CalidadGrafica = Mathf.Clamp(2, 0, QualitySettings.names.Length - 1);
        PantallaCompleta = true;
        ResolucionIndice = -1;
        SensibilidadRaton = 3.0f;
        InvertirEjeY = false;

        AplicarTodosLosAjustes();
        GuardarAjustes();
    }
}
