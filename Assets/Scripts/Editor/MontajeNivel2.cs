using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Nivel 2, incendio forestal: el fuego avanza desde el sur y un río corta el bosque.
/// Hay que cruzar por dos puentes hasta el campamento y la cabaña del guardabosques.
/// </summary>
public static partial class MontajeRescate
{
    private const string SUPERVIVENCIA = KENNEY + "Supervivencia/";
    private const float RIO_Z = 6f;

    private static readonly Vector3 PuenteOeste = new Vector3(-26f, 0f, RIO_Z);
    private static readonly Vector3 PuenteEste = new Vector3(28f, 0f, RIO_Z);

    private static NivelDatos DatosNivel2()
    {
        return new NivelDatos
        {
            numero = 2,
            escena = "Nivel2",
            medio = 56f,
            inicio = new Vector3(0f, 1.05f, -48f),
            cielo = "industrial_sunset_puresky",
            suelo = "forest_floor",
            tinteSuelo = new Color(0.62f, 0.6f, 0.5f),
            repetirSuelo = 40f,
            colorNiebla = new Color(0.62f, 0.38f, 0.22f),
            nieblaInicio = 12f,
            nieblaFin = 130f,
            colorSol = new Color(1f, 0.62f, 0.36f),
            intensidadSol = 1.3f,
            giroSol = new Vector3(18f, 120f, 0f),
            ambienteArriba = new Color(0.6f, 0.46f, 0.4f),
            ambienteMedio = new Color(0.5f, 0.38f, 0.3f),
            ambienteAbajo = new Color(0.2f, 0.15f, 0.12f),
            colorParticulas = new Color(1f, 0.55f, 0.2f, 0.9f),
            musica = "MusicaBosque.ogg",
            musicaCombate = "MusicaCombate.mp3",
            recompensa = 90,
            piel = PielBrigadista,
            avisos = new[]
            {
                "Incendio en la reserva. Hay campistas heridos al otro lado del río.",
                "El agua no se puede cruzar: usa los puentes. Cuidado con los furtivos y el fuego."
            },
            leccion = "ante un incendio aléjate del humo, cúbrete nariz y boca y avanza agachado. Nunca dejes una fogata encendida.",
            construir = ConstruirBosque
        };
    }

    private static void ConstruirBosque(GruposNivel g)
    {
        Transform e = g.escenario;

        ConstruirRio(e);

        // Lugares que deben quedar libres de árboles.
        List<Vector3> libres = new List<Vector3> { new Vector3(0f, 0f, -48f), PuenteOeste, PuenteEste };
        Vector3 campamento = new Vector3(-30f, 0f, 36f);
        Vector3 cabana = new Vector3(32f, 0f, 38f);
        libres.Add(campamento);
        libres.Add(cabana);

        Vector3[] jeringas = { new Vector3(-40f, 0.9f, -30f), new Vector3(36f, 0.9f, -34f), new Vector3(10f, 0.9f, -8f), new Vector3(-8f, 0.9f, 24f), new Vector3(46f, 0.9f, 20f) };
        Vector3[] equipajes = { new Vector3(-14f, 0.9f, -24f), new Vector3(44f, 0.9f, -8f), new Vector3(-46f, 0.9f, 20f), new Vector3(8f, 0.9f, 44f) };
        Vector3[] botiquines = { new Vector3(16f, 0.9f, -38f), new Vector3(-30f, 0.9f, -6f), new Vector3(0f, 0.9f, 16f) };
        libres.AddRange(jeringas);
        libres.AddRange(equipajes);
        libres.AddRange(botiquines);

        Bosque(g.obstaculos, libres);

        // Puesto de brigadistas al sur.
        Vehiculo("firetruck", e, new Vector3(-7f, 0f, -49f), 80f);
        Vehiculo("suv", e, new Vector3(8f, 0f, -50f), 280f);
        Carpa(e, new Vector3(0f, 0f, -53f), 0f, 4.4f, 3f, Mat("LonaNaranja", new Color(0.92f, 0.45f, 0.1f), 0.2f), false);

        // Frente de fuego que cruza el bosque, con huecos para pasar.
        foreach (float x in new[] { -50f, -46f, -42f, -30f, -26f, -22f, -6f, -2f, 2f, 18f, 22f, 26f, 44f, 48f })
        {
            Fuego(g.peligros, new Vector3(x, 0f, -18f + Mathf.Sin(x) * 2f));
        }
        foreach (float x in new[] { -40f, -8f, 12f, 40f })
        {
            Fuego(g.peligros, new Vector3(x, 0f, 22f));
            Humo(g.peligros, new Vector3(x, 0.5f, 22f));
        }
        foreach (float x in new[] { -48f, -24f, 0f, 24f, 48f })
        {
            Humo(g.peligros, new Vector3(x, 0.5f, -19f));
        }

        // Campamento y cabaña.
        Campamento(e, campamento);
        Cabana(e, cabana);
        CrearPaciente(g.nivel, campamento + new Vector3(0f, 0f, -3f), 20f, 2, 1, PielVecino(2));
        CrearPaciente(g.nivel, cabana + new Vector3(-1f, 0f, -6f), -20f, 1, 2, PielVecino(4));

        Suministros(g.suministros, jeringas, equipajes, botiquines);

        Linea(g.monedas, new Vector3(0f, 0f, -44f), new Vector3(-10f, 0f, -24f), 5);
        Linea(g.monedas, new Vector3(10f, 0f, -24f), new Vector3(12f, 0f, -8f), 4);
        Linea(g.monedas, new Vector3(-12f, 0f, -14f), new Vector3(-24f, 0f, 0f), 4);
        Linea(g.monedas, new Vector3(-26f, 0f, 2f), new Vector3(-26f, 0f, 12f), 4);
        Linea(g.monedas, new Vector3(28f, 0f, 2f), new Vector3(28f, 0f, 12f), 4);
        Linea(g.monedas, new Vector3(36f, 0f, -14f), new Vector3(32f, 0f, -2f), 4);
        Linea(g.monedas, new Vector3(-20f, 0f, 16f), new Vector3(-28f, 0f, 28f), 4);
        Linea(g.monedas, new Vector3(20f, 0f, 16f), new Vector3(30f, 0f, 28f), 4);
        Linea(g.monedas, new Vector3(-10f, 0f, 30f), new Vector3(10f, 0f, 40f), 5);
    }

    private static void ConstruirRio(Transform e)
    {
        Material agua = Mat("AguaRio", new Color(0.2f, 0.42f, 0.55f), 0.92f);
        Material madera = Mat("Madera", new Color(0.42f, 0.28f, 0.16f), 0.2f);

        SueloPlano(e, "Rio", new Vector3(0f, 0.05f, RIO_Z), 120f, 9f, agua);

        // Solo se cruza por los puentes.
        float[] cortes = { -60f, PuenteOeste.x - 2.5f, PuenteOeste.x + 2.5f, PuenteEste.x - 2.5f, PuenteEste.x + 2.5f, 60f };
        for (int i = 0; i < cortes.Length; i += 2)
        {
            float ancho = cortes[i + 1] - cortes[i];
            Limite(e, "OrillaRio", new Vector3(cortes[i] + ancho * 0.5f, 5f, RIO_Z), new Vector3(ancho, 12f, 8f));
        }

        foreach (Vector3 puente in new[] { PuenteOeste, PuenteEste })
        {
            Primitiva(PrimitiveType.Cube, e, "Puente", puente + Vector3.up * 0.1f, new Vector3(5f, 0.12f, 11f), madera);
            foreach (float lado in new[] { -2.5f, 2.5f })
            {
                Primitiva(PrimitiveType.Cube, e, "Baranda", puente + new Vector3(lado, 0.8f, 0f), new Vector3(0.14f, 0.12f, 11f), madera);
                for (float z = -5f; z <= 5f; z += 2.5f)
                {
                    Primitiva(PrimitiveType.Cube, e, "Poste", puente + new Vector3(lado, 0.45f, z), new Vector3(0.14f, 0.8f, 0.14f), madera);
                }
            }
        }

        for (int i = 0; i < 24; i++)
        {
            float x = -56f + i * 5f;
            float z = RIO_Z + (i % 2 == 0 ? -5f : 5f);
            if (Mathf.Abs(x - PuenteOeste.x) < 5f || Mathf.Abs(x - PuenteEste.x) < 5f) continue;
            Vegetal("rock_largeA", e, new Vector3(x, 0f, z), i * 40f, 0.9f);
        }
    }

    private static void Bosque(Transform raiz, List<Vector3> libres)
    {
        System.Random azar = new System.Random(5);
        float Rango(float a, float b) => a + (float)azar.NextDouble() * (b - a);

        string[] vivos = { "tree_pineDefaultA", "tree_pineDefaultB", "tree_default", "tree_oak", "tree_cone", "tree_default_fall", "tree_oak_fall" };
        string[] quemados = { "tree_default_dark", "tree_oak_dark", "tree_cone_dark", "tree_detailed" };

        List<Vector3> ocupados = new List<Vector3>();
        for (int intento = 0; intento < 900 && ocupados.Count < 170; intento++)
        {
            Vector3 pos = new Vector3(Rango(-54f, 54f), 0f, Rango(-54f, 54f));

            if (Mathf.Abs(pos.z - RIO_Z) < 6.5f) continue;
            if (libres.Exists(l => Vector3.Distance(l, pos) < 5f)) continue;
            if (ocupados.Exists(o => Vector3.Distance(o, pos) < 5f)) continue;

            ocupados.Add(pos);

            // Al sur del río el fuego ya pasó: predominan los árboles quemados.
            bool quemado = pos.z < RIO_Z && azar.NextDouble() < 0.75;
            string nombre = quemado ? quemados[azar.Next(quemados.Length)] : vivos[azar.Next(vivos.Length)];

            GameObject arbol = Vegetal(nombre, raiz, pos, Rango(0f, 360f), Rango(6f, 10f), true);
            if (quemado) Carbonizar(arbol);
        }

        for (int i = 0; i < 70; i++)
        {
            Vector3 pos = new Vector3(Rango(-54f, 54f), 0f, Rango(-54f, 54f));
            if (Mathf.Abs(pos.z - RIO_Z) < 6f) continue;

            string[] detalles = { "stump_old", "stump_oldTall", "log_large", "rock_largeB", "plant_bush", "mushroom_redGroup", "grass_large" };
            string detalle = detalles[azar.Next(detalles.Length)];
            Vegetal(detalle, raiz, pos, Rango(0f, 360f), detalle.StartsWith("rock") ? 1.2f : 0.7f);
        }
    }

    private static void Carbonizar(GameObject arbol)
    {
        Material carbon = Mat("Carbon", new Color(0.08f, 0.07f, 0.07f), 0.05f);
        Material brasa = Mat("Brasa", new Color(0.16f, 0.1f, 0.08f), 0.05f, 0f, new Color(0.22f, 0.05f, 0.01f));

        foreach (Renderer parte in arbol.GetComponentsInChildren<Renderer>())
        {
            Material[] materiales = new Material[parte.sharedMaterials.Length];
            for (int i = 0; i < materiales.Length; i++) materiales[i] = i == 0 ? brasa : carbon;
            parte.sharedMaterials = materiales;
        }
    }

    private static void Campamento(Transform e, Vector3 centro)
    {
        Poner(SUPERVIVENCIA + "tent-canvas.fbx", e, centro + new Vector3(-8f, 0f, 6f), 150f, 5f);
        Poner(SUPERVIVENCIA + "tent.fbx", e, centro + new Vector3(8f, 0f, 6f), 210f, 5f);
        Poner(SUPERVIVENCIA + "tent-canvas-half.fbx", e, centro + new Vector3(0f, 0f, 9f), 180f, 5f);
        Poner(SUPERVIVENCIA + "campfire-pit.fbx", e, centro + new Vector3(0f, 0f, 3f), 0f, 4f, false);
        Poner(SUPERVIVENCIA + "bedroll.fbx", e, centro + new Vector3(-4f, 0.05f, 2f), 30f, 3.5f, false);
        Poner(SUPERVIVENCIA + "box-large.fbx", e, centro + new Vector3(5f, 0f, 2f), 10f, 3f);
        Poner(SUPERVIVENCIA + "signpost.fbx", e, centro + new Vector3(4f, 0f, -7f), 20f, 4f, false);
    }

    private static void Cabana(Transform e, Vector3 centro)
    {
        // Cabaña armada con módulos del kit de supervivencia.
        foreach (Vector3 lugar in new[] { new Vector3(-2.5f, 0f, 3f), new Vector3(2.5f, 0f, 3f), new Vector3(-2.5f, 0f, 8f), new Vector3(2.5f, 0f, 8f) })
        {
            Poner(SUPERVIVENCIA + "structure.fbx", e, centro + lugar, 0f, 10f);
            Poner(SUPERVIVENCIA + "structure-roof.fbx", e, centro + lugar + Vector3.up * 5f, 0f, 10f, false);
        }

        Poner(SUPERVIVENCIA + "workbench.fbx", e, centro + new Vector3(7f, 0f, 2f), 90f, 4f);
        Poner(SUPERVIVENCIA + "barrel.fbx", e, centro + new Vector3(-7f, 0f, 1f), 0f, 4f);
        Poner(SUPERVIVENCIA + "resource-wood.fbx", e, centro + new Vector3(-7f, 0f, 4f), 40f, 5f);
        Vehiculo("truck", e, centro + new Vector3(10f, 0f, -5f), 200f);
    }

    private static List<CaminoEnemigo> EnemigosNivel2()
    {
        return new List<CaminoEnemigo>
        {
            new CaminoEnemigo("Furtivo_Sur", new[] { new Vector3(-22f, 0f, -30f), new Vector3(-8f, 0f, -36f), new Vector3(20f, 0f, -30f), new Vector3(10f, 0f, -24f) }),
            new CaminoEnemigo("Furtivo_Puente", new[] { new Vector3(-26f, 0f, -6f), new Vector3(-12f, 0f, -8f), new Vector3(14f, 0f, -10f), new Vector3(28f, 0f, -6f) }),
            new CaminoEnemigo("Furtivo_Campamento", new[] { new Vector3(-30f, 0f, 16f), new Vector3(-18f, 0f, 28f), new Vector3(-30f, 0f, 26f), new Vector3(-42f, 0f, 28f) }),
            new CaminoEnemigo("Furtivo_Cabana", new[] { new Vector3(24f, 0f, 16f), new Vector3(38f, 0f, 22f), new Vector3(22f, 0f, 30f), new Vector3(10f, 0f, 24f) })
        };
    }
}
