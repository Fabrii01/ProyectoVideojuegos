using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Events;
using UnityEngine.UI;

/// <summary>
/// Monta las escenas del juego. Solo se ejecuta desde el menú SaludGame, nunca sola,
/// y vuelve a construir por completo lo que genera, así que no duplica nada al repetirse.
/// Los recursos que crea quedan en Assets/Generados.
/// </summary>
public static partial class MontajeRescate
{
    private const string DIR = "Assets/Generados";
    private const string DIR_MAT = DIR + "/Materiales";
    private const string DIR_PREFAB = DIR + "/Prefabs";
    private const string DIR_TIENDA = DIR + "/Tienda";
    private const string CIUDAD = "Assets/Versatile Studio Assets/Demo City By Versatile Studio/Prefabs/";
    private const string OBJETOS = "Assets/Kabungus/HouseholdItems/Prefabs/";
    private const string HOSPITAL_FBX = "Assets/Models/hospital1.fbx";
    private const string RUTA_MONEDA = DIR_PREFAB + "/Moneda.prefab";
    private const string RUTA_ENEMIGO = DIR_PREFAB + "/Saqueador.prefab";
    private const string RUTA_FURTIVO = DIR_PREFAB + "/Furtivo.prefab";
    private const string RUTA_ANDROIDE = DIR_PREFAB + "/Androide.prefab";

    private static readonly Color ColFondo = new Color(0.06f, 0.08f, 0.11f, 0.96f);
    private static readonly Color ColTarjeta = new Color(0.12f, 0.15f, 0.2f, 1f);
    private static readonly Color ColNaranja = new Color(0.95f, 0.5f, 0.1f);
    private static readonly Color ColVerde = new Color(0.13f, 0.6f, 0.45f);
    private static readonly Color ColGris = new Color(0.25f, 0.29f, 0.35f);
    private static readonly Color ColRojo = new Color(0.72f, 0.2f, 0.18f);
    private static readonly Color ColOro = new Color(1f, 0.8f, 0.25f);
    private static readonly Color ColTextoSuave = new Color(0.72f, 0.77f, 0.83f);

    [MenuItem("SaludGame/★ Montar juego completo")]
    public static void MontarTodo()
    {
        if (!Application.isBatchMode && !EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;

        CrearRecursos();
        MontarMenu();
        MontarHub();
        MontarNiveles();

        EditorBuildSettings.scenes = new[]
        {
            new EditorBuildSettingsScene("Assets/Scenes/MenuPrincipal.unity", true),
            new EditorBuildSettingsScene("Assets/Scenes/Hub.unity", true),
            new EditorBuildSettingsScene("Assets/Scenes/Nivel1.unity", true),
            new EditorBuildSettingsScene("Assets/Scenes/Nivel2.unity", true),
            new EditorBuildSettingsScene("Assets/Scenes/Nivel3.unity", true)
        };

        AssetDatabase.SaveAssets();
        EditorSceneManager.OpenScene("Assets/Scenes/MenuPrincipal.unity", OpenSceneMode.Single);
        Debug.Log("[Montaje] Juego montado: menú, Hub y tres niveles.");
    }

    [MenuItem("SaludGame/Borrar monedas y compras")]
    public static void BorrarProgreso()
    {
        DatosJugador.BorrarProgreso();
        Debug.Log("[Montaje] Progreso borrado.");
    }

    [MenuItem("SaludGame/Desbloquear todos los niveles")]
    public static void DesbloquearNiveles()
    {
        DatosJugador.CompletarNivel(3);
        Debug.Log("[Montaje] Niveles desbloqueados.");
    }

    [MenuItem("SaludGame/Dar 200 monedas de prueba")]
    public static void DarMonedas()
    {
        DatosJugador.AgregarMonedas(200);
        Debug.Log("[Montaje] Monedas actuales: " + DatosJugador.Monedas);
    }

    #region Recursos

    private static void CrearRecursos()
    {
        pielesCreadas.Clear();

        foreach (string carpeta in new[] { DIR, DIR_MAT, DIR_PREFAB, DIR_TIENDA })
        {
            Directory.CreateDirectory(carpeta);
        }
        AssetDatabase.Refresh();

        CrearTexturaParticula();
        CrearIconosMapa();
        ConfigurarAnimaciones();
        ConfigurarMusica();
        CrearRocas();
        CrearPrefabMoneda();
        CrearPrefabCasco();
        CrearPrefabMochila();
        CrearPrefabLinterna();
        CrearPrefabFuego();
        CrearPrefabHumo();
        CrearPrefabEnemigo(RUTA_ENEMIGO, "Saqueador", PielSaqueador, new Color(0.6f, 0.08f, 0.07f), 100, 15, 5.2f);
        CrearPrefabEnemigo(RUTA_FURTIVO, "Furtivo", PielFurtivo, new Color(0.2f, 0.35f, 0.12f), 100, 18, 5.6f);
        CrearPrefabEnemigo(RUTA_ANDROIDE, "Androide", PielAndroide, new Color(0.9f, 0.15f, 0.1f), 135, 20, 5.9f);
        CrearCascoEspacial();
        CrearCatalogo();
        CrearBancoSalud();

        AssetDatabase.SaveAssets();
    }

    private static void ConfigurarMusica()
    {
        foreach (string guid in AssetDatabase.FindAssets("t:AudioClip", new[] { "Assets/Externos/Musica" }))
        {
            string ruta = AssetDatabase.GUIDToAssetPath(guid);
            AudioImporter importador = (AudioImporter)AssetImporter.GetAtPath(ruta);
            AudioImporterSampleSettings ajustes = importador.defaultSampleSettings;
            if (ajustes.loadType == AudioClipLoadType.Streaming) continue;

            ajustes.loadType = AudioClipLoadType.Streaming;
            importador.defaultSampleSettings = ajustes;
            importador.SaveAndReimport();
        }
    }

    private static T Cargar<T>(string ruta) where T : Object
    {
        T recurso = AssetDatabase.LoadAssetAtPath<T>(ruta);
        if (recurso == null) Debug.LogError("[Montaje] No se encontró: " + ruta);
        return recurso;
    }

    private static Material Mat(string nombre, Color color, float suavidad = 0.2f, float metal = 0f, Color? emision = null)
    {
        string ruta = DIR_MAT + "/" + nombre + ".mat";
        Material material = AssetDatabase.LoadAssetAtPath<Material>(ruta);
        if (material == null)
        {
            material = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            AssetDatabase.CreateAsset(material, ruta);
        }

        material.SetColor("_BaseColor", color);
        material.SetFloat("_Smoothness", suavidad);
        material.SetFloat("_Metallic", metal);

        if (emision.HasValue)
        {
            material.EnableKeyword("_EMISSION");
            material.SetColor("_EmissionColor", emision.Value);
            material.globalIlluminationFlags = MaterialGlobalIlluminationFlags.None;
        }

        EditorUtility.SetDirty(material);
        return material;
    }

    /// <summary>Copia un material con textura para teñirlo o repetir su textura.</summary>
    private static Material CopiaMat(string nombre, string origen, Color tinte, float repetir = 1f)
    {
        string ruta = DIR_MAT + "/" + nombre + ".mat";
        Material material = AssetDatabase.LoadAssetAtPath<Material>(ruta);
        if (material == null)
        {
            material = new Material(Cargar<Material>(origen));
            AssetDatabase.CreateAsset(material, ruta);
        }

        material.SetColor("_BaseColor", tinte);
        material.SetTextureScale("_BaseMap", Vector2.one * repetir);
        EditorUtility.SetDirty(material);
        return material;
    }

    /// <summary>Suelo de calle con textura repetida.</summary>
    private static Material MatSuelo()
    {
        // El material original usa un shader que URP no dibuja, así que solo se toma su textura.
        Material original = Cargar<Material>("Assets/Versatile Studio Assets/Demo City By Versatile Studio/Materials/asphalt_extra.mat");
        Material suelo = Mat("SueloCalle", new Color(0.72f, 0.7f, 0.68f), 0.15f);
        suelo.SetTexture("_BaseMap", original.GetTexture("_MainTex"));
        suelo.SetTextureScale("_BaseMap", Vector2.one * 30f);
        EditorUtility.SetDirty(suelo);
        return suelo;
    }

    private static void CrearTexturaParticula()
    {
        string ruta = DIR + "/Particula.png";
        if (!File.Exists(ruta))
        {
            const int lado = 128;
            Texture2D textura = new Texture2D(lado, lado, TextureFormat.RGBA32, false);
            for (int y = 0; y < lado; y++)
            {
                for (int x = 0; x < lado; x++)
                {
                    float dx = (x + 0.5f) / lado * 2f - 1f;
                    float dy = (y + 0.5f) / lado * 2f - 1f;
                    float alfa = Mathf.Clamp01(1f - Mathf.Sqrt(dx * dx + dy * dy));
                    textura.SetPixel(x, y, new Color(1f, 1f, 1f, alfa * alfa * (3f - 2f * alfa)));
                }
            }
            File.WriteAllBytes(ruta, textura.EncodeToPNG());
            AssetDatabase.ImportAsset(ruta);

            TextureImporter importador = (TextureImporter)AssetImporter.GetAtPath(ruta);
            importador.alphaIsTransparency = true;
            importador.SaveAndReimport();
        }
    }

    /// <summary>Crea las imágenes de la flecha del jugador y la cruz del paciente.</summary>
    private static void CrearIconosMapa()
    {
        CrearIcono(DIR + "/IconoFlecha.png", (x, y) =>
        {
            float ancho = (1f - y) * 0.46f;
            bool cuerpo = Mathf.Abs(x - 0.5f) < ancho && y > 0.08f;
            bool muesca = y < 0.32f && Mathf.Abs(x - 0.5f) < (0.32f - y) * 1.1f;
            return cuerpo && !muesca;
        });

        CrearIcono(DIR + "/IconoCruz.png", (x, y) =>
        {
            float dx = Mathf.Abs(x - 0.5f);
            float dy = Mathf.Abs(y - 0.5f);
            return (dx < 0.16f && dy < 0.46f) || (dy < 0.16f && dx < 0.46f);
        });
    }

    private static void CrearIcono(string ruta, System.Func<float, float, bool> relleno)
    {
        if (!File.Exists(ruta))
        {
            const int lado = 64;
            Texture2D textura = new Texture2D(lado, lado, TextureFormat.RGBA32, false);
            for (int y = 0; y < lado; y++)
            {
                for (int x = 0; x < lado; x++)
                {
                    bool lleno = relleno((x + 0.5f) / lado, (y + 0.5f) / lado);
                    textura.SetPixel(x, y, lleno ? Color.white : new Color(1f, 1f, 1f, 0f));
                }
            }
            File.WriteAllBytes(ruta, textura.EncodeToPNG());
            AssetDatabase.ImportAsset(ruta);
        }

        TextureImporter importador = (TextureImporter)AssetImporter.GetAtPath(ruta);
        if (importador.textureType != TextureImporterType.Sprite || importador.spriteImportMode != SpriteImportMode.Single)
        {
            importador.textureType = TextureImporterType.Sprite;
            importador.spriteImportMode = SpriteImportMode.Single;
            importador.alphaIsTransparency = true;
            importador.SaveAndReimport();
        }
    }

    private static Material MatParticula(string nombre, bool aditivo)
    {
        string ruta = DIR_MAT + "/" + nombre + ".mat";
        Material material = AssetDatabase.LoadAssetAtPath<Material>(ruta);
        if (material == null)
        {
            material = new Material(Shader.Find("Universal Render Pipeline/Particles/Unlit"));
            AssetDatabase.CreateAsset(material, ruta);
        }

        material.SetTexture("_BaseMap", Cargar<Texture2D>(DIR + "/Particula.png"));
        material.SetFloat("_Surface", 1f);
        material.SetFloat("_Blend", aditivo ? 2f : 0f);
        material.SetFloat("_ZWrite", 0f);
        material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
        material.SetInt("_DstBlend", (int)(aditivo ? UnityEngine.Rendering.BlendMode.One : UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha));
        material.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;
        material.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
        material.SetOverrideTag("RenderType", "Transparent");
        EditorUtility.SetDirty(material);
        return material;
    }

    /// <summary>Crea tres rocas irregulares deformando una esfera, porque los paquetes no traen rocas.</summary>
    private static void CrearRocas()
    {
        Material gris = Mat("Roca", new Color(0.37f, 0.35f, 0.33f), 0.08f);
        Material oscura = Mat("RocaOscura", new Color(0.25f, 0.24f, 0.24f), 0.08f);

        for (int i = 1; i <= 3; i++)
        {
            string rutaMalla = DIR_PREFAB + "/MallaRoca" + i + ".asset";
            Mesh malla = AssetDatabase.LoadAssetAtPath<Mesh>(rutaMalla);
            if (malla == null)
            {
                malla = GenerarRoca(i * 37);
                AssetDatabase.CreateAsset(malla, rutaMalla);
            }

            GameObject roca = new GameObject("Roca" + i);
            roca.AddComponent<MeshFilter>().sharedMesh = malla;
            roca.AddComponent<MeshRenderer>().sharedMaterial = i == 2 ? oscura : gris;
            MeshCollider colision = roca.AddComponent<MeshCollider>();
            colision.sharedMesh = malla;
            colision.convex = true;
            PrefabUtility.SaveAsPrefabAsset(roca, DIR_PREFAB + "/Roca" + i + ".prefab");
            Object.DestroyImmediate(roca);
        }
    }

    private static Mesh GenerarRoca(int semilla)
    {
        GameObject esfera = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        Mesh baseMalla = esfera.GetComponent<MeshFilter>().sharedMesh;
        Object.DestroyImmediate(esfera);

        Vector3[] vertices = baseMalla.vertices;
        int[] triangulos = baseMalla.triangles;

        // La deformación depende de la dirección para que las costuras de la esfera sigan unidas.
        for (int i = 0; i < vertices.Length; i++)
        {
            Vector3 d = vertices[i].normalized;
            float ruido = Mathf.PerlinNoise(d.x * 1.7f + semilla, d.z * 1.7f + d.y * 1.3f + semilla);
            float corte = Mathf.PerlinNoise(d.y * 3.1f + semilla * 0.5f, d.x * 3.1f - d.z * 2.3f);
            float radio = 0.34f + ruido * 0.26f + Mathf.Round(corte * 4f) / 4f * 0.1f;
            Vector3 v = d * radio;
            v.y *= 0.75f;
            vertices[i] = v;
        }

        // Vértices sin compartir para que cada cara quede plana, como piedra partida.
        Vector3[] planos = new Vector3[triangulos.Length];
        int[] indices = new int[triangulos.Length];
        for (int i = 0; i < triangulos.Length; i++)
        {
            planos[i] = vertices[triangulos[i]];
            indices[i] = i;
        }

        Mesh malla = new Mesh { name = "Roca" + semilla, vertices = planos, triangles = indices };
        malla.RecalculateNormals();
        malla.RecalculateBounds();
        return malla;
    }

    private static GameObject Primitiva(PrimitiveType tipo, Transform padre, string nombre, Vector3 pos, Vector3 escala, Material material, Vector3? giro = null, bool colision = false)
    {
        GameObject go = GameObject.CreatePrimitive(tipo);
        go.name = nombre;
        go.transform.SetParent(padre, false);
        go.transform.localPosition = pos;
        go.transform.localScale = escala;
        go.transform.localRotation = Quaternion.Euler(giro ?? Vector3.zero);
        go.GetComponent<Renderer>().sharedMaterial = material;

        if (!colision) Object.DestroyImmediate(go.GetComponent<Collider>());
        return go;
    }

    private static void CrearPrefabMoneda()
    {
        Material oro = Mat("Oro", new Color(1f, 0.72f, 0.12f), 0.8f, 0.9f, new Color(0.9f, 0.5f, 0.05f));
        Material centro = Mat("OroClaro", new Color(1f, 0.88f, 0.4f), 0.8f, 0.9f, new Color(0.8f, 0.55f, 0.1f));

        GameObject raiz = new GameObject("Moneda");
        SphereCollider colision = raiz.AddComponent<SphereCollider>();
        colision.isTrigger = true;
        colision.radius = 0.7f;
        raiz.AddComponent<MonedaRecolectable>().valor = 2;

        Primitiva(PrimitiveType.Cylinder, raiz.transform, "Disco", Vector3.zero, new Vector3(0.6f, 0.04f, 0.6f), oro, new Vector3(90f, 0f, 0f));
        Primitiva(PrimitiveType.Cylinder, raiz.transform, "Centro", Vector3.zero, new Vector3(0.38f, 0.055f, 0.38f), centro, new Vector3(90f, 0f, 0f));

        PrefabUtility.SaveAsPrefabAsset(raiz, RUTA_MONEDA);
        Object.DestroyImmediate(raiz);
    }

    private static void CrearPrefabCasco()
    {
        Material amarillo = Mat("CascoAmarillo", new Color(0.98f, 0.72f, 0.08f), 0.55f);
        Material oscuro = Mat("Goma", new Color(0.1f, 0.1f, 0.11f), 0.2f);
        Material foco = Mat("Foco", Color.white, 0.8f, 0f, new Color(1f, 0.95f, 0.75f));

        GameObject raiz = new GameObject("CascoRescate");
        Primitiva(PrimitiveType.Sphere, raiz.transform, "Cupula", Vector3.zero, new Vector3(0.27f, 0.2f, 0.31f), amarillo);
        Primitiva(PrimitiveType.Cylinder, raiz.transform, "Ala", new Vector3(0f, -0.035f, 0.015f), new Vector3(0.31f, 0.008f, 0.36f), amarillo);
        Primitiva(PrimitiveType.Cube, raiz.transform, "Cresta", new Vector3(0f, 0.09f, 0f), new Vector3(0.04f, 0.04f, 0.26f), amarillo);
        Primitiva(PrimitiveType.Cube, raiz.transform, "Banda", new Vector3(0f, 0f, 0.15f), new Vector3(0.11f, 0.06f, 0.03f), oscuro);
        Primitiva(PrimitiveType.Cube, raiz.transform, "Foco", new Vector3(0f, 0f, 0.165f), new Vector3(0.07f, 0.04f, 0.01f), foco);

        PrefabUtility.SaveAsPrefabAsset(raiz, DIR_TIENDA + "/CascoRescate.prefab");
        Object.DestroyImmediate(raiz);
    }

    private static void CrearPrefabMochila()
    {
        Material rojo = Mat("MochilaRoja", new Color(0.72f, 0.1f, 0.1f), 0.25f);
        Material rojoOscuro = Mat("MochilaRojaOscura", new Color(0.42f, 0.06f, 0.07f), 0.25f);
        Material blanco = Mat("Blanco", new Color(0.92f, 0.92f, 0.9f), 0.3f);
        Material gris = Mat("Lona", new Color(0.3f, 0.31f, 0.3f), 0.15f);

        GameObject raiz = new GameObject("MochilaRescate");
        Primitiva(PrimitiveType.Cube, raiz.transform, "Cuerpo", Vector3.zero, new Vector3(0.4f, 0.52f, 0.2f), rojo);
        Primitiva(PrimitiveType.Cube, raiz.transform, "Bolsillo", new Vector3(0f, -0.12f, -0.12f), new Vector3(0.32f, 0.2f, 0.07f), rojoOscuro);
        Primitiva(PrimitiveType.Cube, raiz.transform, "CruzV", new Vector3(0f, 0.11f, -0.105f), new Vector3(0.05f, 0.16f, 0.01f), blanco);
        Primitiva(PrimitiveType.Cube, raiz.transform, "CruzH", new Vector3(0f, 0.11f, -0.105f), new Vector3(0.16f, 0.05f, 0.01f), blanco);
        Primitiva(PrimitiveType.Cylinder, raiz.transform, "Manta", new Vector3(0f, 0.32f, 0f), new Vector3(0.14f, 0.2f, 0.14f), gris, new Vector3(0f, 0f, 90f));

        PrefabUtility.SaveAsPrefabAsset(raiz, DIR_TIENDA + "/MochilaRescate.prefab");
        Object.DestroyImmediate(raiz);
    }

    private static void CrearPrefabLinterna()
    {
        GameObject raiz = new GameObject("LinternaPecho");
        GameObject modelo = (GameObject)PrefabUtility.InstantiatePrefab(Cargar<GameObject>(OBJETOS + "Flashlight.prefab"), raiz.transform);
        modelo.transform.localPosition = Vector3.zero;
        modelo.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);

        GameObject foco = new GameObject("Haz");
        foco.transform.SetParent(raiz.transform, false);
        foco.transform.localPosition = new Vector3(0f, 0f, 0.15f);
        foco.transform.localRotation = Quaternion.Euler(8f, 0f, 0f);
        Light luz = foco.AddComponent<Light>();
        luz.type = LightType.Spot;
        luz.range = 16f;
        luz.spotAngle = 55f;
        luz.intensity = 30f;
        luz.color = new Color(1f, 0.95f, 0.82f);

        PrefabUtility.SaveAsPrefabAsset(raiz, DIR_TIENDA + "/LinternaPecho.prefab");
        Object.DestroyImmediate(raiz);
    }

    private static void CrearPrefabFuego()
    {
        GameObject raiz = new GameObject("Fuego");
        ParticleSystem sistema = raiz.AddComponent<ParticleSystem>();

        ParticleSystem.MainModule principal = sistema.main;
        principal.startLifetime = new ParticleSystem.MinMaxCurve(0.5f, 1.1f);
        principal.startSpeed = new ParticleSystem.MinMaxCurve(1.2f, 2.8f);
        principal.startSize = new ParticleSystem.MinMaxCurve(0.6f, 1.3f);
        principal.startColor = new ParticleSystem.MinMaxGradient(new Color(1f, 0.55f, 0.1f, 0.9f), new Color(1f, 0.25f, 0.03f, 0.75f));
        principal.maxParticles = 90;

        ParticleSystem.EmissionModule emision = sistema.emission;
        emision.rateOverTime = 45f;

        ParticleSystem.ShapeModule forma = sistema.shape;
        forma.shapeType = ParticleSystemShapeType.Box;
        forma.scale = new Vector3(2f, 0.1f, 2f);
        forma.rotation = new Vector3(-90f, 0f, 0f);

        ParticleSystem.SizeOverLifetimeModule tamano = sistema.sizeOverLifetime;
        tamano.enabled = true;
        tamano.size = new ParticleSystem.MinMaxCurve(1f, AnimationCurve.EaseInOut(0f, 1f, 1f, 0.15f));

        ParticleSystem.ColorOverLifetimeModule color = sistema.colorOverLifetime;
        color.enabled = true;
        Gradient degradado = new Gradient();
        degradado.SetKeys(
            new[] { new GradientColorKey(Color.white, 0f), new GradientColorKey(new Color(1f, 0.3f, 0.1f), 1f) },
            new[] { new GradientAlphaKey(0f, 0f), new GradientAlphaKey(1f, 0.15f), new GradientAlphaKey(0f, 1f) });
        color.color = degradado;

        raiz.GetComponent<ParticleSystemRenderer>().sharedMaterial = MatParticula("ParticulaFuego", true);

        GameObject brillo = new GameObject("Brillo");
        brillo.transform.SetParent(raiz.transform, false);
        brillo.transform.localPosition = Vector3.up * 1.2f;
        Light luz = brillo.AddComponent<Light>();
        luz.type = LightType.Point;
        luz.color = new Color(1f, 0.5f, 0.15f);
        luz.range = 9f;
        luz.intensity = 12f;

        BoxCollider zona = raiz.AddComponent<BoxCollider>();
        zona.isTrigger = true;
        zona.center = Vector3.up * 0.9f;
        zona.size = new Vector3(2.2f, 1.8f, 2.2f);
        raiz.AddComponent<ZonaPeligro>().danio = 10;

        PrefabUtility.SaveAsPrefabAsset(raiz, DIR_PREFAB + "/Fuego.prefab");
        Object.DestroyImmediate(raiz);
    }

    private static void CrearPrefabHumo()
    {
        GameObject raiz = new GameObject("Humo");
        ParticleSystem sistema = raiz.AddComponent<ParticleSystem>();

        ParticleSystem.MainModule principal = sistema.main;
        principal.startLifetime = new ParticleSystem.MinMaxCurve(6f, 10f);
        principal.startSpeed = new ParticleSystem.MinMaxCurve(1.5f, 3f);
        principal.startSize = new ParticleSystem.MinMaxCurve(3f, 6f);
        principal.startColor = new ParticleSystem.MinMaxGradient(new Color(0.14f, 0.13f, 0.13f, 0.5f), new Color(0.3f, 0.28f, 0.27f, 0.4f));
        principal.maxParticles = 60;
        principal.prewarm = true;

        ParticleSystem.EmissionModule emision = sistema.emission;
        emision.rateOverTime = 6f;

        ParticleSystem.ShapeModule forma = sistema.shape;
        forma.shapeType = ParticleSystemShapeType.Cone;
        forma.angle = 12f;
        forma.radius = 1.2f;
        forma.rotation = new Vector3(-90f, 0f, 0f);

        ParticleSystem.SizeOverLifetimeModule tamano = sistema.sizeOverLifetime;
        tamano.enabled = true;
        tamano.size = new ParticleSystem.MinMaxCurve(1f, AnimationCurve.Linear(0f, 0.5f, 1f, 2f));

        ParticleSystem.ColorOverLifetimeModule color = sistema.colorOverLifetime;
        color.enabled = true;
        Gradient degradado = new Gradient();
        degradado.SetKeys(
            new[] { new GradientColorKey(Color.white, 0f), new GradientColorKey(Color.white, 1f) },
            new[] { new GradientAlphaKey(0f, 0f), new GradientAlphaKey(1f, 0.2f), new GradientAlphaKey(0f, 1f) });
        color.color = degradado;

        raiz.GetComponent<ParticleSystemRenderer>().sharedMaterial = MatParticula("ParticulaHumo", false);

        PrefabUtility.SaveAsPrefabAsset(raiz, DIR_PREFAB + "/Humo.prefab");
        Object.DestroyImmediate(raiz);
    }

    private static void CrearPrefabEnemigo(string ruta, string nombre, Material piel, Color colorPano, int vidaEnemigo, int danio, float velocidad)
    {
        Material pano = Mat("Pano" + nombre, colorPano, 0.1f);

        GameObject raiz = new GameObject(nombre);

        NavMeshAgent agente = raiz.AddComponent<NavMeshAgent>();
        agente.radius = 0.4f;
        agente.height = 1.85f;
        agente.angularSpeed = 300f;
        agente.acceleration = 14f;
        agente.stoppingDistance = 0.3f;

        CapsuleCollider colision = raiz.AddComponent<CapsuleCollider>();
        colision.center = Vector3.up * 0.95f;
        colision.height = 1.9f;
        colision.radius = 0.4f;

        GameObject modelo = CrearPersonaje(raiz.transform, "Modelo", piel, 0f);

        // Pañuelo rojo atado a la cabeza para distinguirlo a simple vista.
        Transform cabeza = Hueso(modelo, "Head");
        GameObject panuelo = Primitiva(PrimitiveType.Sphere, raiz.transform, "Panuelo", Vector3.zero, new Vector3(0.36f, 0.14f, 0.36f), pano);
        panuelo.transform.position = cabeza.position + Vector3.up * 0.2f;
        panuelo.transform.SetParent(cabeza, true);

        GameObject alerta = new GameObject("Alerta");
        alerta.transform.SetParent(raiz.transform, false);
        alerta.transform.localPosition = Vector3.up * 2.45f;
        TextMeshPro signo = CrearTextoMundo(alerta, "!", 5f, new Color(1f, 0.15f, 0.1f));
        alerta.AddComponent<Billboard>();

        GameObject barra = new GameObject("BarraVida");
        barra.transform.SetParent(raiz.transform, false);
        barra.transform.localPosition = Vector3.up * 2.15f;
        barra.AddComponent<Billboard>();
        Material fondo = Mat("BarraFondo", new Color(0.05f, 0.05f, 0.06f), 0f, 0f, new Color(0.02f, 0.02f, 0.02f));
        Material vida = Mat("BarraVida", new Color(0.85f, 0.12f, 0.1f), 0f, 0f, new Color(0.8f, 0.1f, 0.08f));
        Primitiva(PrimitiveType.Quad, barra.transform, "Fondo", Vector3.zero, new Vector3(1.04f, 0.14f, 1f), fondo);
        GameObject relleno = Primitiva(PrimitiveType.Quad, barra.transform, "Relleno", new Vector3(0f, 0f, -0.01f), Vector3.one, vida);
        barra.transform.localScale = new Vector3(1f, 0.1f, 1f);
        barra.transform.Find("Fondo").localScale = new Vector3(1.04f, 1.4f, 1f);

        GameObject luzGO = new GameObject("LuzEstado");
        luzGO.transform.SetParent(raiz.transform, false);
        luzGO.transform.localPosition = new Vector3(0f, 1.5f, 0.8f);
        Light luz = luzGO.AddComponent<Light>();
        luz.type = LightType.Point;
        luz.range = 6f;
        luz.intensity = 4f;

        EnemigoPatrulla enemigo = raiz.AddComponent<EnemigoPatrulla>();
        enemigo.indicadorAlerta = signo;
        enemigo.luzEstado = luz;
        enemigo.animador = modelo.GetComponent<Animator>();
        enemigo.barraVida = barra;
        enemigo.rellenoVida = relleno.transform;
        enemigo.premio = Cargar<GameObject>(RUTA_MONEDA);
        enemigo.vidaMaxima = vidaEnemigo;
        enemigo.danio = danio;
        enemigo.velocidadPersecucion = velocidad;

        PrefabUtility.SaveAsPrefabAsset(raiz, ruta);
        Object.DestroyImmediate(raiz);
    }

    /// <summary>Burbuja de vidrio del traje espacial; no se vende, la pone el nivel lunar.</summary>
    private static void CrearCascoEspacial()
    {
        string ruta = DIR_MAT + "/VidrioCasco.mat";
        Material vidrio = AssetDatabase.LoadAssetAtPath<Material>(ruta);
        if (vidrio == null)
        {
            vidrio = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            AssetDatabase.CreateAsset(vidrio, ruta);
        }

        vidrio.SetFloat("_Surface", 1f);
        vidrio.SetFloat("_Blend", 0f);
        vidrio.SetFloat("_ZWrite", 0f);
        vidrio.SetFloat("_Smoothness", 0.95f);
        vidrio.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
        vidrio.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
        vidrio.SetColor("_BaseColor", new Color(0.6f, 0.82f, 1f, 0.28f));
        vidrio.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
        vidrio.SetOverrideTag("RenderType", "Transparent");
        vidrio.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;
        EditorUtility.SetDirty(vidrio);

        GameObject raiz = new GameObject("CascoEspacial");
        Primitiva(PrimitiveType.Sphere, raiz.transform, "Burbuja", Vector3.zero, Vector3.one * 0.46f, vidrio);
        Primitiva(PrimitiveType.Cylinder, raiz.transform, "Cuello", new Vector3(0f, -0.21f, 0f), new Vector3(0.3f, 0.03f, 0.3f), Mat("Blanco", new Color(0.92f, 0.92f, 0.9f), 0.3f));
        PrefabUtility.SaveAsPrefabAsset(raiz, DIR_TIENDA + "/CascoEspacial.prefab");
        Object.DestroyImmediate(raiz);

        ObjetoTienda objeto = ObtenerAsset<ObjetoTienda>(DIR_TIENDA + "/Fijo_CascoEspacial.asset");
        objeto.id = "casco_espacial";
        objeto.nombre = "Casco espacial";
        objeto.prefabVisual = Cargar<GameObject>(DIR_TIENDA + "/CascoEspacial.prefab");
        objeto.punto = PuntoEquipo.Cabeza;
        objeto.escala = 1f;
        CalcularEnHueso(objeto, new Vector3(0f, 0.15f, 0.03f), Vector3.zero);
        EditorUtility.SetDirty(objeto);
    }

    private static void CrearCatalogo()
    {
        CatalogoTienda catalogo = ObtenerAsset<CatalogoTienda>(DIR_TIENDA + "/CatalogoTienda.asset");
        catalogo.objetos.Clear();

        catalogo.objetos.Add(CrearObjeto("bate", "Bate de defensa", "Tu arma. Clic izquierdo o F para golpear.", 80,
            new Color(0.8f, 0.3f, 0.2f), Cargar<GameObject>(OBJETOS + "Bat.prefab"), PuntoEquipo.ManoDerecha,
            new Vector3(0f, -0.03f, 0.03f), new Vector3(35f, 0f, 0f), 0.75f, 0, 0f, 34));

        catalogo.objetos.Add(CrearObjeto("casco", "Casco de rescate", "Protege en derrumbes. +25 de salud máxima.", 40,
            new Color(0.98f, 0.72f, 0.08f), Cargar<GameObject>(DIR_TIENDA + "/CascoRescate.prefab"), PuntoEquipo.Cabeza,
            new Vector3(0f, 0.21f, 0.02f), Vector3.zero, 1.15f, 25, 0f));

        catalogo.objetos.Add(CrearObjeto("mochila", "Mochila de rescate", "Reparte mejor la carga. +1 de velocidad.", 60,
            new Color(0.75f, 0.14f, 0.13f), Cargar<GameObject>(DIR_TIENDA + "/MochilaRescate.prefab"), PuntoEquipo.Espalda,
            new Vector3(0f, -0.08f, -0.13f), Vector3.zero, 0.8f, 0, 1f));

        catalogo.objetos.Add(CrearObjeto("linterna", "Linterna de pecho", "Ilumina el camino entre el polvo y el humo.", 30,
            new Color(0.3f, 0.65f, 0.95f), Cargar<GameObject>(DIR_TIENDA + "/LinternaPecho.prefab"), PuntoEquipo.Pecho,
            new Vector3(0.09f, -0.02f, 0.14f), Vector3.zero, 0.55f, 0, 0f));

        catalogo.objetos.Add(CrearObjeto("palanca", "Palanca de rescate", "Sirve para abrir paso entre los escombros.", 50,
            new Color(0.5f, 0.55f, 0.6f), Cargar<GameObject>(OBJETOS + "Crowbar.prefab"), PuntoEquipo.Cintura,
            new Vector3(-0.2f, 0f, 0f), new Vector3(0f, 90f, 8f), 0.9f, 0, 0f));

        catalogo.objetos.Add(CrearObjeto("prismaticos", "Prismáticos", "Para ubicar a los heridos desde lejos.", 35,
            new Color(0.2f, 0.45f, 0.35f), Cargar<GameObject>(OBJETOS + "Binoculars.prefab"), PuntoEquipo.Pecho,
            new Vector3(-0.05f, -0.14f, 0.13f), Vector3.zero, 0.6f, 0, 0f));

        catalogo.objetos.Add(CrearObjeto("botiquin", "Botiquín de cintura", "Primeros auxilios a mano. +15 de salud máxima.", 45,
            new Color(0.2f, 0.7f, 0.4f), Cargar<GameObject>(OBJETOS + "Medkit.prefab"), PuntoEquipo.Cintura,
            new Vector3(0.2f, 0f, 0f), new Vector3(0f, 90f, 0f), 0.36f, 15, 0f));

        EditorUtility.SetDirty(catalogo);
    }

    private static T ObtenerAsset<T>(string ruta) where T : ScriptableObject
    {
        T recurso = AssetDatabase.LoadAssetAtPath<T>(ruta);
        if (recurso == null)
        {
            recurso = ScriptableObject.CreateInstance<T>();
            AssetDatabase.CreateAsset(recurso, ruta);
        }
        return recurso;
    }

    private static ObjetoTienda CrearObjeto(string id, string nombre, string descripcion, int precio, Color color, GameObject prefab,
        PuntoEquipo punto, Vector3 desplazamiento, Vector3 giro, float escala, int vida, float velocidad, int danio = 0)
    {
        ObjetoTienda objeto = ObtenerAsset<ObjetoTienda>(DIR_TIENDA + "/Objeto_" + id + ".asset");
        objeto.id = id;
        objeto.nombre = nombre;
        objeto.descripcion = descripcion;
        objeto.precio = precio;
        objeto.colorIcono = color;
        objeto.prefabVisual = prefab;
        objeto.punto = punto;
        objeto.escala = escala;
        objeto.vidaExtra = vida;
        objeto.velocidadExtra = velocidad;
        objeto.danioArma = danio;
        CalcularEnHueso(objeto, desplazamiento, giro);
        EditorUtility.SetDirty(objeto);
        return objeto;
    }

    #endregion

    #region Piezas del escenario

    /// <summary>Instancia un prefab y le pone una colisión de caja según lo que mide.</summary>
    private static GameObject Poner(string ruta, Transform padre, Vector3 pos, float giroY = 0f, float escala = 1f, bool solido = true)
    {
        GameObject go = (GameObject)PrefabUtility.InstantiatePrefab(Cargar<GameObject>(ruta), padre);
        go.transform.position = Vector3.zero;
        go.transform.rotation = Quaternion.identity;
        go.transform.localScale = Vector3.one * escala;

        if (solido && go.GetComponentInChildren<Collider>() == null)
        {
            Bounds caja = MedirCaja(go);
            BoxCollider colision = go.AddComponent<BoxCollider>();
            colision.center = caja.center / escala;
            colision.size = caja.size / escala;
        }

        go.transform.position = pos;
        go.transform.rotation = Quaternion.Euler(0f, giroY, 0f);
        return go;
    }

    private static Bounds MedirCaja(GameObject go)
    {
        Bounds caja = new Bounds(go.transform.position, Vector3.zero);
        bool primero = true;
        foreach (Renderer parte in go.GetComponentsInChildren<Renderer>())
        {
            if (parte is ParticleSystemRenderer) continue;
            if (primero) { caja = parte.bounds; primero = false; }
            else caja.Encapsulate(parte.bounds);
        }
        return caja;
    }

    private static GameObject Roca(Transform padre, Vector3 pos, float tamano, float giroY, int tipo = 1)
    {
        GameObject roca = (GameObject)PrefabUtility.InstantiatePrefab(Cargar<GameObject>(DIR_PREFAB + "/Roca" + tipo + ".prefab"), padre);
        roca.transform.position = pos;
        roca.transform.rotation = Quaternion.Euler(0f, giroY, 0f);
        roca.transform.localScale = new Vector3(tamano * 1.2f, tamano, tamano);
        return roca;
    }

    /// <summary>Losa de concreto que se puede inclinar para simular un techo o muro caído.</summary>
    private static GameObject Losa(Transform padre, Vector3 pos, Vector3 tamano, Vector3 giro)
    {
        GameObject losa = Primitiva(PrimitiveType.Cube, padre, "Losa", Vector3.zero, tamano, Mat("Concreto", new Color(0.46f, 0.45f, 0.43f), 0.1f), giro, true);
        losa.transform.position = pos;
        return losa;
    }

    private static GameObject Caja(Transform padre, Vector3 pos, float giroY, float escala = 1.6f)
    {
        return Poner(OBJETOS + "AmmoCrate.prefab", padre, pos, giroY, escala);
    }

    private static GameObject Fuego(Transform padre, Vector3 pos, bool peligroso = true)
    {
        GameObject fuego = Poner(DIR_PREFAB + "/Fuego.prefab", padre, pos, 0f, 1f, false);
        if (!peligroso)
        {
            Object.DestroyImmediate(fuego.GetComponent<ZonaPeligro>());
            Object.DestroyImmediate(fuego.GetComponent<BoxCollider>());
        }
        return fuego;
    }

    private static GameObject Humo(Transform padre, Vector3 pos)
    {
        return Poner(DIR_PREFAB + "/Humo.prefab", padre, pos, 0f, 1f, false);
    }

    /// <summary>Saca una ambulancia del modelo del hospital, a tamaño real.</summary>
    private static GameObject Ambulancia(Transform padre, Vector3 pos, float giroY)
    {
        GameObject hospital = Object.Instantiate(Cargar<GameObject>(HOSPITAL_FBX));
        GameObject modelo = hospital.transform.Find("Ambulance").gameObject;
        Vector3 escala = modelo.transform.localScale * 2.7f;
        modelo.transform.SetParent(null, true);
        Object.DestroyImmediate(hospital);

        GameObject raiz = new GameObject("Ambulancia");
        raiz.transform.SetParent(padre, false);
        modelo.transform.SetParent(raiz.transform, false);
        modelo.transform.localPosition = Vector3.zero;
        modelo.transform.localScale = escala;

        Bounds caja = MedirCaja(raiz);
        modelo.transform.localPosition = new Vector3(-caja.center.x, -caja.min.y, -caja.center.z);

        BoxCollider colision = raiz.AddComponent<BoxCollider>();
        colision.center = Vector3.up * caja.size.y * 0.5f;
        colision.size = caja.size;

        GameObject luces = new GameObject("Sirena");
        luces.transform.SetParent(raiz.transform, false);
        luces.transform.localPosition = Vector3.up * (caja.size.y + 0.4f);
        Light azul = CrearLuz(luces.transform, new Vector3(-0.8f, 0f, 0f), new Color(0.15f, 0.35f, 1f), 16f);
        Light roja = CrearLuz(luces.transform, new Vector3(0.8f, 0f, 0f), new Color(1f, 0.1f, 0.08f), 16f);
        LuzEmergencia sirena = luces.AddComponent<LuzEmergencia>();
        sirena.luzA = azul;
        sirena.luzB = roja;
        sirena.intensidad = 18f;

        raiz.transform.position = pos;
        raiz.transform.rotation = Quaternion.Euler(0f, giroY, 0f);
        return raiz;
    }

    private static Light CrearLuz(Transform padre, Vector3 pos, Color color, float rango)
    {
        GameObject go = new GameObject("Luz");
        go.transform.SetParent(padre, false);
        go.transform.localPosition = pos;
        Light luz = go.AddComponent<Light>();
        luz.type = LightType.Point;
        luz.color = color;
        luz.range = rango;
        return luz;
    }

    /// <summary>Carpa con techo a dos aguas y cruz roja. Sin colisión en el centro para poder entrar.</summary>
    private static GameObject Carpa(Transform padre, Vector3 pos, float giroY, float ancho, float largo, Material lona, bool cruz)
    {
        GameObject raiz = new GameObject("Carpa");
        raiz.transform.SetParent(padre, false);
        Material poste = Mat("PosteMetal", new Color(0.32f, 0.33f, 0.35f), 0.4f, 0.6f);
        Material rojo = Mat("CruzRoja", new Color(0.78f, 0.1f, 0.1f), 0.3f);

        float alto = 2.7f;
        for (int sx = -1; sx <= 1; sx += 2)
        {
            for (int sz = -1; sz <= 1; sz += 2)
            {
                Primitiva(PrimitiveType.Cube, raiz.transform, "Poste", new Vector3(sx * ancho * 0.5f, alto * 0.5f, sz * largo * 0.5f), new Vector3(0.12f, alto, 0.12f), poste, null, true);
            }
        }

        float inclinacion = 22f;
        float media = ancho * 0.5f / Mathf.Cos(inclinacion * Mathf.Deg2Rad);
        float sube = Mathf.Tan(inclinacion * Mathf.Deg2Rad) * ancho * 0.25f;
        Primitiva(PrimitiveType.Cube, raiz.transform, "TechoI", new Vector3(-ancho * 0.25f, alto + sube, 0f), new Vector3(media + 0.2f, 0.06f, largo + 0.4f), lona, new Vector3(0f, 0f, inclinacion));
        Primitiva(PrimitiveType.Cube, raiz.transform, "TechoD", new Vector3(ancho * 0.25f, alto + sube, 0f), new Vector3(media + 0.2f, 0.06f, largo + 0.4f), lona, new Vector3(0f, 0f, -inclinacion));

        if (cruz)
        {
            float z = -largo * 0.5f - 0.1f;
            Primitiva(PrimitiveType.Cube, raiz.transform, "CruzV", new Vector3(0f, alto + 0.05f, z), new Vector3(0.28f, 0.9f, 0.04f), rojo);
            Primitiva(PrimitiveType.Cube, raiz.transform, "CruzH", new Vector3(0f, alto + 0.05f, z), new Vector3(0.9f, 0.28f, 0.04f), rojo);
        }

        raiz.transform.position = pos;
        raiz.transform.rotation = Quaternion.Euler(0f, giroY, 0f);
        return raiz;
    }

    /// <summary>Paciente tendido en una camilla, para usarse como decorado o como objetivo.</summary>
    private static GameObject Camilla(Transform padre, Vector3 pos, float giroY, Material piel = null)
    {
        GameObject raiz = new GameObject("Camilla");
        raiz.transform.SetParent(padre, false);

        Material lona = Mat("Lona", new Color(0.3f, 0.31f, 0.3f), 0.15f);
        Material metal = Mat("PosteMetal", new Color(0.32f, 0.33f, 0.35f), 0.4f, 0.6f);
        Primitiva(PrimitiveType.Cube, raiz.transform, "Lona", new Vector3(0f, 0.55f, 0f), new Vector3(0.75f, 0.08f, 2.1f), lona);
        for (int sx = -1; sx <= 1; sx += 2)
        {
            for (int sz = -1; sz <= 1; sz += 2)
            {
                Primitiva(PrimitiveType.Cube, raiz.transform, "Pata", new Vector3(sx * 0.3f, 0.26f, sz * 0.9f), new Vector3(0.06f, 0.52f, 0.06f), metal);
            }
        }

        raiz.transform.position = pos;
        raiz.transform.rotation = Quaternion.Euler(0f, giroY, 0f);
        Tendido(raiz.transform, new Vector3(0f, 0.72f, 0.1f), piel != null ? piel : PielVecino(3));
        return raiz;
    }

    private static TextMeshPro CrearTextoMundo(GameObject go, string texto, float tamano, Color color)
    {
        TextMeshPro tmp = go.AddComponent<TextMeshPro>();
        tmp.text = texto;
        tmp.fontSize = tamano;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.color = color;
        tmp.fontStyle = FontStyles.Bold;
        tmp.font = Fuente;
        tmp.rectTransform.sizeDelta = new Vector2(8f, 2f);
        return tmp;
    }

    /// <summary>Letrero flotante que siempre mira a la cámara.</summary>
    private static GameObject Letrero(Transform padre, string nombre, string texto, Vector3 pos, float tamano, Color color)
    {
        GameObject go = new GameObject(nombre);
        go.transform.SetParent(padre, false);
        go.transform.position = pos;
        CrearTextoMundo(go, texto, tamano, color);
        go.AddComponent<Billboard>();
        return go;
    }

    /// <summary>Ajusta niebla, cielo y luz para el ambiente de polvo de la zona de desastre.</summary>
    private static void AmbienteDesastre(Camera camara, Light sol, Color cielo, float densidadNiebla, float intensidadSol)
    {
        RenderSettings.fog = true;
        RenderSettings.fogMode = FogMode.ExponentialSquared;
        RenderSettings.fogColor = cielo;
        RenderSettings.fogDensity = densidadNiebla;
        RenderSettings.skybox = null;
        RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
        RenderSettings.ambientLight = Color.Lerp(cielo, new Color(0.62f, 0.64f, 0.68f), 0.65f);

        if (camara != null)
        {
            camara.clearFlags = CameraClearFlags.SolidColor;
            camara.backgroundColor = cielo;
            camara.farClipPlane = 400f;
        }

        if (sol != null)
        {
            sol.color = new Color(1f, 0.84f, 0.68f);
            sol.intensity = intensidadSol;
            sol.shadows = LightShadows.Soft;
            sol.transform.rotation = Quaternion.Euler(32f, -35f, 0f);
        }
    }

    /// <summary>Deja el jugador listo: modelo a escala, equipo, cámara y bonificaciones.</summary>
    private static PlayerController PrepararJugador(GameObject jugador)
    {
        // Limpia componentes de scripts que ya no existen en el proyecto.
        GameObjectUtility.RemoveMonoBehavioursWithMissingScript(jugador);

        MeshRenderer capsulaVisual = jugador.GetComponent<MeshRenderer>();
        if (capsulaVisual != null) capsulaVisual.enabled = false;

        foreach (string viejo in new[] { "Doctor", "Modelo" })
        {
            Transform hijo = jugador.transform.Find(viejo);
            if (hijo != null) Object.DestroyImmediate(hijo.gameObject);
        }

        GameObject modelo = CrearPersonaje(jugador.transform, "Modelo", PielRescatista, -1f);

        PlayerController controlador = jugador.GetComponent<PlayerController>();
        controlador.modeloVisualDoctor = modelo.transform;
        controlador.animador = modelo.GetComponent<Animator>();
        controlador.velocidadMovimiento = 4.2f;
        controlador.velocidadCorrer = 8f;
        controlador.fuerzaSalto = 7f;

        EquipamientoJugador equipo = jugador.GetComponent<EquipamientoJugador>();
        if (equipo == null) equipo = jugador.AddComponent<EquipamientoJugador>();
        equipo.catalogo = Cargar<CatalogoTienda>(DIR_TIENDA + "/CatalogoTienda.asset");

        if (jugador.GetComponent<AtaqueJugador>() == null) jugador.AddComponent<AtaqueJugador>();

        Camera camara = jugador.GetComponentInChildren<Camera>(true);
        if (camara == null) camara = Camera.main;
        camara.transform.SetParent(null, true);
        camara.gameObject.SetActive(true);
        camara.nearClipPlane = 0.15f;
        controlador.camaraJugador = camara;

        CameraController orbita = camara.GetComponent<CameraController>();
        if (orbita == null) orbita = camara.gameObject.AddComponent<CameraController>();
        orbita.objetivo = jugador.transform;
        orbita.offsetObjetivo = new Vector3(0f, 0.65f, 0f);
        orbita.distancia = 4.2f;
        orbita.capasColision = ~0;

        return controlador;
    }

    #endregion

    #region Interfaz

    private static TMP_FontAsset Fuente => Cargar<TMP_FontAsset>("Assets/TextMesh Pro/Resources/Fonts & Materials/LiberationSans SDF.asset");
    private static Sprite SpriteRedondeado => AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd");
    private static Sprite SpriteCirculo => AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Knob.psd");

    private static readonly Vector2 Centro = new Vector2(0.5f, 0.5f);

    private static Canvas PrepararCanvas()
    {
        Canvas canvas = Object.FindAnyObjectByType<Canvas>();
        for (int i = canvas.transform.childCount - 1; i >= 0; i--)
        {
            Object.DestroyImmediate(canvas.transform.GetChild(i).gameObject);
        }

        canvas.renderMode = RenderMode.ScreenSpaceOverlay;

        CanvasScaler escala = canvas.GetComponent<CanvasScaler>();
        if (escala == null) escala = canvas.gameObject.AddComponent<CanvasScaler>();
        escala.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        escala.referenceResolution = new Vector2(1920f, 1080f);
        escala.matchWidthOrHeight = 0.5f;

        if (canvas.GetComponent<GraphicRaycaster>() == null) canvas.gameObject.AddComponent<GraphicRaycaster>();
        return canvas;
    }

    private static RectTransform NuevoRect(Transform padre, string nombre, Vector2 ancla, Vector2 pos, Vector2 tam)
    {
        GameObject go = new GameObject(nombre, typeof(RectTransform));
        go.transform.SetParent(padre, false);
        RectTransform rt = (RectTransform)go.transform;
        rt.anchorMin = ancla;
        rt.anchorMax = ancla;
        rt.pivot = ancla;
        rt.anchoredPosition = pos;
        rt.sizeDelta = tam;
        return rt;
    }

    private static GameObject PanelUI(Transform padre, string nombre, Vector2 ancla, Vector2 pos, Vector2 tam, Color color, bool redondeado = true)
    {
        RectTransform rt = NuevoRect(padre, nombre, ancla, pos, tam);
        Image imagen = rt.gameObject.AddComponent<Image>();
        imagen.color = color;
        imagen.raycastTarget = true;
        if (redondeado)
        {
            imagen.sprite = SpriteRedondeado;
            imagen.type = Image.Type.Sliced;
            imagen.pixelsPerUnitMultiplier = 0.45f;
        }
        return rt.gameObject;
    }

    /// <summary>Panel que ocupa todo el padre, útil para oscurecer el fondo.</summary>
    private static GameObject Velo(Transform padre, string nombre, Color color)
    {
        GameObject go = new GameObject(nombre, typeof(RectTransform));
        go.transform.SetParent(padre, false);
        RectTransform rt = (RectTransform)go.transform;
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.sizeDelta = Vector2.zero;
        go.AddComponent<Image>().color = color;
        return go;
    }

    private static TextMeshProUGUI TextoUI(Transform padre, string nombre, string texto, float tamano, Vector2 ancla, Vector2 pos, Vector2 tam, TextAlignmentOptions alineacion, Color color, bool negrita = false)
    {
        RectTransform rt = NuevoRect(padre, nombre, ancla, pos, tam);
        TextMeshProUGUI tmp = rt.gameObject.AddComponent<TextMeshProUGUI>();
        tmp.font = Fuente;
        tmp.text = texto;
        tmp.fontSize = tamano;
        tmp.alignment = alineacion;
        tmp.color = color;
        tmp.raycastTarget = false;
        tmp.textWrappingMode = TextWrappingModes.Normal;
        if (negrita) tmp.fontStyle = FontStyles.Bold;
        return tmp;
    }

    private static Button BotonUI(Transform padre, string nombre, string texto, float tamano, Vector2 ancla, Vector2 pos, Vector2 tam, Color color, UnityAction accion)
    {
        GameObject go = PanelUI(padre, nombre, ancla, pos, tam, Color.white);
        Image imagen = go.GetComponent<Image>();

        Button boton = go.AddComponent<Button>();
        boton.targetGraphic = imagen;
        ColorBlock colores = boton.colors;
        colores.normalColor = color;
        colores.highlightedColor = Color.Lerp(color, Color.white, 0.18f);
        colores.selectedColor = color;
        colores.pressedColor = Color.Lerp(color, Color.black, 0.2f);
        colores.disabledColor = Color.Lerp(color, Color.black, 0.4f);
        boton.colors = colores;

        go.AddComponent<BotonMenuFx>();

        TextMeshProUGUI etiqueta = TextoUI(go.transform, "Texto", texto, tamano, Centro, Vector2.zero, tam, TextAlignmentOptions.Center, Color.white, true);
        etiqueta.characterSpacing = 3f;

        // AddPersistentListener queda guardado en la escena; onClick.AddListener no.
        if (accion != null) UnityEventTools.AddPersistentListener(boton.onClick, accion);
        return boton;
    }

    private static GameObject ChipMonedas(Transform padre, Vector2 ancla, Vector2 pos)
    {
        GameObject chip = PanelUI(padre, "ChipMonedas", ancla, pos, new Vector2(170f, 58f), new Color(0.06f, 0.08f, 0.11f, 0.88f));

        RectTransform moneda = NuevoRect(chip.transform, "Icono", new Vector2(0f, 0.5f), new Vector2(14f, 0f), new Vector2(34f, 34f));
        Image imagen = moneda.gameObject.AddComponent<Image>();
        imagen.sprite = SpriteCirculo;
        imagen.color = ColOro;

        TextMeshProUGUI cantidad = TextoUI(chip.transform, "Cantidad", "0", 30f, new Vector2(1f, 0.5f), new Vector2(-18f, 0f), new Vector2(105f, 44f), TextAlignmentOptions.Right, ColOro, true);
        cantidad.gameObject.AddComponent<TextoMonedas>();
        return chip;
    }

    private static GameObject PanelPausa(Transform canvas, PlayerController jugador, string textoSalida, UnityAction salida)
    {
        GameObject velo = Velo(canvas, "PanelPausa", new Color(0.02f, 0.03f, 0.05f, 0.72f));
        GameObject tarjeta = PanelUI(velo.transform, "Tarjeta", Centro, Vector2.zero, new Vector2(520f, 420f), ColFondo);

        TextoUI(tarjeta.transform, "Titulo", "PAUSA", 46f, new Vector2(0.5f, 1f), new Vector2(0f, -36f), new Vector2(440f, 60f), TextAlignmentOptions.Center, Color.white, true);
        BotonUI(tarjeta.transform, "BtnReanudar", "CONTINUAR", 24f, Centro, new Vector2(0f, 40f), new Vector2(400f, 66f), ColNaranja, jugador.ReanudarJuego);
        BotonUI(tarjeta.transform, "BtnSalida", textoSalida, 22f, Centro, new Vector2(0f, -40f), new Vector2(400f, 62f), ColGris, salida);
        BotonUI(tarjeta.transform, "BtnSalir", "SALIR DEL JUEGO", 22f, Centro, new Vector2(0f, -116f), new Vector2(400f, 62f), ColRojo, jugador.SalirJuego);

        jugador.panelPausa = velo;
        velo.SetActive(false);
        return velo;
    }

    #endregion
}
