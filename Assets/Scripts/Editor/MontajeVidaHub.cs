using System.Collections.Generic;
using Unity.AI.Navigation;
using UnityEditor;
using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// Da vida al Hub: cielo, césped, paseos, parques, calle con tráfico, fuente, pájaros,
/// puesto de mando y residentes. Los recursos externos están en Assets/Externos (CC0).
/// </summary>
public static partial class MontajeRescate
{
    private const string KENNEY = "Assets/Externos/Kenney/";
    private const string POLY = "Assets/Externos/PolyHaven/";
    private const float CALLE = 41.5f;

    private static readonly string[] Nombres =
    {
        "Ana", "Luis", "Marta", "Diego", "Rosa", "Pablo", "Elena", "Hugo", "Sara", "Tomás", "Inés", "Raúl", "Lucía", "Mario"
    };

    private static readonly string[] Consejos =
    {
        "Lávate las manos con agua y jabón antes de comer.",
        "En un sismo: agáchate, cúbrete y sujétate.",
        "Ten lista una mochila de emergencia en casa.",
        "En una emergencia, bebe solo agua segura.",
        "Si una herida sangra, presiona con un paño limpio.",
        "Aprende el número de emergencias de tu país.",
        "Después de un sismo, aléjate de muros dañados.",
        "Dormir bien también cuida tu salud."
    };

    private static void ConstruirVidaHub(GameObject jugador)
    {
        Transform raiz = new GameObject("VidaHub").transform;
        Transform suelos = Grupo(raiz, "Suelos");
        Transform plantas = Grupo(raiz, "Plantas");
        Transform calle = Grupo(raiz, "Calle");
        Transform personas = Grupo(raiz, "Residentes");

        ConstruirSuelosHub(suelos);
        ConstruirParque(plantas, -1f, 11);
        ConstruirParque(plantas, 1f, 29);
        ConstruirFuente(raiz);
        ConstruirEntornoCalle(calle);
        Transform mando = ConstruirPuestoMando(raiz);

        raiz.gameObject.AddComponent<AmbienteHub>();

        // Se hornea con todo lo sólido ya colocado; los vehículos y personas se agregan después.
        HornearNavMeshHub(raiz, jugador);

        ConstruirTrafico(calle);
        ConstruirResidentes(personas);
        ConstruirEquipoMando(mando);
        ConstruirPajaros(raiz);
    }

    private static void ConstruirSuelosHub(Transform raiz)
    {
        Material asfalto = Mat("AsfaltoHub", new Color(0.2f, 0.2f, 0.22f), 0.15f);
        Material linea = Mat("LineaCalle", new Color(0.92f, 0.9f, 0.8f), 0.2f);

        // Paseos de adoquín, con repetición continua aunque se crucen.
        Mesh paseos = MallaPaseos();
        GameObject suelo = new GameObject("Paseos");
        suelo.transform.SetParent(raiz, false);
        suelo.AddComponent<MeshFilter>().sharedMesh = paseos;
        MeshRenderer dibujo = suelo.AddComponent<MeshRenderer>();
        dibujo.sharedMaterial = MatTextura("PaseoHub", "herringbone_pavement_03", Color.white, 1f);
        dibujo.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;

        // Calle que rodea todo el Hub.
        SueloPlano(raiz, "CalleSur", new Vector3(0f, 0.03f, -CALLE), 90f, 7f, asfalto);
        SueloPlano(raiz, "CalleNorte", new Vector3(0f, 0.03f, CALLE), 90f, 7f, asfalto);
        SueloPlano(raiz, "CalleOeste", new Vector3(-CALLE, 0.03f, 0f), 7f, 90f, asfalto);
        SueloPlano(raiz, "CalleEste", new Vector3(CALLE, 0.03f, 0f), 7f, 90f, asfalto);

        for (float d = -36f; d <= 36f; d += 6f)
        {
            SueloPlano(raiz, "Linea", new Vector3(d, 0.04f, -CALLE), 3f, 0.25f, linea);
            SueloPlano(raiz, "Linea", new Vector3(d, 0.04f, CALLE), 3f, 0.25f, linea);
            SueloPlano(raiz, "Linea", new Vector3(-CALLE, 0.04f, d), 0.25f, 3f, linea);
            SueloPlano(raiz, "Linea", new Vector3(CALLE, 0.04f, d), 0.25f, 3f, linea);
        }
    }

    /// <summary>Crea un material con textura y relieve de Poly Haven.</summary>
    private static Material MatTextura(string nombre, string textura, Color tinte, float repetir)
    {
        string relieve = POLY + textura + "_nor_gl_1k.jpg";
        TextureImporter importador = (TextureImporter)AssetImporter.GetAtPath(relieve);
        if (importador.textureType != TextureImporterType.NormalMap)
        {
            importador.textureType = TextureImporterType.NormalMap;
            importador.SaveAndReimport();
        }

        Material material = Mat(nombre, tinte, 0.12f);
        material.SetTexture("_BaseMap", Cargar<Texture2D>(POLY + textura + "_diff_1k.jpg"));
        material.SetTexture("_BumpMap", Cargar<Texture2D>(relieve));
        material.SetTextureScale("_BaseMap", Vector2.one * repetir);
        material.EnableKeyword("_NORMALMAP");
        EditorUtility.SetDirty(material);
        return material;
    }

    private static Mesh MallaPaseos()
    {
        List<Vector3> vertices = new List<Vector3>();
        List<int> triangulos = new List<int>();

        void Rectangulo(float x0, float z0, float x1, float z1)
        {
            int i = vertices.Count;
            vertices.Add(new Vector3(x0, 0.02f, z0));
            vertices.Add(new Vector3(x0, 0.02f, z1));
            vertices.Add(new Vector3(x1, 0.02f, z1));
            vertices.Add(new Vector3(x1, 0.02f, z0));
            triangulos.AddRange(new[] { i, i + 1, i + 2, i, i + 2, i + 3 });
        }

        Rectangulo(-3.5f, -14f, 3.5f, 0f);    // del Hub a la plaza
        Rectangulo(-20f, -3f, 16f, 14f);      // explanada de la tienda y el puesto de mando
        Rectangulo(-24f, -24f, 24f, -20f);    // cruce hacia los parques

        const int LADOS = 64;
        const float RADIO = 13f;
        int centro = vertices.Count;
        vertices.Add(new Vector3(0f, 0.021f, -22f));
        for (int i = 0; i < LADOS; i++)
        {
            float angulo = i * Mathf.PI * 2f / LADOS;
            vertices.Add(new Vector3(Mathf.Cos(angulo) * RADIO, 0.021f, -22f + Mathf.Sin(angulo) * RADIO));
            triangulos.AddRange(new[] { centro, centro + 1 + (i + 1) % LADOS, centro + 1 + i });
        }

        Vector2[] uv = new Vector2[vertices.Count];
        for (int i = 0; i < uv.Length; i++) uv[i] = new Vector2(vertices[i].x, vertices[i].z) / 3.5f;

        string ruta = DIR + "/MallaPaseos.asset";
        AssetDatabase.DeleteAsset(ruta);
        Mesh malla = new Mesh { name = "Paseos" };
        malla.SetVertices(vertices);
        malla.SetTriangles(triangulos, 0);
        malla.uv = uv;
        malla.RecalculateNormals();
        malla.RecalculateTangents();
        malla.RecalculateBounds();
        AssetDatabase.CreateAsset(malla, ruta);
        return malla;
    }

    private static float EscalaPara(string ruta, float alto)
    {
        GameObject prueba = Object.Instantiate(Cargar<GameObject>(ruta));
        float escala = alto / MedirCaja(prueba).size.y;
        Object.DestroyImmediate(prueba);
        return escala;
    }

    /// <summary>Carpa de coordinación junto al médico de misiones, con mesa, mapa y suministros.</summary>
    private static Transform ConstruirPuestoMando(Transform raiz)
    {
        Transform puesto = Grupo(raiz, "PuestoMando");
        puesto.position = new Vector3(10.5f, 0f, 4.5f);
        puesto.rotation = Quaternion.Euler(0f, -25f, 0f);

        Vector3 Mundo(float x, float y, float z) => puesto.TransformPoint(new Vector3(x, y, z));
        float giro = puesto.eulerAngles.y;

        Material lona = Mat("LonaAzul", new Color(0.16f, 0.35f, 0.6f), 0.2f);
        Material madera = Mat("Madera", new Color(0.42f, 0.28f, 0.16f), 0.2f);
        Material tablero = Mat("TableroMapa", Color.white, 0.2f);
        Texture2D mapa = AssetDatabase.LoadAssetAtPath<Texture2D>(DIR + "/MapaNivel1.png");
        if (mapa != null) tablero.SetTexture("_BaseMap", mapa);

        GameObject carpa = Carpa(puesto, Vector3.zero, 0f, 7.5f, 5.5f, lona, true);
        carpa.transform.localPosition = Vector3.zero;
        carpa.transform.localRotation = Quaternion.identity;

        // Mesa de operaciones con el mapa de la zona.
        Primitiva(PrimitiveType.Cube, puesto, "Mesa", new Vector3(0f, 0.78f, 0.5f), new Vector3(3f, 0.08f, 1.3f), madera, null, true);
        foreach (Vector3 pata in new[] { new Vector3(-1.4f, 0.37f, -0.05f), new Vector3(1.4f, 0.37f, -0.05f), new Vector3(-1.4f, 0.37f, 1.05f), new Vector3(1.4f, 0.37f, 1.05f) })
        {
            Primitiva(PrimitiveType.Cube, puesto, "Pata", pata, new Vector3(0.08f, 0.74f, 0.08f), madera, null, true);
        }
        Primitiva(PrimitiveType.Cube, puesto, "MapaMesa", new Vector3(-0.3f, 0.825f, 0.5f), new Vector3(0.7f, 0.01f, 1.15f), tablero);

        // Tablero al fondo.
        Primitiva(PrimitiveType.Cube, puesto, "Tablero", new Vector3(0f, 1.45f, 2.5f), new Vector3(1.05f, 2f, 0.06f), tablero, null, true);
        Primitiva(PrimitiveType.Cube, puesto, "MarcoTablero", new Vector3(0f, 1.45f, 2.54f), new Vector3(1.2f, 2.15f, 0.04f), madera);

        Poner(OBJETOS + "HealthKit.prefab", puesto, Mundo(1f, 0.83f, 0.5f), giro + 20f, 0.9f, false);
        Poner(OBJETOS + "TV.prefab", puesto, Mundo(-2.6f, 0.82f, 1.8f), giro + 160f, EscalaPara(OBJETOS + "TV.prefab", 0.55f), false);
        Primitiva(PrimitiveType.Cube, puesto, "MesaRadio", new Vector3(-2.6f, 0.4f, 1.8f), new Vector3(1f, 0.8f, 0.7f), madera, null, true);

        Caja(puesto, Mundo(2.9f, 0f, 1.9f), giro + 10f, 1.5f);
        Caja(puesto, Mundo(2.9f, 0.72f, 1.9f), giro - 12f, 1.3f);
        Caja(puesto, Mundo(3.1f, 0f, 0.5f), giro + 80f, 1.4f);

        // Herido atendido fuera de la carpa.
        GameObject camilla = Camilla(puesto, Mundo(-5.2f, 0f, 0.5f), giro + 90f);
        BoxCollider solido = camilla.AddComponent<BoxCollider>();
        solido.center = Vector3.up * 0.45f;
        solido.size = new Vector3(0.9f, 0.9f, 2.2f);

        Letrero(puesto, "LetreroMando", "PUESTO DE MANDO", Mundo(0f, 4.6f, -2.8f), 3.6f, Color.white);
        return puesto;
    }

    private static void ConstruirEquipoMando(Transform puesto)
    {
        string[] frasesEquipo =
        {
            "Revisamos la zona afectada. Habla con el médico jefe para salir.",
            "Si ves a un herido, primero asegúrate de que la zona sea segura.",
            "El triage ayuda a atender primero a quien está más grave.",
            "No muevas a un herido si no es necesario."
        };

        Vector3 Mundo(float x, float z) => puesto.TransformPoint(new Vector3(x, 0f, z));
        float giro = puesto.eulerAngles.y;

        NPCResidente Equipo(string id, string nombre, Material piel, Vector3 pos, float mirar, bool atendiendo = false)
        {
            NPCResidente persona = CrearResidente(puesto, id, nombre, piel, pos, giro + mirar, null, false);
            persona.trabajando = atendiendo;
            persona.frases = frasesEquipo;
            return persona;
        }

        Equipo("Paramedico1", "Paramédico Javier", PielParamedico, Mundo(-0.6f, -0.45f), 0f);
        Equipo("Paramedico2", "Paramédica Noelia", PielParamedico, Mundo(0.9f, 1.6f), 180f);
        Equipo("Operador", "Operadora Vera", PielEnfermera, Mundo(-2.6f, 1f), 0f);
        Equipo("Enfermera", "Enfermero Iván", PielEnfermera, Mundo(-5.2f, -0.8f), 0f, true);
    }

    private static void SueloPlano(Transform padre, string nombre, Vector3 pos, float ancho, float largo, Material material)
    {
        // Un plano de Unity mide 10 x 10.
        GameObject plano = Primitiva(PrimitiveType.Plane, padre, nombre, pos, new Vector3(ancho / 10f, 1f, largo / 10f), material);
        plano.GetComponent<Renderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
    }

    /// <summary>Coloca un modelo de la naturaleza a la altura indicada. Los árboles llevan colisión en el tronco.</summary>
    private static GameObject Vegetal(string nombre, Transform padre, Vector3 pos, float giroY, float altura, bool tronco = false)
    {
        GameObject go = (GameObject)PrefabUtility.InstantiatePrefab(Cargar<GameObject>(KENNEY + "Naturaleza/" + nombre + ".fbx"), padre);
        go.transform.position = Vector3.zero;
        go.transform.rotation = Quaternion.identity;

        PintarVegetal(go);

        Bounds medida = MedirCaja(go);
        float escala = altura / medida.size.y;
        go.transform.localScale = Vector3.one * escala;
        go.transform.position = pos + Vector3.up * (-medida.min.y * escala);
        go.transform.rotation = Quaternion.Euler(0f, giroY, 0f);

        if (tronco)
        {
            CapsuleCollider colision = go.AddComponent<CapsuleCollider>();
            colision.radius = 0.4f / escala;
            colision.height = 3f / escala;
            colision.center = Vector3.up * (1.5f / escala);
        }
        return go;
    }

    /// <summary>Los colores de estos FBX se importan azulados, así que se cambian por materiales propios.</summary>
    private static void PintarVegetal(GameObject go)
    {
        foreach (Renderer parte in go.GetComponentsInChildren<Renderer>())
        {
            Material[] materiales = parte.sharedMaterials;
            for (int i = 0; i < materiales.Length; i++)
            {
                materiales[i] = MaterialVegetal(materiales[i] != null ? materiales[i].name : string.Empty, materiales[i]);
            }
            parte.sharedMaterials = materiales;
        }
    }

    private static Material MaterialVegetal(string nombre, Material original)
    {
        if (nombre.StartsWith("leafsDark")) return Mat("HojaOscura", new Color(0.13f, 0.36f, 0.2f), 0.1f);
        if (nombre.StartsWith("leafs")) return Mat("Hoja", new Color(0.3f, 0.56f, 0.2f), 0.1f);
        if (nombre.StartsWith("grass")) return Mat("Pasto", new Color(0.34f, 0.6f, 0.24f), 0.1f);
        if (nombre.StartsWith("woodBarkDark")) return Mat("CortezaOscura", new Color(0.26f, 0.18f, 0.13f), 0.1f);
        if (nombre.StartsWith("wood")) return Mat("Corteza", new Color(0.42f, 0.28f, 0.17f), 0.1f);
        if (nombre.StartsWith("colorRed")) return Mat("FlorRoja", new Color(0.85f, 0.15f, 0.15f), 0.2f);
        if (nombre.StartsWith("colorYellow")) return Mat("FlorAmarilla", new Color(0.95f, 0.8f, 0.15f), 0.2f);
        if (nombre.StartsWith("colorPurple")) return Mat("FlorMorada", new Color(0.6f, 0.25f, 0.8f), 0.2f);
        return original;
    }

    private static void ConstruirParque(Transform raiz, float lado, int semilla)
    {
        System.Random azar = new System.Random(semilla);
        float Rango(float a, float b) => a + (float)azar.NextDouble() * (b - a);

        string[] arboles = { "tree_default", "tree_oak", "tree_detailed", "tree_fat", "tree_cone", "tree_pineRoundA" };
        string[] flores = { "flower_redA", "flower_yellowA", "flower_purpleA" };
        string[] arbustos = { "plant_bush", "plant_bushLarge", "plant_bushDetailed" };

        List<Vector3> ocupados = new List<Vector3>();
        int colocados = 0;
        for (int intento = 0; intento < 200 && colocados < 15; intento++)
        {
            Vector3 pos = new Vector3(lado * Rango(20f, 35f), 0f, Rango(-32f, 32f));
            if (ocupados.Exists(o => Vector3.Distance(o, pos) < 6.5f)) continue;

            ocupados.Add(pos);
            colocados++;
            Vegetal(arboles[azar.Next(arboles.Length)], raiz, pos, Rango(0f, 360f), Rango(5.5f, 8.5f), true);
        }

        for (int i = 0; i < 18; i++)
        {
            Vector3 pos = new Vector3(lado * Rango(19f, 35f), 0f, Rango(-33f, 33f));
            Vegetal(arbustos[azar.Next(arbustos.Length)], raiz, pos, Rango(0f, 360f), Rango(0.7f, 1.2f));
        }

        // Macizos de flores y pasto.
        for (int i = 0; i < 9; i++)
        {
            Vector3 centro = new Vector3(lado * Rango(19f, 35f), 0f, Rango(-32f, 32f));
            string flor = flores[azar.Next(flores.Length)];
            for (int j = 0; j < 7; j++)
            {
                Vector3 pos = centro + new Vector3(Rango(-1.3f, 1.3f), 0f, Rango(-1.3f, 1.3f));
                Vegetal(flor, raiz, pos, Rango(0f, 360f), Rango(0.35f, 0.55f));
            }
        }

        for (int i = 0; i < 40; i++)
        {
            Vector3 pos = new Vector3(lado * Rango(18.5f, 35.5f), 0f, Rango(-33.5f, 33.5f));
            Vegetal(i % 2 == 0 ? "grass" : "grass_large", raiz, pos, Rango(0f, 360f), Rango(0.3f, 0.5f));
        }

        // Bancas mirando al paseo central.
        for (float z = -28f; z <= 28f; z += 14f)
        {
            Poner(CIUDAD + "bench.prefab", raiz, new Vector3(lado * 18.6f, 0f, z), lado > 0f ? -90f : 90f, 0.8f);
        }
    }

    private static void ConstruirFuente(Transform raiz)
    {
        Transform fuente = Grupo(raiz, "Fuente");
        fuente.position = new Vector3(0f, 0f, -22f);

        Material piedra = Mat("PiedraClara", new Color(0.74f, 0.72f, 0.68f), 0.2f);
        Material agua = Mat("Agua", new Color(0.25f, 0.55f, 0.75f), 0.9f);

        Primitiva(PrimitiveType.Cylinder, fuente, "Borde", new Vector3(0f, 0.3f, 0f), new Vector3(7f, 0.3f, 7f), piedra, null, true);
        Primitiva(PrimitiveType.Cylinder, fuente, "Agua", new Vector3(0f, 0.58f, 0f), new Vector3(6.2f, 0.03f, 6.2f), agua);
        Primitiva(PrimitiveType.Cylinder, fuente, "Columna", new Vector3(0f, 1.1f, 0f), new Vector3(0.7f, 0.6f, 0.7f), piedra);
        Primitiva(PrimitiveType.Cylinder, fuente, "Taza", new Vector3(0f, 1.75f, 0f), new Vector3(2f, 0.1f, 2f), piedra);

        GameObject chorro = new GameObject("Chorro");
        chorro.transform.SetParent(fuente, false);
        chorro.transform.localPosition = Vector3.up * 1.9f;

        ParticleSystem sistema = chorro.AddComponent<ParticleSystem>();
        ParticleSystem.MainModule principal = sistema.main;
        principal.startLifetime = 1.3f;
        principal.startSpeed = new ParticleSystem.MinMaxCurve(3.5f, 4.5f);
        principal.startSize = new ParticleSystem.MinMaxCurve(0.1f, 0.22f);
        principal.startColor = new Color(0.8f, 0.92f, 1f, 0.75f);
        principal.gravityModifier = 0.8f;
        principal.maxParticles = 200;

        ParticleSystem.EmissionModule emision = sistema.emission;
        emision.rateOverTime = 90f;

        ParticleSystem.ShapeModule forma = sistema.shape;
        forma.shapeType = ParticleSystemShapeType.Cone;
        forma.angle = 16f;
        forma.radius = 0.08f;
        forma.rotation = new Vector3(-90f, 0f, 0f);

        chorro.GetComponent<ParticleSystemRenderer>().sharedMaterial = MatParticula("ParticulaAgua", false);
    }

    private static void ConstruirEntornoCalle(Transform raiz)
    {
        // Setos sobre los límites para no ver un muro liso.
        for (float d = -47f; d <= 47f; d += 6f)
        {
            Poner(CIUDAD + "hedge_with_base.prefab", raiz, new Vector3(d, 0f, 49f), 0f, 1f, false);
            Poner(CIUDAD + "hedge_with_base.prefab", raiz, new Vector3(d, 0f, -49f), 0f, 1f, false);
            Poner(CIUDAD + "hedge_with_base.prefab", raiz, new Vector3(49f, 0f, d), 90f, 1f, false);
            Poner(CIUDAD + "hedge_with_base.prefab", raiz, new Vector3(-49f, 0f, d), 90f, 1f, false);
        }

        // Barrio alrededor, para que el Hub no parezca una isla.
        Poner(CIUDAD + "office_building_2.prefab", raiz, new Vector3(-30f, 0f, 76f), 180f, 1f, false);
        Poner(CIUDAD + "office_building_3.prefab", raiz, new Vector3(28f, 0f, 80f), 180f, 1f, false);
        Poner(CIUDAD + "office_building_1.prefab", raiz, new Vector3(82f, 0f, 0f), -90f, 1f, false);
        Poner(CIUDAD + "office_building_4.prefab", raiz, new Vector3(-78f, 0f, -6f), 90f, 1f, false);
        Poner(CIUDAD + "office_building_2.prefab", raiz, new Vector3(-20f, 0f, -78f), 0f, 1f, false);
        Poner(CIUDAD + "office_building_3.prefab", raiz, new Vector3(36f, 0f, -80f), 0f, 1f, false);

        for (float d = -36f; d <= 36f; d += 18f)
        {
            Poner(CIUDAD + "lamp_pole_small.prefab", raiz, new Vector3(d, 0f, 37.4f), 0f, 1f, false);
            Poner(CIUDAD + "lamp_pole_small.prefab", raiz, new Vector3(d, 0f, -37.4f), 0f, 1f, false);
        }

        // Estacionamiento detrás del hospital.
        Vehiculo("police", raiz, new Vector3(-12f, 0f, 30f), 0f);
        Vehiculo("firetruck", raiz, new Vector3(-7f, 0f, 30f), 0f);
        Vehiculo("van", raiz, new Vector3(-2f, 0f, 30f), 0f);
        Vehiculo("suv", raiz, new Vector3(3f, 0f, 30f), 180f);

        // Camión de reparto junto a la tienda.
        Vehiculo("delivery", raiz, new Vector3(-15.5f, 0f, 7f), 8f);

        foreach (Vector3 pos in new[] { new Vector3(6.5f, 0f, 28f), new Vector3(7.5f, 0f, 30f), new Vector3(6.5f, 0f, 32f) })
        {
            Poner(KENNEY + "Vehiculos/cone.fbx", raiz, pos, 0f, 1f, false);
        }
    }

    private static GameObject Vehiculo(string nombre, Transform padre, Vector3 pos, float giroY)
    {
        return Poner(KENNEY + "Vehiculos/" + nombre + ".fbx", padre, pos, giroY, 1.4f);
    }

    private static void HornearNavMeshHub(Transform raiz, GameObject jugador)
    {
        NavMeshSurface superficie = raiz.gameObject.AddComponent<NavMeshSurface>();
        superficie.collectObjects = CollectObjects.Volume;
        superficie.center = new Vector3(0f, 2f, 0f);
        superficie.size = new Vector3(76f, 8f, 76f);
        superficie.useGeometry = NavMeshCollectGeometry.PhysicsColliders;

        jugador.SetActive(false);
        superficie.BuildNavMesh();
        jugador.SetActive(true);

        string ruta = DIR + "/NavMeshHub.asset";
        AssetDatabase.DeleteAsset(ruta);
        AssetDatabase.CreateAsset(superficie.navMeshData, ruta);
        EditorUtility.SetDirty(superficie);
    }

    private static void ConstruirTrafico(Transform raiz)
    {
        Transform ruta = Grupo(raiz, "RutaVehiculos");
        float a = CALLE + 1.7f;
        float b = 36f;

        // Sentido antihorario por el carril exterior, con las esquinas recortadas.
        Vector3[] esquinas =
        {
            new Vector3(-b, 0f, -a), new Vector3(b, 0f, -a), new Vector3(a, 0f, -b), new Vector3(a, 0f, b),
            new Vector3(b, 0f, a), new Vector3(-b, 0f, a), new Vector3(-a, 0f, b), new Vector3(-a, 0f, -b)
        };

        Transform[] puntos = new Transform[esquinas.Length];
        for (int i = 0; i < esquinas.Length; i++)
        {
            GameObject punto = new GameObject("Punto" + (i + 1));
            punto.transform.SetParent(ruta, false);
            punto.transform.position = esquinas[i];
            puntos[i] = punto.transform;
        }

        VehiculoEnRuta("ambulance", raiz, puntos, 0, 8.5f, true);
        VehiculoEnRuta("taxi", raiz, puntos, 3, 8f, false);
        VehiculoEnRuta("sedan", raiz, puntos, 5, 7.5f, false);
    }

    private static void VehiculoEnRuta(string nombre, Transform raiz, Transform[] puntos, int inicio, float velocidad, bool sirena)
    {
        Transform anterior = puntos[(inicio + puntos.Length - 1) % puntos.Length];
        Vector3 direccion = puntos[inicio].position - anterior.position;
        GameObject coche = Vehiculo(nombre, raiz, anterior.position, Quaternion.LookRotation(direccion).eulerAngles.y);

        Rigidbody cuerpo = coche.AddComponent<Rigidbody>();
        cuerpo.isKinematic = true;

        VehiculoRuta ruta = coche.AddComponent<VehiculoRuta>();
        ruta.puntos = puntos;
        ruta.indice = inicio;
        ruta.velocidad = velocidad;

        if (!sirena) return;

        GameObject luces = new GameObject("Sirena");
        luces.transform.SetParent(coche.transform, false);
        luces.transform.localPosition = Vector3.up * 2f;
        LuzEmergencia emergencia = luces.AddComponent<LuzEmergencia>();
        emergencia.luzA = CrearLuz(luces.transform, new Vector3(-0.5f, 0f, 0f), new Color(0.15f, 0.35f, 1f), 9f);
        emergencia.luzB = CrearLuz(luces.transform, new Vector3(0.5f, 0f, 0f), new Color(1f, 0.1f, 0.08f), 9f);
        emergencia.intensidad = 8f;
    }

    private static void ConstruirResidentes(Transform raiz)
    {
        Transform rutas = Grupo(raiz, "PuntosPaseo");
        Vector3[] lugares =
        {
            new Vector3(-9f, 0f, -22f), new Vector3(9f, 0f, -22f), new Vector3(0f, 0f, -11.5f), new Vector3(0f, 0f, -32.5f),
            new Vector3(-16f, 0f, -14f), new Vector3(16f, 0f, -14f), new Vector3(-16f, 0f, 12f), new Vector3(16f, 0f, 2f),
            new Vector3(-23f, 0f, 24f), new Vector3(23f, 0f, -26f), new Vector3(-24f, 0f, -24f), new Vector3(24f, 0f, 22f),
            new Vector3(-4f, 0f, 0f), new Vector3(14f, 0f, 26f), new Vector3(-14f, 0f, 26f), new Vector3(0f, 0f, -3f)
        };

        Transform[] puntos = new Transform[lugares.Length];
        for (int i = 0; i < lugares.Length; i++)
        {
            GameObject punto = new GameObject("Paseo" + (i + 1));
            punto.transform.SetParent(rutas, false);
            punto.transform.position = lugares[i];
            puntos[i] = punto.transform;
        }

        for (int i = 0; i < 9; i++)
        {
            Vector3 inicio = lugares[(i * 3 + 1) % lugares.Length];
            CrearResidente(raiz, "Residente" + (i + 1), Nombres[i], PielVecino(i), inicio, i * 40f, puntos, true);
        }

        // Grupos que charlan quietos.
        CrearResidente(raiz, "Charla1", Nombres[9], PielVecino(1), new Vector3(-7f, 0f, -12f), 90f, null, false);
        CrearResidente(raiz, "Charla2", Nombres[10], PielVecino(2), new Vector3(-5.6f, 0f, -12f), -90f, null, false);
        CrearResidente(raiz, "Charla3", Nombres[11], PielVecino(4), new Vector3(13.5f, 0f, -6f), 200f, null, false);
        CrearResidente(raiz, "Charla4", Nombres[12], PielVecino(5), new Vector3(13f, 0f, -7.6f), 20f, null, false);
        CrearResidente(raiz, "Paseante", Nombres[13], PielVecino(0), new Vector3(-17.3f, 0f, 3f), 90f, null, false);
    }

    private static NPCResidente CrearResidente(Transform padre, string id, string nombre, Material piel, Vector3 pos, float giroY, Transform[] puntos, bool pasea)
    {
        GameObject raiz = new GameObject(id);
        raiz.transform.SetParent(padre, false);
        raiz.transform.position = pos;
        raiz.transform.rotation = Quaternion.Euler(0f, giroY, 0f);

        NavMeshAgent agente = raiz.AddComponent<NavMeshAgent>();
        agente.radius = 0.35f;
        agente.height = 1.8f;
        agente.speed = 2.2f;
        agente.angularSpeed = 240f;
        agente.acceleration = 6f;
        agente.stoppingDistance = 0.2f;
        agente.avoidancePriority = 40 + Mathf.Abs(id.GetHashCode()) % 40;

        CapsuleCollider colision = raiz.AddComponent<CapsuleCollider>();
        colision.center = Vector3.up * 0.9f;
        colision.height = 1.8f;
        colision.radius = 0.32f;

        GameObject modelo = CrearPersonaje(raiz.transform, "Modelo", piel, 0f);

        GameObject marca = new GameObject("MarcaCharla");
        marca.transform.SetParent(raiz.transform, false);
        marca.transform.localPosition = Vector3.up * 2.2f;
        TMPro.TextMeshPro puntos3 = CrearTextoMundo(marca, "...", 5f, ColOro);
        marca.AddComponent<Billboard>();

        NPCResidente residente = raiz.AddComponent<NPCResidente>();
        residente.nombre = nombre;
        residente.puntos = puntos;
        residente.pasea = pasea;
        residente.animador = modelo.GetComponent<Animator>();
        residente.marcaCharla = puntos3;
        residente.frases = Consejos;
        return residente;
    }

    private static void ConstruirPajaros(Transform raiz)
    {
        Transform grupo = Grupo(raiz, "Pajaros");
        Material gris = Mat("Pajaro", new Color(0.2f, 0.21f, 0.24f), 0.1f);

        for (int i = 0; i < 7; i++)
        {
            GameObject pajaro = new GameObject("Pajaro" + (i + 1));
            pajaro.transform.SetParent(grupo, false);
            pajaro.transform.position = new Vector3(0f, 14f, 0f);

            Primitiva(PrimitiveType.Sphere, pajaro.transform, "Cuerpo", Vector3.zero, new Vector3(0.12f, 0.1f, 0.34f), gris);

            GameObject alaI = new GameObject("AlaI");
            alaI.transform.SetParent(pajaro.transform, false);
            Primitiva(PrimitiveType.Cube, alaI.transform, "Pluma", new Vector3(-0.22f, 0f, 0f), new Vector3(0.44f, 0.015f, 0.16f), gris);

            GameObject alaD = new GameObject("AlaD");
            alaD.transform.SetParent(pajaro.transform, false);
            Primitiva(PrimitiveType.Cube, alaD.transform, "Pluma", new Vector3(0.22f, 0f, 0f), new Vector3(0.44f, 0.015f, 0.16f), gris);

            PajaroVuelo vuelo = pajaro.AddComponent<PajaroVuelo>();
            vuelo.centro = new Vector3(i % 2 == 0 ? -14f : 14f, 13f + i * 0.8f, -6f + i * 3f);
            vuelo.radio = 11f + i * 2.2f;
            vuelo.velocidadAngular = (i % 2 == 0 ? 1f : -1f) * (24f + i * 2f);
            vuelo.alaIzquierda = alaI.transform;
            vuelo.alaDerecha = alaD.transform;
        }
    }
}
