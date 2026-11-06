using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

/// <summary>
/// Nivel 3, base lunar Selene: una lluvia de meteoritos dañó los módulos.
/// Hay gravedad baja, androides averiados y fugas de plasma. Se ve la Tierra en el cielo.
/// </summary>
public static partial class MontajeRescate
{
    private const string ESPACIO = KENNEY + "Espacio/";

    private static NivelDatos DatosNivel3()
    {
        return new NivelDatos
        {
            numero = 3,
            escena = "Nivel3",
            medio = 56f,
            inicio = new Vector3(0f, 1.05f, -47f),
            cielo = null,
            suelo = "moon_01",
            tinteSuelo = new Color(0.72f, 0.72f, 0.76f),
            repetirSuelo = 26f,
            colorNiebla = new Color(0.02f, 0.02f, 0.05f),
            nieblaInicio = 180f,
            nieblaFin = 700f,
            colorSol = new Color(0.95f, 0.97f, 1f),
            intensidadSol = 2.1f,
            giroSol = new Vector3(30f, 40f, 0f),
            ambienteArriba = new Color(0.28f, 0.3f, 0.4f),
            ambienteMedio = new Color(0.2f, 0.2f, 0.26f),
            ambienteAbajo = new Color(0.1f, 0.1f, 0.12f),
            colorParticulas = Color.clear,
            musica = "MusicaLuna.ogg",
            musicaCombate = "MusicaCombateLuna.ogg",
            recompensa = 150,
            piel = PielAstronauta,
            gravedad = -5f,
            avisos = new[]
            {
                "Base Selene, emergencia. Tras los meteoritos hay un caso de hipoxia y uno de descompresión.",
                "Aquí la gravedad es baja: saltas más alto. Si ves una marca roja en el suelo, apártate."
            },
            leccion = "en un espacio cerrado sin aire seguro, sal con calma y sigue las rutas de evacuación. Revisar el equipo salva vidas.",
            construir = ConstruirLuna
        };
    }

    private static void ConstruirLuna(GruposNivel g)
    {
        Transform e = g.escenario;

        ConstruirCieloLunar(e);

        // Módulos principales.
        Poner(ESPACIO + "hangar_roundGlass.fbx", e, new Vector3(0f, 0f, 2f), 0f, 7f);
        Poner(ESPACIO + "hangar_roundA.fbx", e, new Vector3(-32f, 0f, 16f), 90f, 6f);
        Poner(ESPACIO + "hangar_roundB.fbx", e, new Vector3(32f, 0f, 16f), -90f, 6f);
        Poner(ESPACIO + "hangar_largeA.fbx", e, new Vector3(-32f, 0f, -24f), 0f, 6f);
        Poner(ESPACIO + "hangar_largeB.fbx", e, new Vector3(32f, 0f, -24f), 0f, 6f);
        Poner(ESPACIO + "hangar_smallA.fbx", e, new Vector3(0f, 0f, 44f), 180f, 7f);

        // Túneles que unen los módulos: obligan a rodearlos o a saltarlos con la gravedad baja.
        Tunel(g.obstaculos, new Vector3(-22f, 0f, 9f), new Vector3(-12f, 0f, 5f));
        Tunel(g.obstaculos, new Vector3(12f, 0f, 5f), new Vector3(22f, 0f, 9f));
        Tunel(g.obstaculos, new Vector3(0f, 0f, 14f), new Vector3(0f, 0f, 36f));

        // Pista de aterrizaje y cohete.
        SueloPlano(e, "Pista", new Vector3(0f, 0.03f, -47f), 22f, 14f, Mat("Pista", new Color(0.22f, 0.23f, 0.27f), 0.3f));
        Cohete(e, new Vector3(-16f, 0f, -48f));
        Poner(ESPACIO + "craft_cargoA.fbx", e, new Vector3(14f, 0f, -47f), 200f, 3.5f);
        Poner(ESPACIO + "rover.fbx", e, new Vector3(6f, 0f, -36f), 40f, 7f);
        Poner(ESPACIO + "rover.fbx", e, new Vector3(-44f, 0f, 2f), 120f, 7f);
        Poner(ESPACIO + "craft_miner.fbx", e, new Vector3(46f, 0f, -6f), 300f, 3f);

        foreach (Vector3 lugar in new[] { new Vector3(-46f, 0f, 44f), new Vector3(46f, 0f, 42f), new Vector3(46f, 0f, -44f) })
        {
            Poner(ESPACIO + "satelliteDish_large.fbx", e, lugar, lugar.x * 3f, 8f);
        }

        foreach (Vector3 lugar in new[] { new Vector3(-14f, 0f, -26f), new Vector3(14f, 0f, -26f), new Vector3(-20f, 0f, 30f), new Vector3(20f, 0f, 32f) })
        {
            Poner(ESPACIO + "machine_generatorLarge.fbx", e, lugar, lugar.z * 5f, 3f);
        }

        Vector3[] jeringas = { new Vector3(-26f, 0.9f, -38f), new Vector3(22f, 0.9f, -38f), new Vector3(-48f, 0.9f, -12f), new Vector3(48f, 0.9f, 8f), new Vector3(-12f, 0.9f, 30f), new Vector3(14f, 0.9f, 46f) };
        Vector3[] equipajes = { new Vector3(0f, 0.9f, -22f), new Vector3(-48f, 0.9f, 28f), new Vector3(48f, 0.9f, 26f), new Vector3(-18f, 0.9f, 46f) };
        Vector3[] botiquines = { new Vector3(-6f, 0.9f, -34f), new Vector3(42f, 0.9f, -16f), new Vector3(-42f, 0.9f, 8f) };
        Vector3[] pacientes = { new Vector3(-46f, 0f, 18f), new Vector3(46f, 0f, 16f) };

        List<Vector3> libres = new List<Vector3>(jeringas);
        libres.AddRange(equipajes);
        libres.AddRange(botiquines);
        libres.AddRange(pacientes);
        ConstruirSuperficieLunar(g.obstaculos, libres);

        // Fugas de plasma junto a los módulos dañados.
        foreach (Vector3 lugar in new[] { new Vector3(-10f, 0f, -14f), new Vector3(11f, 0f, -14f), new Vector3(-21f, 0f, -6f), new Vector3(23f, 0f, -4f), new Vector3(-40f, 0f, 30f), new Vector3(40f, 0f, 30f), new Vector3(0f, 0f, 26f) })
        {
            FugaPlasma(g.peligros, lugar);
        }

        Suministros(g.suministros, jeringas, equipajes, botiquines);

        Linea(g.monedas, new Vector3(0f, 0f, -44f), new Vector3(0f, 0f, -28f), 5);
        Arco(g.monedas, new Vector3(-18f, 0f, -14f), new Vector3(-18f, 0f, 0f), 6, 4f);
        Arco(g.monedas, new Vector3(18f, 0f, -14f), new Vector3(18f, 0f, 0f), 6, 4f);
        Linea(g.monedas, new Vector3(-48f, 0f, -38f), new Vector3(-48f, 0f, -18f), 5);
        Linea(g.monedas, new Vector3(48f, 0f, -38f), new Vector3(48f, 0f, -22f), 4);
        Linea(g.monedas, new Vector3(-16f, 0f, 22f), new Vector3(-8f, 0f, 44f), 5);
        Linea(g.monedas, new Vector3(16f, 0f, 22f), new Vector3(8f, 0f, 36f), 4);
        Arco(g.monedas, new Vector3(-6f, 0f, 25f), new Vector3(6f, 0f, 25f), 5, 4.5f);

        CrearPaciente(g.nivel, pacientes[0], 80f, 2, 1, PielAstronauta, "hipoxia");
        CrearPaciente(g.nivel, pacientes[1], -80f, 2, 2, PielAstronauta, "descompresion");

        LluviaMeteoritos lluvia = new GameObject("LluviaMeteoritos").AddComponent<LluviaMeteoritos>();
        lluvia.transform.SetParent(g.nivel, false);
        lluvia.meteorito = PrefabMeteorito();
        lluvia.marcador = PrefabMarcaImpacto();
    }

    private static void Tunel(Transform padre, Vector3 desde, Vector3 hasta)
    {
        Vector3 direccion = hasta - desde;
        float largo = direccion.magnitude;
        float giro = Quaternion.LookRotation(direccion).eulerAngles.y;

        const float TRAMO = 4f;
        int tramos = Mathf.CeilToInt(largo / TRAMO);
        for (int i = 0; i < tramos; i++)
        {
            Vector3 lugar = Vector3.Lerp(desde, hasta, (i + 0.5f) / tramos);
            string pieza = i % 2 == 0 ? "corridor_window.fbx" : "corridor_detailed.fbx";
            Poner(ESPACIO + pieza, padre, lugar, giro, TRAMO);
        }
    }

    private static void Cohete(Transform padre, Vector3 pos)
    {
        const float ESCALA = 4f;
        string[] piezas = { "rocket_baseA", "rocket_finsA", "rocket_fuelA", "rocket_sidesA", "rocket_sidesA", "rocket_topA" };
        float altura = 0f;

        foreach (string pieza in piezas)
        {
            GameObject parte = Poner(ESPACIO + pieza + ".fbx", padre, pos + Vector3.up * altura, 0f, ESCALA, altura == 0f);
            altura += MedirCaja(parte).size.y * 0.92f;
        }
    }

    private static void ConstruirSuperficieLunar(Transform o, List<Vector3> libres)
    {
        System.Random azar = new System.Random(31);
        float Rango(float a, float b) => a + (float)azar.NextDouble() * (b - a);

        string[] rocas = { "rock_largeA", "rock_largeB", "rock_crystals", "rock_crystalsLargeA", "rock_crystalsLargeB", "meteor", "meteor_half" };

        for (int i = 0; i < 42; i++)
        {
            Vector3 pos = new Vector3(Rango(-53f, 53f), 0f, Rango(-53f, 53f));
            if (Mathf.Abs(pos.x) < 9f && pos.z < -20f) continue;
            if (new Vector2(pos.x, pos.z - 2f).magnitude < 14f) continue;
            if (libres.Exists(l => Vector2.Distance(new Vector2(l.x, l.z), new Vector2(pos.x, pos.z)) < 6f)) continue;

            string roca = rocas[azar.Next(rocas.Length)];
            PintarRoca(Poner(ESPACIO + roca + ".fbx", o, pos, Rango(0f, 360f), Rango(3f, 6f)));
        }

        for (int i = 0; i < 16; i++)
        {
            Vector3 pos = new Vector3(Rango(-53f, 53f), 0.01f, Rango(-53f, 53f));
            PintarRoca(Poner(ESPACIO + (i % 2 == 0 ? "crater.fbx" : "craterLarge.fbx"), o, pos, Rango(0f, 360f), Rango(8f, 14f), false));
        }

        for (int i = 0; i < 30; i++)
        {
            Vector3 pos = new Vector3(Rango(-54f, 54f), 0.01f, Rango(-54f, 54f));
            PintarRoca(Poner(ESPACIO + (i % 2 == 0 ? "rocks_smallA.fbx" : "rocks_smallB.fbx"), o, pos, Rango(0f, 360f), Rango(2f, 4f), false));
        }
    }

    /// <summary>Las rocas del kit son rojizas como Marte; aquí se pintan de gris lunar.</summary>
    private static void PintarRoca(GameObject roca)
    {
        Material gris = Mat("RocaLunar", new Color(0.5f, 0.5f, 0.53f), 0.08f);
        Material oscura = Mat("RocaLunarOscura", new Color(0.3f, 0.3f, 0.33f), 0.08f);
        Material cristal = Mat("Cristal", new Color(0.3f, 0.85f, 0.95f), 0.85f, 0f, new Color(0.1f, 0.5f, 0.6f));

        foreach (Renderer parte in roca.GetComponentsInChildren<Renderer>())
        {
            Material[] materiales = parte.sharedMaterials;
            for (int i = 0; i < materiales.Length; i++)
            {
                string nombre = materiales[i] != null ? materiales[i].name : string.Empty;
                if (nombre.StartsWith("rockDark")) materiales[i] = oscura;
                else if (nombre.StartsWith("rock")) materiales[i] = gris;
                else if (nombre.StartsWith("crystal")) materiales[i] = cristal;
            }
            parte.sharedMaterials = materiales;
        }
    }

    private static void FugaPlasma(Transform padre, Vector3 pos)
    {
        GameObject fuga = Fuego(padre, pos);
        fuga.name = "FugaPlasma";

        Material plasma = MatParticula("ParticulaPlasma", true);
        ParticleSystem sistema = fuga.GetComponent<ParticleSystem>();
        ParticleSystem.MainModule principal = sistema.main;
        principal.startColor = new ParticleSystem.MinMaxGradient(new Color(0.3f, 0.75f, 1f, 0.9f), new Color(0.5f, 0.3f, 1f, 0.8f));
        fuga.GetComponent<ParticleSystemRenderer>().sharedMaterial = plasma;

        ParticleSystem.ColorOverLifetimeModule color = sistema.colorOverLifetime;
        Gradient degradado = new Gradient();
        degradado.SetKeys(
            new[] { new GradientColorKey(Color.white, 0f), new GradientColorKey(new Color(0.4f, 0.5f, 1f), 1f) },
            new[] { new GradientAlphaKey(0f, 0f), new GradientAlphaKey(1f, 0.15f), new GradientAlphaKey(0f, 1f) });
        color.color = degradado;

        fuga.GetComponentInChildren<Light>().color = new Color(0.35f, 0.6f, 1f);
    }

    private static void ConstruirCieloLunar(Transform e)
    {
        // La Tierra en el horizonte.
        Material tierra = Mat("Tierra", Color.white, 0.4f, 0f, new Color(0.22f, 0.3f, 0.45f));
        tierra.SetTexture("_BaseMap", TexturaTierra());
        tierra.SetTexture("_EmissionMap", TexturaTierra());
        EditorUtility.SetDirty(tierra);

        GameObject planeta = Primitiva(PrimitiveType.Sphere, e, "Tierra", Vector3.zero, Vector3.one * 110f, tierra, new Vector3(20f, 40f, 0f));
        planeta.transform.position = new Vector3(120f, 170f, 420f);
        planeta.GetComponent<Renderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
    }

    /// <summary>Genera el cielo de estrellas.</summary>
    private static Material MatCieloEstrellas()
    {
        string ruta = DIR + "/CieloEstrellas.png";
        if (!File.Exists(ruta))
        {
            const int ANCHO = 2048;
            const int ALTO = 1024;
            Color[] pixeles = new Color[ANCHO * ALTO];
            for (int i = 0; i < pixeles.Length; i++) pixeles[i] = new Color(0.006f, 0.007f, 0.016f);

            System.Random azar = new System.Random(9);
            for (int i = 0; i < 5200; i++)
            {
                int x = azar.Next(ANCHO);
                int y = azar.Next(ALTO);
                float brillo = Mathf.Pow((float)azar.NextDouble(), 3f) * 0.9f + 0.1f;
                Color tono = Color.Lerp(new Color(0.75f, 0.85f, 1f), new Color(1f, 0.9f, 0.75f), (float)azar.NextDouble());
                pixeles[y * ANCHO + x] = tono * brillo;
                if (brillo > 0.6f && x + 1 < ANCHO) pixeles[y * ANCHO + x + 1] = tono * brillo * 0.6f;
            }

            Texture2D textura = new Texture2D(ANCHO, ALTO, TextureFormat.RGB24, false);
            textura.SetPixels(pixeles);
            File.WriteAllBytes(ruta, textura.EncodeToPNG());
            AssetDatabase.ImportAsset(ruta);
        }

        return MatCieloPanoramico("CieloLuna", Cargar<Texture>(ruta));
    }

    private static Texture2D TexturaTierra()
    {
        string ruta = DIR + "/Tierra.png";
        if (!File.Exists(ruta))
        {
            const int ANCHO = 512;
            const int ALTO = 256;
            Texture2D textura = new Texture2D(ANCHO, ALTO, TextureFormat.RGB24, false);

            for (int y = 0; y < ALTO; y++)
            {
                for (int x = 0; x < ANCHO; x++)
                {
                    float tierra = Mathf.PerlinNoise(x * 0.018f + 3f, y * 0.026f + 7f) + Mathf.PerlinNoise(x * 0.05f, y * 0.06f) * 0.3f;
                    float nubes = Mathf.PerlinNoise(x * 0.03f + 40f, y * 0.05f + 12f);

                    Color color = tierra > 0.75f ? new Color(0.28f, 0.48f, 0.22f) : new Color(0.08f, 0.26f, 0.62f);
                    if (nubes > 0.62f) color = Color.Lerp(color, Color.white, 0.8f);
                    textura.SetPixel(x, y, color);
                }
            }

            File.WriteAllBytes(ruta, textura.EncodeToPNG());
            AssetDatabase.ImportAsset(ruta);
        }
        return Cargar<Texture2D>(ruta);
    }

    private static GameObject PrefabMeteorito()
    {
        string ruta = DIR_PREFAB + "/MeteoritoCaida.prefab";
        GameObject raiz = (GameObject)PrefabUtility.InstantiatePrefab(Cargar<GameObject>(ESPACIO + "meteor.fbx"));
        raiz.transform.localScale = Vector3.one * 3f;
        PintarRoca(raiz);
        GameObject prefab = PrefabUtility.SaveAsPrefabAsset(raiz, ruta);
        Object.DestroyImmediate(raiz);
        return prefab;
    }

    private static GameObject PrefabMarcaImpacto()
    {
        Material rojo = Mat("MarcaImpacto", new Color(1f, 0.1f, 0.05f), 0f, 0f, new Color(1f, 0.1f, 0.03f));
        GameObject raiz = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        Object.DestroyImmediate(raiz.GetComponent<Collider>());
        raiz.name = "MarcaImpacto";
        raiz.transform.localScale = new Vector3(8f, 0.01f, 8f);
        raiz.GetComponent<Renderer>().sharedMaterial = rojo;

        GameObject prefab = PrefabUtility.SaveAsPrefabAsset(raiz, DIR_PREFAB + "/MarcaImpacto.prefab");
        Object.DestroyImmediate(raiz);
        return prefab;
    }

    private static List<CaminoEnemigo> EnemigosNivel3()
    {
        return new List<CaminoEnemigo>
        {
            new CaminoEnemigo("Androide_Sur", new[] { new Vector3(-20f, 0f, -28f), new Vector3(20f, 0f, -28f), new Vector3(16f, 0f, -18f), new Vector3(-16f, 0f, -18f) }),
            new CaminoEnemigo("Androide_Oeste", new[] { new Vector3(-48f, 0f, -6f), new Vector3(-48f, 0f, 34f), new Vector3(-20f, 0f, 36f), new Vector3(-16f, 0f, 24f) }),
            new CaminoEnemigo("Androide_Este", new[] { new Vector3(48f, 0f, -8f), new Vector3(48f, 0f, 34f), new Vector3(20f, 0f, 38f), new Vector3(16f, 0f, 24f) }),
            new CaminoEnemigo("Androide_Norte", new[] { new Vector3(-16f, 0f, 48f), new Vector3(18f, 0f, 48f), new Vector3(10f, 0f, 30f), new Vector3(-10f, 0f, 30f) })
        };
    }
}
