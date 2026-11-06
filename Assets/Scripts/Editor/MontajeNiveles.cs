using System;
using System.Collections.Generic;
using System.IO;
using TMPro;
using Unity.AI.Navigation;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.EventSystems;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Object = UnityEngine.Object;

/// <summary>
/// Base común de los niveles: cada uno describe su ambiente en NivelDatos y este
/// archivo crea la escena, el jugador, la interfaz, el mapa y la navegación.
/// </summary>
public static partial class MontajeRescate
{
    private class NivelDatos
    {
        public int numero;
        public string escena;
        public float medio;
        public Vector3 inicio;
        public string cielo;
        public string suelo;
        public Color tinteSuelo;
        public float repetirSuelo;
        public Color colorNiebla;
        public float nieblaInicio;
        public float nieblaFin;
        public Color colorSol;
        public float intensidadSol;
        public Vector3 giroSol;
        public Color ambienteArriba;
        public Color ambienteMedio;
        public Color ambienteAbajo;
        public Color colorParticulas;
        public string musica;
        public string musicaCombate;
        public int recompensa;
        public Material piel;
        public float gravedad;
        public string[] avisos;
        public string leccion;
        public Action<GruposNivel> construir;
    }

    private class GruposNivel
    {
        public Transform nivel;
        public Transform escenario;
        public Transform obstaculos;
        public Transform peligros;
        public Transform suministros;
        public Transform monedas;
        public Transform enemigos;
    }

    private class CaminoEnemigo
    {
        public string nombre;
        public Vector3[] puntos;

        public CaminoEnemigo(string nombre, Vector3[] puntos)
        {
            this.nombre = nombre;
            this.puntos = puntos;
        }
    }

    private const int MAPA_PIXELES = 768;
    private const float ESCALA_SUMINISTRO = 2.4f;

    private static void MontarNiveles()
    {
        MontarNivel(DatosNivel1(), EnemigosNivel1(), RUTA_ENEMIGO);
        MontarNivel(DatosNivel2(), EnemigosNivel2(), RUTA_FURTIVO);
        MontarNivel(DatosNivel3(), EnemigosNivel3(), RUTA_ANDROIDE);
    }

    private static void MontarNivel(NivelDatos d, List<CaminoEnemigo> caminos, string rutaEnemigo)
    {
        Scene escena = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

        Camera camara = CrearCamara();
        Light sol = new GameObject("Sol").AddComponent<Light>();
        sol.type = LightType.Directional;
        sol.shadows = LightShadows.Soft;
        sol.transform.rotation = Quaternion.Euler(d.giroSol);
        sol.color = d.colorSol;
        sol.intensity = d.intensidadSol;

        GameObject sistema = new GameObject("EventSystem");
        sistema.AddComponent<EventSystem>();
        sistema.AddComponent<StandaloneInputModule>();

        GameObject lienzo = new GameObject("Canvas");
        lienzo.AddComponent<Canvas>();

        GameObject volumen = new GameObject("Global Volume");
        Volume postproceso = volumen.AddComponent<Volume>();
        postproceso.isGlobal = true;
        postproceso.sharedProfile = Cargar<VolumeProfile>("Assets/Settings/SampleSceneProfile.asset");

        GameObject jugadorGO = CrearJugador(d, camara);
        PlayerController jugador = PrepararJugador(jugadorGO);
        jugadorGO.transform.position = d.inicio;
        ((Renderer)jugador.modeloVisualDoctor.GetComponentInChildren<SkinnedMeshRenderer>()).sharedMaterial = d.piel;
        if (d.gravedad < 0f) PonerCascoEspacial(jugadorGO);

        camara.transform.position = d.inicio + new Vector3(0f, 2f, -5f);
        camara.transform.rotation = Quaternion.Euler(12f, 0f, 0f);

        AmbienteNivel(d, camara);

        GruposNivel g = CrearGrupos();
        ConstruirSuelo(d, g.escenario);
        ConstruirLimites(d, g.escenario);
        d.construir(g);

        if (d.colorParticulas.a > 0f) ParticulasAmbiente(camara.transform, d.colorParticulas);

        GestorMision gestor = new GameObject("GestorMision").AddComponent<GestorMision>();
        gestor.numeroNivel = d.numero;
        gestor.recompensa = d.recompensa;
        gestor.jugador = jugador;
        gestor.avisosIniciales = d.avisos;
        gestor.leccion = d.leccion;

        if (d.gravedad < 0f) new GameObject("Gravedad").AddComponent<GravedadEscena>().gravedad = d.gravedad;

        Canvas canvas = PrepararCanvas();
        ConstruirHud(canvas.transform, jugador, gestor);
        PanelPausa(canvas.transform, jugador, "VOLVER AL HUB", jugador.VolverAlHub);
        ConstruirVictoria(canvas.transform, gestor);
        ConstruirDialogo(canvas.transform);
        ConstruirAvisoCentral(canvas.transform);
        ConstruirFichaMedica(canvas.transform, jugador);

        // Los enemigos se agregan después de hornear para que no se tapen su propio camino.
        HornearNavMesh(d, g, jugadorGO);
        Texture2D mapa = HornearMapa(d, g, jugadorGO);
        foreach (CaminoEnemigo camino in caminos) CrearEnemigo(g.enemigos, rutaEnemigo, camino.nombre, camino.puntos);

        ConstruirMapa(canvas.transform, jugador, mapa, d.medio);
        MusicaDeEscena(d.musica, d.musicaCombate);

        EditorUtility.SetDirty(jugador);
        EditorSceneManager.MarkSceneDirty(escena);
        EditorSceneManager.SaveScene(escena, "Assets/Scenes/" + d.escena + ".unity");
    }

    private static Camera CrearCamara()
    {
        GameObject go = new GameObject("Main Camera");
        go.tag = "MainCamera";
        go.AddComponent<AudioListener>();
        Camera camara = go.AddComponent<Camera>();
        camara.farClipPlane = 900f;
        camara.fieldOfView = 58f;
        camara.GetUniversalAdditionalCameraData().renderPostProcessing = true;
        return camara;
    }

    private static GameObject CrearJugador(NivelDatos d, Camera camara)
    {
        GameObject jugador = new GameObject("Jugador");
        jugador.tag = "Player";

        Rigidbody cuerpo = jugador.AddComponent<Rigidbody>();
        cuerpo.mass = 1f;
        cuerpo.freezeRotation = true;
        cuerpo.collisionDetectionMode = CollisionDetectionMode.Continuous;

        CapsuleCollider capsula = jugador.AddComponent<CapsuleCollider>();
        capsula.height = 2f;
        capsula.radius = 0.45f;

        jugador.AddComponent<PlayerController>();
        return jugador;
    }

    private static void PonerCascoEspacial(GameObject jugador)
    {
        EquipamientoJugador equipo = jugador.GetComponent<EquipamientoJugador>();
        equipo.objetosFijos = new[] { Cargar<ObjetoTienda>(DIR_TIENDA + "/Fijo_CascoEspacial.asset") };
        EditorUtility.SetDirty(equipo);
    }

    private static void AmbienteNivel(NivelDatos d, Camera camara)
    {
        Material cielo = d.cielo != null
            ? MatCieloPanoramico("Cielo_" + d.escena, Cargar<Texture>(POLY + d.cielo + "_2k.hdr"))
            : MatCieloEstrellas();

        RenderSettings.skybox = cielo;
        RenderSettings.fog = true;
        RenderSettings.fogMode = FogMode.Linear;
        RenderSettings.fogColor = d.colorNiebla;
        RenderSettings.fogStartDistance = d.nieblaInicio;
        RenderSettings.fogEndDistance = d.nieblaFin;
        RenderSettings.ambientMode = AmbientMode.Trilight;
        RenderSettings.ambientSkyColor = d.ambienteArriba;
        RenderSettings.ambientEquatorColor = d.ambienteMedio;
        RenderSettings.ambientGroundColor = d.ambienteAbajo;

        camara.clearFlags = CameraClearFlags.Skybox;
    }

    private static Material MatCieloPanoramico(string nombre, Texture textura)
    {
        string ruta = DIR_MAT + "/" + nombre + ".mat";
        Material cielo = AssetDatabase.LoadAssetAtPath<Material>(ruta);
        if (cielo == null)
        {
            cielo = new Material(Shader.Find("Skybox/Panoramic"));
            AssetDatabase.CreateAsset(cielo, ruta);
        }

        cielo.SetTexture("_MainTex", textura);
        cielo.SetFloat("_Exposure", 1f);
        EditorUtility.SetDirty(cielo);
        return cielo;
    }

    private static GruposNivel CrearGrupos()
    {
        Transform nivel = new GameObject("Nivel").transform;
        return new GruposNivel
        {
            nivel = nivel,
            escenario = Grupo(nivel, "Escenario"),
            obstaculos = Grupo(nivel, "Obstaculos"),
            peligros = Grupo(nivel, "Peligros"),
            suministros = Grupo(nivel, "Suministros"),
            monedas = Grupo(nivel, "Monedas"),
            enemigos = Grupo(nivel, "Enemigos")
        };
    }

    private static Transform Grupo(Transform padre, string nombre)
    {
        Transform grupo = new GameObject(nombre).transform;
        grupo.SetParent(padre, false);
        return grupo;
    }

    private static void ConstruirSuelo(NivelDatos d, Transform raiz)
    {
        Material material = MatTextura("Suelo_" + d.escena, d.suelo, d.tinteSuelo, d.repetirSuelo * 3f);

        // Se extiende más allá de la zona de juego para llegar al horizonte.
        GameObject plano = Primitiva(PrimitiveType.Plane, raiz, "Suelo", Vector3.zero, new Vector3(60f, 1f, 60f), material, null, true);
        plano.GetComponent<Renderer>().shadowCastingMode = ShadowCastingMode.Off;
    }

    private static void ConstruirLimites(NivelDatos d, Transform raiz)
    {
        float m = d.medio;
        Limite(raiz, "LimiteOeste", new Vector3(-m, 10f, 0f), new Vector3(1f, 30f, m * 2f));
        Limite(raiz, "LimiteEste", new Vector3(m, 10f, 0f), new Vector3(1f, 30f, m * 2f));
        Limite(raiz, "LimiteSur", new Vector3(0f, 10f, -m), new Vector3(m * 2f, 30f, 1f));
        Limite(raiz, "LimiteNorte", new Vector3(0f, 10f, m), new Vector3(m * 2f, 30f, 1f));
    }

    private static void Limite(Transform padre, string nombre, Vector3 pos, Vector3 tam)
    {
        GameObject limite = new GameObject(nombre);
        limite.transform.SetParent(padre, false);
        limite.transform.position = pos;
        limite.AddComponent<BoxCollider>().size = tam;
    }

    /// <summary>Polvo, ceniza o brasas que siguen a la cámara.</summary>
    private static void ParticulasAmbiente(Transform camara, Color color)
    {
        GameObject go = new GameObject("ParticulasAmbiente");
        go.transform.SetParent(camara, false);
        go.transform.localPosition = new Vector3(0f, 2f, 10f);

        ParticleSystem sistema = go.AddComponent<ParticleSystem>();
        ParticleSystem.MainModule principal = sistema.main;
        principal.simulationSpace = ParticleSystemSimulationSpace.World;
        principal.startLifetime = 6f;
        principal.startSpeed = 0.4f;
        principal.startSize = new ParticleSystem.MinMaxCurve(0.03f, 0.1f);
        principal.startColor = color;
        principal.gravityModifier = 0.01f;
        principal.maxParticles = 260;
        principal.prewarm = true;

        ParticleSystem.EmissionModule emision = sistema.emission;
        emision.rateOverTime = 42f;

        ParticleSystem.ShapeModule forma = sistema.shape;
        forma.shapeType = ParticleSystemShapeType.Box;
        forma.scale = new Vector3(28f, 10f, 28f);

        go.GetComponent<ParticleSystemRenderer>().sharedMaterial = MatParticula("ParticulaFuego", true);
    }

    private static void Suministros(Transform raiz, Vector3[] jeringas, Vector3[] equipajes, Vector3[] botiquines)
    {
        const string JERINGA = "Assets/Prefabs/Recolectables/JeringaRecolectable.prefab";
        const string EQUIPAJE = "Assets/Prefabs/Recolectables/EquipajeSaludRecolectable.prefab";
        const string BOTIQUIN = "Assets/Prefabs/Recolectables/BotiquinRecolectable.prefab";

        // Grandes y a media altura para verlos de lejos.
        Vector3 alto = Vector3.up * 0.4f;
        for (int i = 0; i < jeringas.Length; i++) Poner(JERINGA, raiz, jeringas[i] + alto, 0f, ESCALA_SUMINISTRO, false).name = "Jeringa_" + (i + 1);
        for (int i = 0; i < equipajes.Length; i++) Poner(EQUIPAJE, raiz, equipajes[i] + alto, 0f, ESCALA_SUMINISTRO, false).name = "Equipaje_" + (i + 1);
        for (int i = 0; i < botiquines.Length; i++) Poner(BOTIQUIN, raiz, botiquines[i] + alto, 0f, ESCALA_SUMINISTRO, false).name = "Botiquin_" + (i + 1);
    }

    private static void Linea(Transform raiz, Vector3 desde, Vector3 hasta, int cantidad)
    {
        for (int i = 0; i < cantidad; i++)
        {
            float t = cantidad == 1 ? 0.5f : i / (float)(cantidad - 1);
            Poner(RUTA_MONEDA, raiz, Vector3.Lerp(desde, hasta, t) + Vector3.up * 0.9f, 0f, 1.4f, false);
        }
    }

    private static void Arco(Transform raiz, Vector3 desde, Vector3 hasta, int cantidad, float altura)
    {
        for (int i = 0; i < cantidad; i++)
        {
            float t = i / (float)(cantidad - 1);
            Vector3 punto = Vector3.Lerp(desde, hasta, t) + Vector3.up * (0.9f + Mathf.Sin(t * Mathf.PI) * altura);
            Poner(RUTA_MONEDA, raiz, punto, 0f, 1.4f, false);
        }
    }

    /// <summary>Caja que solo se abre con la palanca de rescate.</summary>
    private static void CajaEmergencia(Transform padre, Vector3 pos, float giro)
    {
        GameObject caja = Caja(padre, pos, giro, 2.3f);
        caja.name = "CajaEmergencia";

        GameObject sellos = new GameObject("Sellos");
        sellos.transform.SetParent(caja.transform, false);
        Material rojo = Mat("SelloRojo", new Color(0.8f, 0.1f, 0.08f), 0.3f, 0f, new Color(0.5f, 0.04f, 0.02f));
        foreach (float x in new[] { -0.35f, 0.35f })
        {
            Primitiva(PrimitiveType.Cube, sellos.transform, "Sello", new Vector3(x, 0.24f, 0f), new Vector3(0.09f, 0.5f, 0.8f), rojo);
        }

        GameObject luzGO = new GameObject("Luz");
        luzGO.transform.SetParent(caja.transform, false);
        luzGO.transform.localPosition = Vector3.up * 1.2f;
        Light luz = luzGO.AddComponent<Light>();
        luz.type = LightType.Point;
        luz.color = new Color(1f, 0.25f, 0.2f);
        luz.range = 5f;
        luz.intensity = 3f;

        SphereCollider zona = caja.AddComponent<SphereCollider>();
        zona.isTrigger = true;
        zona.radius = 2.6f / 2.3f;

        CajaSellada sellada = caja.AddComponent<CajaSellada>();
        sellada.sellos = sellos;
        sellada.luz = luz;
        sellada.botiquin = Cargar<GameObject>("Assets/Prefabs/Recolectables/BotiquinRecolectable.prefab");
        EditorUtility.SetDirty(sellada);
    }

    private static Paciente CrearPaciente(Transform nivel, Vector3 pos, float giro, int jeringas, int equipajes, Material piel, string enfermedad)
    {
        GameObject raiz = new GameObject("Paciente");
        raiz.transform.SetParent(nivel, false);
        raiz.transform.position = pos;
        raiz.transform.rotation = Quaternion.Euler(0f, giro, 0f);

        Paciente paciente = raiz.AddComponent<Paciente>();
        paciente.jeringasRequeridas = jeringas;
        paciente.equipajesRequeridos = equipajes;
        paciente.enfermedad = Cargar<Enfermedad>(DIR_SALUD + "/" + enfermedad + ".asset");

        BoxCollider zona = raiz.AddComponent<BoxCollider>();
        zona.isTrigger = true;
        zona.center = Vector3.up;
        zona.size = new Vector3(6f, 2.5f, 6f);

        GameObject camilla = Camilla(raiz.transform, pos, giro, piel);
        BoxCollider solido = camilla.AddComponent<BoxCollider>();
        solido.center = Vector3.up * 0.45f;
        solido.size = new Vector3(0.9f, 0.9f, 2.2f);

        GameObject luzGO = new GameObject("LuzTriage");
        luzGO.transform.SetParent(raiz.transform, false);
        luzGO.transform.localPosition = Vector3.up * 2.4f;
        Light luz = luzGO.AddComponent<Light>();
        luz.type = LightType.Point;
        luz.range = 9f;
        paciente.luzEstado = luz;

        GameObject estado = new GameObject("EstadoPaciente");
        estado.transform.SetParent(raiz.transform, false);
        estado.transform.localPosition = Vector3.up * 2.2f;
        paciente.textoEstado = CrearTextoMundo(estado, "", 1.8f, Color.white);
        estado.AddComponent<Billboard>();

        return paciente;
    }

    private static void ConstruirHud(Transform canvas, PlayerController jugador, GestorMision gestor)
    {
        GameObject tarjeta = PanelUI(canvas, "HUD_Estado", new Vector2(0f, 1f), new Vector2(30f, -28f), new Vector2(430f, 236f), new Color(0.06f, 0.08f, 0.11f, 0.86f));
        tarjeta.GetComponent<Image>().raycastTarget = false;

        TextoUI(tarjeta.transform, "EtiquetaSalud", "SALUD", 19f, new Vector2(0f, 1f), new Vector2(24f, -14f), new Vector2(200f, 26f), TextAlignmentOptions.Left, ColTextoSuave, true).characterSpacing = 3f;
        jugador.barraDeVida = BarraSalud(tarjeta.transform);

        TextoUI(tarjeta.transform, "EtiquetaMision", "SUMINISTROS", 19f, new Vector2(0f, 1f), new Vector2(24f, -84f), new Vector2(390f, 26f), TextAlignmentOptions.Left, ColNaranja, true).characterSpacing = 2f;
        jugador.textoJeringas = TextoUI(tarjeta.transform, "TextoJeringas", "Jeringas", 25f, new Vector2(0f, 1f), new Vector2(24f, -112f), new Vector2(380f, 34f), TextAlignmentOptions.Left, new Color(0.45f, 0.88f, 1f), true);
        jugador.textoEquipajes = TextoUI(tarjeta.transform, "TextoEquipajes", "Equipajes", 25f, new Vector2(0f, 1f), new Vector2(24f, -146f), new Vector2(380f, 34f), TextAlignmentOptions.Left, new Color(1f, 0.86f, 0.4f), true);
        gestor.textoPacientes = TextoUI(tarjeta.transform, "TextoPacientes", "Pacientes", 25f, new Vector2(0f, 1f), new Vector2(24f, -180f), new Vector2(380f, 34f), TextAlignmentOptions.Left, new Color(0.5f, 1f, 0.65f), true);

        jugador.textoSuministros = null;
        jugador.textoObjetivo = null;

        GameObject ayuda = PanelUI(canvas, "AyudaAtaque", new Vector2(0.5f, 0f), new Vector2(0f, 30f), new Vector2(430f, 52f), new Color(0.06f, 0.08f, 0.11f, 0.8f));
        ayuda.GetComponent<Image>().raycastTarget = false;
        TextoUI(ayuda.transform, "Texto", "Clic izquierdo o F para atacar", 23f, Centro, Vector2.zero, new Vector2(410f, 46f), TextAlignmentOptions.Center, Color.white, true);
        jugador.GetComponent<AtaqueJugador>().textoAyuda = ayuda.GetComponentInChildren<TextMeshProUGUI>();
        EditorUtility.SetDirty(jugador.GetComponent<AtaqueJugador>());

        ChipMonedas(canvas, new Vector2(1f, 1f), new Vector2(-300f, -30f));
    }

    private static Slider BarraSalud(Transform padre)
    {
        RectTransform rt = NuevoRect(padre, "BarraDeVida", new Vector2(0f, 1f), new Vector2(24f, -44f), new Vector2(382f, 24f));
        Slider barra = rt.gameObject.AddComponent<Slider>();

        GameObject fondo = PanelUI(rt, "Fondo", Centro, Vector2.zero, new Vector2(382f, 24f), new Color(0.16f, 0.18f, 0.22f));
        fondo.GetComponent<Image>().raycastTarget = false;

        GameObject area = new GameObject("Area", typeof(RectTransform));
        area.transform.SetParent(rt, false);
        RectTransform rtArea = (RectTransform)area.transform;
        rtArea.anchorMin = Vector2.zero;
        rtArea.anchorMax = Vector2.one;
        rtArea.sizeDelta = Vector2.zero;

        GameObject relleno = new GameObject("Relleno", typeof(RectTransform));
        relleno.transform.SetParent(area.transform, false);
        Image imagen = relleno.AddComponent<Image>();
        imagen.color = new Color(0.25f, 0.8f, 0.45f);
        imagen.raycastTarget = false;
        imagen.sprite = SpriteRedondeado;
        imagen.type = Image.Type.Sliced;
        imagen.pixelsPerUnitMultiplier = 1f;
        RectTransform rtRelleno = (RectTransform)relleno.transform;
        rtRelleno.sizeDelta = Vector2.zero;

        barra.fillRect = rtRelleno;
        barra.transition = Selectable.Transition.None;
        barra.interactable = false;
        barra.minValue = 0f;
        barra.maxValue = 100f;
        barra.value = 100f;
        return barra;
    }

    private static void ConstruirVictoria(Transform canvas, GestorMision gestor)
    {
        GameObject velo = Velo(canvas, "PanelVictoria", new Color(0.02f, 0.03f, 0.05f, 0.75f));
        GameObject tarjeta = PanelUI(velo.transform, "Tarjeta", Centro, Vector2.zero, new Vector2(920f, 580f), ColFondo);
        Cinta(tarjeta.transform, 920f);

        TextoUI(tarjeta.transform, "Antetitulo", "MISIÓN CUMPLIDA", 24f, new Vector2(0.5f, 1f), new Vector2(0f, -40f), new Vector2(800f, 34f), TextAlignmentOptions.Center, ColVerde, true).characterSpacing = 6f;
        TextoUI(tarjeta.transform, "Titulo", "Todos a salvo", 54f, new Vector2(0.5f, 1f), new Vector2(0f, -78f), new Vector2(820f, 74f), TextAlignmentOptions.Center, Color.white, true);
        gestor.textoVictoria = TextoUI(tarjeta.transform, "Mensaje", "", 26f, Centro, new Vector2(0f, -22f), new Vector2(800f, 230f), TextAlignmentOptions.Center, ColTextoSuave);

        BotonUI(tarjeta.transform, "BtnVolverHubVictoria", "VOLVER AL HUB", 24f, new Vector2(0.5f, 0f), new Vector2(0f, 36f), new Vector2(380f, 72f), ColNaranja, gestor.VolverAlHub);

        gestor.panelVictoria = velo;
        velo.SetActive(false);
    }

    private static void HornearNavMesh(NivelDatos d, GruposNivel g, GameObject jugador)
    {
        NavMeshSurface superficie = g.nivel.gameObject.AddComponent<NavMeshSurface>();
        superficie.collectObjects = CollectObjects.Volume;
        superficie.center = new Vector3(0f, 2f, 0f);
        superficie.size = new Vector3(d.medio * 2f, 8f, d.medio * 2f);
        superficie.useGeometry = NavMeshCollectGeometry.PhysicsColliders;

        jugador.SetActive(false);
        superficie.BuildNavMesh();
        jugador.SetActive(true);

        string ruta = DIR + "/NavMesh" + d.escena + ".asset";
        AssetDatabase.DeleteAsset(ruta);
        AssetDatabase.CreateAsset(superficie.navMeshData, ruta);
        EditorUtility.SetDirty(superficie);
    }

    /// <summary>Saca una foto del nivel desde arriba y la guarda como imagen del mapa.</summary>
    private static Texture2D HornearMapa(NivelDatos d, GruposNivel g, GameObject jugador)
    {
        string ruta = DIR + "/Mapa" + d.escena + ".png";

        GameObject camaraGO = new GameObject("CamaraMapa");
        camaraGO.transform.position = Vector3.up * 80f;
        camaraGO.transform.rotation = Quaternion.Euler(90f, 0f, 0f);

        Camera camara = camaraGO.AddComponent<Camera>();
        camara.orthographic = true;
        camara.orthographicSize = d.medio;
        camara.nearClipPlane = 1f;
        camara.farClipPlane = 120f;
        camara.clearFlags = CameraClearFlags.SolidColor;
        camara.backgroundColor = new Color(0.09f, 0.09f, 0.1f);

        // Se oculta lo que se mueve o tendrá su propia marca.
        bool niebla = RenderSettings.fog;
        RenderSettings.fog = false;
        jugador.SetActive(false);
        g.monedas.gameObject.SetActive(false);
        g.suministros.gameObject.SetActive(false);
        List<ParticleSystemRenderer> particulas = new List<ParticleSystemRenderer>(g.nivel.GetComponentsInChildren<ParticleSystemRenderer>());
        particulas.ForEach(p => p.enabled = false);

        RenderTexture destino = new RenderTexture(MAPA_PIXELES, MAPA_PIXELES, 24, RenderTextureFormat.ARGB32);
        camara.targetTexture = destino;
        camara.Render();

        RenderTexture.active = destino;
        Texture2D imagen = new Texture2D(MAPA_PIXELES, MAPA_PIXELES, TextureFormat.RGB24, false);
        imagen.ReadPixels(new Rect(0, 0, MAPA_PIXELES, MAPA_PIXELES), 0, 0);
        imagen.Apply();
        File.WriteAllBytes(ruta, imagen.EncodeToPNG());

        RenderTexture.active = null;
        camara.targetTexture = null;
        Object.DestroyImmediate(destino);
        Object.DestroyImmediate(imagen);
        Object.DestroyImmediate(camaraGO);

        particulas.ForEach(p => p.enabled = true);
        g.suministros.gameObject.SetActive(true);
        g.monedas.gameObject.SetActive(true);
        jugador.SetActive(true);
        RenderSettings.fog = niebla;

        AssetDatabase.ImportAsset(ruta, ImportAssetOptions.ForceUpdate);
        return Cargar<Texture2D>(ruta);
    }

    private static void ConstruirMapa(Transform canvas, PlayerController jugador, Texture2D mapa, float medio)
    {
        const float LADO = 240f;

        GameObject panel = PanelUI(canvas, "Mapa", new Vector2(1f, 1f), new Vector2(-30f, -28f), new Vector2(LADO + 16f, LADO + 94f), new Color(0.06f, 0.08f, 0.11f, 0.9f));
        panel.GetComponent<Image>().raycastTarget = false;

        RectTransform area = NuevoRect(panel.transform, "Area", new Vector2(0.5f, 1f), new Vector2(0f, -8f), new Vector2(LADO, LADO));
        RawImage fondo = area.gameObject.AddComponent<RawImage>();
        fondo.texture = mapa;
        fondo.color = new Color(0.78f, 0.78f, 0.8f);
        fondo.raycastTarget = false;

        MinimapaUI minimapa = panel.AddComponent<MinimapaUI>();
        minimapa.area = area;
        minimapa.jugador = jugador;
        minimapa.mundoMin = new Vector2(-medio, -medio);
        minimapa.mundoMax = new Vector2(medio, medio);
        minimapa.spritePunto = SpriteCirculo;
        minimapa.spriteFlecha = Cargar<Sprite>(DIR + "/IconoFlecha.png");
        minimapa.spriteCruz = Cargar<Sprite>(DIR + "/IconoCruz.png");

        Leyenda(panel.transform, 0, "Enemigo", minimapa.colorEnemigo);
        Leyenda(panel.transform, 1, "Jeringa", minimapa.colorJeringa);
        Leyenda(panel.transform, 2, "Equipaje", minimapa.colorEquipaje);
        Leyenda(panel.transform, 3, "Botiquín", minimapa.colorBotiquin);
        Leyenda(panel.transform, 4, "Moneda", minimapa.colorMoneda);
        minimapa.leyendaPaciente = Leyenda(panel.transform, 5, "Paciente", Color.white);
        minimapa.leyendaCaja = Leyenda(panel.transform, 6, "Caja", minimapa.colorCaja);

        // Debajo de los paneles de pausa y victoria.
        panel.transform.SetSiblingIndex(1);
        EditorUtility.SetDirty(minimapa);
    }

    private static GameObject Leyenda(Transform panel, int indice, string texto, Color color)
    {
        float x = 12f + (indice % 2) * 118f;
        float y = 10f + (3 - indice / 2) * 19f;

        RectTransform grupo = NuevoRect(panel, "Leyenda" + texto, Vector2.zero, Vector2.zero, Vector2.zero);
        RectTransform punto = NuevoRect(grupo, "Punto" + texto, Vector2.zero, new Vector2(x, y + 3f), new Vector2(10f, 10f));
        Image imagen = punto.gameObject.AddComponent<Image>();
        imagen.sprite = SpriteCirculo;
        imagen.color = color;
        imagen.raycastTarget = false;

        TextoUI(grupo, "Texto" + texto, texto, 15f, Vector2.zero, new Vector2(x + 14f, y - 1f), new Vector2(96f, 18f), TextAlignmentOptions.MidlineLeft, ColTextoSuave);
        return grupo.gameObject;
    }

    private static void CrearEnemigo(Transform raiz, string prefab, string nombre, Vector3[] puntos)
    {
        GameObject enemigo = (GameObject)PrefabUtility.InstantiatePrefab(Cargar<GameObject>(prefab), raiz);
        enemigo.name = nombre;
        enemigo.transform.position = puntos[0];

        Transform ruta = Grupo(raiz, nombre + "_Ruta");
        Transform[] marcas = new Transform[puntos.Length];
        for (int i = 0; i < puntos.Length; i++)
        {
            GameObject punto = new GameObject("Punto" + (i + 1));
            punto.transform.SetParent(ruta, false);
            punto.transform.position = puntos[i];
            marcas[i] = punto.transform;
        }

        EnemigoPatrulla patrulla = enemigo.GetComponent<EnemigoPatrulla>();
        patrulla.puntos = marcas;
        EditorUtility.SetDirty(patrulla);
    }
}
