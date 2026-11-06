using UnityEngine;

/// <summary>
/// Sistema de audio del juego. Reproduce efectos de sonido (SFX) y música ambiental.
/// Genera clips procedurales por defecto para que funcione de forma inmediata sin dependencias externas.
/// </summary>
public class AudioManager : MonoBehaviour
{
    public static AudioManager Instancia { get; private set; }

    [Header("Fuentes de Audio")]
    public AudioSource fuenteSFX;
    public AudioSource fuenteMusica;
    public AudioSource fuenteMusicaB;

    [Header("Clips Opcionales (Si se dejan vacíos se sintetizan automáticamente)")]
    public AudioClip clipJeringa;
    public AudioClip clipEquipaje;
    public AudioClip clipBotiquin;
    public AudioClip clipDanio;
    public AudioClip clipCurar;
    public AudioClip clipVictoria;
    public AudioClip clipClickUI;
    public AudioClip clipMoneda;
    public AudioClip clipCompra;
    public AudioClip clipAlerta;
    public AudioClip clipPajaro;
    public AudioClip clipGolpe;
    public AudioClip clipFallo;

    private AudioSource musicaActiva;
    private float duracionFundido = 1.5f;
    private bool musicaApagada;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void InicializarEnArranque()
    {
        if (Instancia == null)
        {
            GameObject go = new GameObject("AudioManager_Global");
            Instancia = go.AddComponent<AudioManager>();
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

        ConfigurarFuentes();
        GenerarClipsProceduralesSiFaltan();
        ActualizarVolumenes();

        if (AjustesManager.Instancia != null)
        {
            AjustesManager.Instancia.OnAjustesCambiados += ActualizarVolumenes;
        }
    }

    private void OnDestroy()
    {
        if (AjustesManager.Instancia != null)
        {
            AjustesManager.Instancia.OnAjustesCambiados -= ActualizarVolumenes;
        }
    }

    private void ConfigurarFuentes()
    {
        if (fuenteSFX == null)
        {
            fuenteSFX = gameObject.AddComponent<AudioSource>();
            fuenteSFX.playOnAwake = false;
        }

        if (fuenteMusica == null)
        {
            fuenteMusica = CrearFuenteMusica();
        }

        if (fuenteMusicaB == null)
        {
            fuenteMusicaB = CrearFuenteMusica();
        }

        musicaActiva = fuenteMusica;
    }

    public void ActualizarVolumenes()
    {
        if (AjustesManager.Instancia == null) return;

        if (fuenteSFX != null)
        {
            fuenteSFX.volume = AjustesManager.Instancia.VolumenSFX;
        }

    }

    private AudioSource CrearFuenteMusica()
    {
        AudioSource fuente = gameObject.AddComponent<AudioSource>();
        fuente.loop = true;
        fuente.playOnAwake = false;
        fuente.volume = 0f;
        return fuente;
    }

    /// <summary>Pasa suavemente de la música actual a la indicada.</summary>
    public void CambiarMusica(AudioClip clip, float segundosFundido = 1.5f)
    {
        if (clip == null || (musicaActiva.clip == clip && musicaActiva.isPlaying && !musicaApagada)) return;

        duracionFundido = Mathf.Max(0.05f, segundosFundido);
        musicaApagada = false;

        if (musicaActiva.clip == clip) return;

        musicaActiva = musicaActiva == fuenteMusica ? fuenteMusicaB : fuenteMusica;
        musicaActiva.clip = clip;
        musicaActiva.Play();
    }

    /// <summary>Baja la música hasta el silencio, por ejemplo al ganar.</summary>
    public void DetenerMusica(float segundosFundido = 1f)
    {
        duracionFundido = Mathf.Max(0.05f, segundosFundido);
        musicaApagada = true;
    }

    public AudioClip MusicaActual => musicaApagada ? null : musicaActiva.clip;

    private void Update()
    {
        // Sin escala de tiempo: la música sigue fundiendo en pausa y en la pantalla de victoria.
        float volumen = AjustesManager.Instancia != null ? AjustesManager.Instancia.VolumenMusica : 0.7f;
        float paso = Time.unscaledDeltaTime / duracionFundido;

        AudioSource otra = musicaActiva == fuenteMusica ? fuenteMusicaB : fuenteMusica;
        musicaActiva.volume = Mathf.MoveTowards(musicaActiva.volume, musicaApagada ? 0f : volumen, paso);
        otra.volume = Mathf.MoveTowards(otra.volume, 0f, paso);

        if (otra.isPlaying && otra.volume <= 0f) otra.Stop();
    }

    public void PlaySFX(AudioClip clip, float pitch = 1f)
    {
        if (fuenteSFX != null && clip != null)
        {
            fuenteSFX.pitch = pitch;
            fuenteSFX.PlayOneShot(clip, AjustesManager.Instancia != null ? AjustesManager.Instancia.VolumenSFX : 0.8f);
        }
    }

    public void PlayJeringa() => PlaySFX(clipJeringa, Random.Range(0.95f, 1.15f));
    public void PlayEquipaje() => PlaySFX(clipEquipaje, Random.Range(0.95f, 1.05f));
    public void PlayBotiquin() => PlaySFX(clipBotiquin, 1f);
    public void PlayCurar() => PlaySFX(clipCurar, 1.1f);
    public void PlayDanio() => PlaySFX(clipDanio, Random.Range(0.9f, 1.1f));
    public void PlayVictoria() => PlaySFX(clipVictoria, 1f);
    public void PlayClick() => PlaySFX(clipClickUI, 1f);
    public void PlayMoneda() => PlaySFX(clipMoneda, Random.Range(0.98f, 1.08f));
    public void PlayCompra() => PlaySFX(clipCompra, 1f);
    public void PlayAlerta() => PlaySFX(clipAlerta, 1f);
    public void PlayPajaro() => PlaySFX(clipPajaro, Random.Range(0.9f, 1.2f));
    public void PlayGolpe() => PlaySFX(clipGolpe, Random.Range(0.9f, 1.1f));
    public void PlayFallo() => PlaySFX(clipFallo, Random.Range(0.95f, 1.1f));

    private void GenerarClipsProceduralesSiFaltan()
    {
        if (clipJeringa == null)
            clipJeringa = SintetizarTono("SFX_Jeringa", 880f, 0.22f, FormaOnda.Senoidal, 0.5f);

        if (clipEquipaje == null)
            clipEquipaje = SintetizarAcorde("SFX_Equipaje", new float[] { 440f, 554.37f, 659.25f }, 0.4f);

        if (clipBotiquin == null)
            clipBotiquin = SintetizarAcorde("SFX_Botiquin", new float[] { 523.25f, 659.25f, 783.99f }, 0.45f);

        if (clipCurar == null)
            clipCurar = SintetizarArpegio("SFX_Curar", new float[] { 523.25f, 659.25f, 783.99f, 1046.50f }, 0.5f);

        if (clipDanio == null)
            clipDanio = SintetizarTono("SFX_Danio", 160f, 0.25f, FormaOnda.Sierra, 0.7f);

        if (clipVictoria == null)
            clipVictoria = SintetizarArpegio("SFX_Victoria", new float[] { 440f, 554.37f, 659.25f, 880f, 1108.73f }, 0.9f);

        if (clipClickUI == null)
            clipClickUI = SintetizarTono("SFX_ClickUI", 1200f, 0.05f, FormaOnda.Senoidal, 0.3f);

        if (clipMoneda == null)
            clipMoneda = SintetizarArpegio("SFX_Moneda", new float[] { 987.77f, 1318.51f }, 0.18f);

        if (clipCompra == null)
            clipCompra = SintetizarArpegio("SFX_Compra", new float[] { 659.25f, 880f, 1318.51f }, 0.35f);

        if (clipAlerta == null)
            clipAlerta = SintetizarTono("SFX_Alerta", 330f, 0.3f, FormaOnda.Cuadrada, 0.5f);

        if (clipPajaro == null)
            clipPajaro = SintetizarArpegio("SFX_Pajaro", new float[] { 3100f, 3700f, 3300f }, 0.22f);

        if (clipGolpe == null)
            clipGolpe = SintetizarTono("SFX_Golpe", 110f, 0.16f, FormaOnda.Cuadrada, 0.35f);

        if (clipFallo == null)
            clipFallo = SintetizarTono("SFX_Fallo", 420f, 0.09f, FormaOnda.Sierra, 0.3f);
    }

    private enum FormaOnda { Senoidal, Sierra, Cuadrada }

    private AudioClip SintetizarTono(string nombre, float freq, float duracion, FormaOnda forma, float decaimiento)
    {
        int sampleRate = 44100;
        int totalSamples = (int)(sampleRate * duracion);
        float[] samples = new float[totalSamples];

        for (int i = 0; i < totalSamples; i++)
        {
            float t = (float)i / sampleRate;
            float envolvente = Mathf.Exp(-t / (duracion * decaimiento));
            float onda = 0f;

            switch (forma)
            {
                case FormaOnda.Senoidal:
                    onda = Mathf.Sin(2 * Mathf.PI * freq * t);
                    break;
                case FormaOnda.Sierra:
                    onda = 2f * (t * freq - Mathf.Floor(t * freq + 0.5f));
                    break;
                case FormaOnda.Cuadrada:
                    onda = Mathf.Sign(Mathf.Sin(2 * Mathf.PI * freq * t));
                    break;
            }

            samples[i] = onda * envolvente * 0.4f;
        }

        AudioClip clip = AudioClip.Create(nombre, totalSamples, 1, sampleRate, false);
        clip.SetData(samples, 0);
        return clip;
    }

    private AudioClip SintetizarAcorde(string nombre, float[] freqs, float duracion)
    {
        int sampleRate = 44100;
        int totalSamples = (int)(sampleRate * duracion);
        float[] samples = new float[totalSamples];

        for (int i = 0; i < totalSamples; i++)
        {
            float t = (float)i / sampleRate;
            float envolvente = Mathf.Exp(-t / (duracion * 0.4f));
            float valor = 0f;

            foreach (var f in freqs)
            {
                valor += Mathf.Sin(2 * Mathf.PI * f * t);
            }

            samples[i] = (valor / freqs.Length) * envolvente * 0.5f;
        }

        AudioClip clip = AudioClip.Create(nombre, totalSamples, 1, sampleRate, false);
        clip.SetData(samples, 0);
        return clip;
    }

    private AudioClip SintetizarArpegio(string nombre, float[] notas, float duracion)
    {
        int sampleRate = 44100;
        int totalSamples = (int)(sampleRate * duracion);
        float[] samples = new float[totalSamples];
        float tiempoNota = duracion / notas.Length;

        for (int i = 0; i < totalSamples; i++)
        {
            float t = (float)i / sampleRate;
            int indiceNota = Mathf.Min((int)(t / tiempoNota), notas.Length - 1);
            float freq = notas[indiceNota];
            float tLocal = t - (indiceNota * tiempoNota);
            float envolvente = Mathf.Exp(-tLocal / (tiempoNota * 0.7f));

            samples[i] = Mathf.Sin(2 * Mathf.PI * freq * t) * envolvente * 0.45f;
        }

        AudioClip clip = AudioClip.Create(nombre, totalSamples, 1, sampleRate, false);
        clip.SetData(samples, 0);
        return clip;
    }

}
