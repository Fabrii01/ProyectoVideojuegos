using System;
using UnityEngine;

/// <summary>
/// Ajustes del jugador: volúmenes y sensibilidad del ratón.
/// Se guardan automáticamente con PlayerPrefs.
/// </summary>
public class AjustesManager : MonoBehaviour
{
    public static AjustesManager Instancia { get; private set; }

    private const string PREF_VOLUMEN_MASTER = "Ajustes_VolumenMaster";
    private const string PREF_VOLUMEN_SFX = "Ajustes_VolumenSFX";
    private const string PREF_VOLUMEN_MUSICA = "Ajustes_VolumenMusica";
    private const string PREF_SENSIBILIDAD_RATON = "Ajustes_SensibilidadRaton";

    private const float MASTER_INICIAL = 0.8f;
    private const float SFX_INICIAL = 0.9f;
    private const float MUSICA_INICIAL = 0.7f;
    private const float SENSIBILIDAD_INICIAL = 3f;

    public float VolumenMaster { get; private set; } = MASTER_INICIAL;
    public float VolumenSFX { get; private set; } = SFX_INICIAL;
    public float VolumenMusica { get; private set; } = MUSICA_INICIAL;
    public float SensibilidadRaton { get; private set; } = SENSIBILIDAD_INICIAL;

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
        AudioListener.volume = VolumenMaster;
    }

    private void CargarAjustes()
    {
        VolumenMaster = PlayerPrefs.GetFloat(PREF_VOLUMEN_MASTER, MASTER_INICIAL);
        VolumenSFX = PlayerPrefs.GetFloat(PREF_VOLUMEN_SFX, SFX_INICIAL);
        VolumenMusica = PlayerPrefs.GetFloat(PREF_VOLUMEN_MUSICA, MUSICA_INICIAL);
        SensibilidadRaton = PlayerPrefs.GetFloat(PREF_SENSIBILIDAD_RATON, SENSIBILIDAD_INICIAL);
    }

    private void GuardarAjustes()
    {
        PlayerPrefs.SetFloat(PREF_VOLUMEN_MASTER, VolumenMaster);
        PlayerPrefs.SetFloat(PREF_VOLUMEN_SFX, VolumenSFX);
        PlayerPrefs.SetFloat(PREF_VOLUMEN_MUSICA, VolumenMusica);
        PlayerPrefs.SetFloat(PREF_SENSIBILIDAD_RATON, SensibilidadRaton);
        PlayerPrefs.Save();

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

    public void SetSensibilidadRaton(float sensibilidad)
    {
        SensibilidadRaton = Mathf.Clamp(sensibilidad, 0.5f, 10f);
        GuardarAjustes();
    }

    public void RestablecerPorDefecto()
    {
        VolumenMaster = MASTER_INICIAL;
        VolumenSFX = SFX_INICIAL;
        VolumenMusica = MUSICA_INICIAL;
        SensibilidadRaton = SENSIBILIDAD_INICIAL;

        AudioListener.volume = VolumenMaster;
        GuardarAjustes();
    }
}
