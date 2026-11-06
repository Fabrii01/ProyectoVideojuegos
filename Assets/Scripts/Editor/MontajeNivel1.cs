using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Nivel 1, terremoto en el centro: cuatro manzanas alrededor de una plaza.
/// Las calles están cortadas por escombros y hay dos heridos en lotes derrumbados.
/// </summary>
public static partial class MontajeRescate
{
    private const string CIUDAD_KIT = KENNEY + "Ciudad/building-";

    private static NivelDatos DatosNivel1()
    {
        return new NivelDatos
        {
            numero = 1,
            escena = "Nivel1",
            medio = 56f,
            inicio = new Vector3(-47f, 1.05f, -47f),
            cielo = "belfast_sunset_puresky",
            suelo = "cracked_concrete",
            tinteSuelo = new Color(0.7f, 0.66f, 0.62f),
            repetirSuelo = 34f,
            colorNiebla = new Color(0.62f, 0.5f, 0.42f),
            nieblaInicio = 30f,
            nieblaFin = 260f,
            colorSol = new Color(1f, 0.78f, 0.6f),
            intensidadSol = 1.5f,
            giroSol = new Vector3(24f, -55f, 0f),
            ambienteArriba = new Color(0.62f, 0.56f, 0.56f),
            ambienteMedio = new Color(0.52f, 0.46f, 0.42f),
            ambienteAbajo = new Color(0.24f, 0.2f, 0.18f),
            colorParticulas = new Color(0.75f, 0.68f, 0.6f, 0.5f),
            musica = "MusicaNivel.ogg",
            musicaCombate = "MusicaCombate.mp3",
            recompensa = 60,
            piel = PielRescatista,
            avisos = new[]
            {
                "Réplica de terremoto en el centro. Hay dos heridos atrapados al norte de la ciudad.",
                "Reúne jeringas y equipajes por las manzanas y llévalos a cada paciente. Mira el mapa."
            },
            leccion = "en un sismo agáchate, cúbrete y sujétate. Después aléjate de muros y vidrios dañados.",
            construir = ConstruirCiudad
        };
    }

    private static void ConstruirCiudad(GruposNivel g)
    {
        Transform e = g.escenario;

        // Edificios de cada manzana. Los vacíos son lotes derrumbados con heridos o el puesto inicial.
        string[] tipos = { "a", "b", "d", "f", "g", "h", "i", "c", "l", "m" };
        int indice = 0;
        foreach (float sx in new[] { -1f, 1f })
        {
            foreach (float sz in new[] { -1f, 1f })
            {
                foreach (Vector2 lugar in new[] { new Vector2(16f, 16f), new Vector2(34f, 16f), new Vector2(16f, 34f), new Vector2(34f, 34f) })
                {
                    bool lote = lugar.x == 34f && lugar.y == 34f && sz > 0f;
                    bool inicio = lugar.x == 34f && lugar.y == 34f && sx < 0f && sz < 0f;
                    if (lote || inicio) continue;

                    string tipo = tipos[indice++ % tipos.Length];
                    Edificio(e, tipo, new Vector3(sx * lugar.x, 0f, sz * lugar.y), indice * 90f, indice % 3 == 0);
                }
            }
        }

        // Rascacielos de fondo, fuera de la zona jugable.
        foreach (Vector3 lugar in new[] { new Vector3(-85f, 0f, -30f), new Vector3(-82f, 0f, 45f), new Vector3(-20f, 0f, 88f), new Vector3(52f, 0f, 84f), new Vector3(86f, 0f, 20f), new Vector3(84f, 0f, -48f), new Vector3(20f, 0f, -86f), new Vector3(-44f, 0f, -84f) })
        {
            Poner(CIUDAD_KIT + "skyscraper-" + "abc"[Mathf.Abs((int)lugar.x) % 3] + ".fbx", e, lugar, lugar.x, 13f, false);
        }

        ConstruirPlazaRota(e);
        ConstruirEscombrosCiudad(g.obstaculos);

        // Puesto inicial.
        Ambulancia(e, new Vector3(-37f, 0f, -44f), 80f);
        Carpa(e, new Vector3(-46f, 0f, -36f), 90f, 4.4f, 3.6f, Mat("LonaBlanca", new Color(0.86f, 0.86f, 0.83f), 0.15f), true);
        foreach (Vector3 lugar in new[] { new Vector3(-41f, 0f, -49f), new Vector3(-39f, 0f, -49f), new Vector3(-37f, 0f, -49f) })
        {
            Poner(KENNEY + "Vehiculos/cone.fbx", e, lugar, 0f, 1.2f, false);
        }

        // Peligros: autos en llamas y gas en las esquinas.
        foreach (Vector3 lugar in new[] { new Vector3(-4f, 0f, -24f), new Vector3(-24f, 0f, 3f), new Vector3(24f, 0f, -4f), new Vector3(4f, 0f, 25f), new Vector3(46f, 0f, 10f), new Vector3(-46f, 0f, 22f), new Vector3(25.5f, 0f, 30f), new Vector3(-25.5f, 0f, -26f) })
        {
            Fuego(g.peligros, lugar);
            Humo(g.peligros, lugar + Vector3.up * 0.5f);
        }

        Suministros(g.suministros,
            new[] { new Vector3(-25f, 0.9f, -40f), new Vector3(25f, 0.9f, -20f), new Vector3(-48f, 0.9f, 14f), new Vector3(48f, 0.9f, -28f), new Vector3(0f, 0.9f, 44f), new Vector3(-25f, 0.9f, 20f) },
            new[] { new Vector3(25f, 0.9f, -40f), new Vector3(48f, 0.9f, 30f), new Vector3(-16f, 0.9f, 47f) },
            new[] { new Vector3(0f, 0.9f, -42f), new Vector3(0f, 0.9f, 11f), new Vector3(-48f, 0.9f, 40f) });

        // Monedas: marcan los recorridos y premian entrar por los callejones.
        Linea(g.monedas, new Vector3(-44f, 0f, -44f), new Vector3(-12f, 0f, -44f), 7);
        Linea(g.monedas, new Vector3(-25f, 0f, -36f), new Vector3(-25f, 0f, -8f), 5);
        Linea(g.monedas, new Vector3(25f, 0f, -36f), new Vector3(25f, 0f, -8f), 5);
        Linea(g.monedas, new Vector3(-46f, 0f, -20f), new Vector3(-46f, 0f, 8f), 5);
        Linea(g.monedas, new Vector3(47f, 0f, -44f), new Vector3(47f, 0f, -8f), 6);
        Linea(g.monedas, new Vector3(-8f, 0f, 0f), new Vector3(8f, 0f, 0f), 4);
        Linea(g.monedas, new Vector3(-25f, 0f, 10f), new Vector3(-25f, 0f, 38f), 5);
        Linea(g.monedas, new Vector3(25f, 0f, 10f), new Vector3(25f, 0f, 24f), 3);
        Linea(g.monedas, new Vector3(-12f, 0f, 47f), new Vector3(20f, 0f, 47f), 6);
        Arco(g.monedas, new Vector3(0f, 0f, -26f), new Vector3(0f, 0f, -22f), 3, 1.8f);

        // Heridos en los lotes derrumbados del norte.
        CrearPaciente(g.nivel, new Vector3(-34f, 0f, 36f), 40f, 2, 1, PielVecino(3));
        CrearPaciente(g.nivel, new Vector3(36f, 0f, 36f), -30f, 2, 1, PielVecino(1));
        LoteDerrumbado(g.obstaculos, new Vector3(-34f, 0f, 34f));
        LoteDerrumbado(g.obstaculos, new Vector3(35f, 0f, 34f));
    }

    private static void Edificio(Transform padre, string tipo, Vector3 pos, float giro, bool inclinado)
    {
        GameObject edificio = Poner(CIUDAD_KIT + tipo + ".fbx", padre, pos, giro, 11f);
        if (!inclinado) return;

        // Algunos quedaron ladeados por el sismo.
        edificio.transform.rotation = Quaternion.Euler(3.5f, giro, 4.5f);
        edificio.transform.position = pos + Vector3.down * 0.4f;
    }

    private static void ConstruirPlazaRota(Transform padre)
    {
        Material piedra = Mat("PiedraClara", new Color(0.74f, 0.72f, 0.68f), 0.2f);
        Material agua = Mat("Agua", new Color(0.25f, 0.55f, 0.75f), 0.9f);

        Primitiva(PrimitiveType.Cylinder, padre, "FuenteBorde", new Vector3(0f, 0.3f, 0f), new Vector3(7f, 0.3f, 7f), piedra, null, true);
        Primitiva(PrimitiveType.Cylinder, padre, "FuenteAgua", new Vector3(0f, 0.58f, 0f), new Vector3(6.2f, 0.03f, 6.2f), agua);
        Primitiva(PrimitiveType.Cylinder, padre, "ColumnaCaida", new Vector3(1.2f, 1.1f, 0.4f), new Vector3(0.8f, 2f, 0.8f), piedra, new Vector3(70f, 30f, 0f), true);

        foreach (Vector3 lugar in new[] { new Vector3(-6.2f, 0f, 6.2f), new Vector3(6.2f, 0f, 6.4f) })
        {
            Poner(CIUDAD + "bench.prefab", padre, lugar, lugar.x * 10f, 0.8f);
        }
    }

    private static void ConstruirEscombrosCiudad(Transform o)
    {
        // Cortes en las calles principales: se saltan o se rodean por el hueco.
        Barricada(o, new Vector3(0f, 0f, -24f), 0f);
        Barricada(o, new Vector3(-24f, 0f, 0f), 90f);
        Barricada(o, new Vector3(24f, 0f, 0f), 90f);
        Barricada(o, new Vector3(0f, 0f, 24f), 0f);

        // Autos abandonados y chocados.
        Vehiculo("sedan", o, new Vector3(3.5f, 0f, -36f), 25f);
        Vehiculo("taxi", o, new Vector3(-3f, 0f, -14f), 160f);
        Vehiculo("van", o, new Vector3(-13f, 0f, 3f), 85f);
        Vehiculo("suv", o, new Vector3(12f, 0f, -3f), 280f);
        Vehiculo("truck", o, new Vector3(-3f, 0f, 37f), 200f);
        Vehiculo("police", o, new Vector3(44f, 0f, -14f), 12f);
        Vehiculo("garbage-truck", o, new Vector3(-44f, 0f, 2f), 170f);
        Vehiculo("firetruck", o, new Vector3(46f, 0f, 46f), 220f);

        foreach (string pieza in new[] { "debris-door", "debris-tire", "debris-plate-a", "debris-plate-b", "debris-bumper" })
        {
            for (int i = 0; i < 3; i++)
            {
                float angulo = (pieza.Length * 37 + i * 121) * Mathf.Deg2Rad;
                Vector3 lugar = new Vector3(Mathf.Cos(angulo) * (12f + i * 15f), 0f, Mathf.Sin(angulo) * (12f + i * 15f));
                Poner(KENNEY + "Vehiculos/" + pieza + ".fbx", o, lugar, i * 70f, 1.6f, false);
            }
        }

        // Rocas sueltas en los callejones.
        Roca(o, new Vector3(-25f, 0.1f, -27f), 2.3f, 30f, 1);
        Roca(o, new Vector3(25f, 0.1f, -30f), 2.6f, 80f, 2);
        Roca(o, new Vector3(25.8f, 0.1f, 14f), 2f, 140f, 3);
        Roca(o, new Vector3(-26f, 0.1f, 30f), 2.2f, 220f, 1);
        Caja(o, new Vector3(-24f, 0f, 44f), 20f);
        Caja(o, new Vector3(24f, 0f, 44f), 70f);
    }

    private static void Barricada(Transform o, Vector3 pos, float giro)
    {
        Quaternion rotacion = Quaternion.Euler(0f, giro, 0f);
        Vector3 lado = rotacion * Vector3.right;

        // Bloques saltables de un lado y un paso libre del otro.
        foreach (float d in new[] { -6.2f, -3.1f, 0f })
        {
            Poner(CIUDAD + "concrete_block.prefab", o, pos + lado * d, giro);
        }
        Roca(o, pos + lado * 6.4f + Vector3.up * 0.1f, 2.4f, giro + 30f, 2);
    }

    private static void LoteDerrumbado(Transform o, Vector3 centro)
    {
        Losa(o, centro + new Vector3(-7f, 1.2f, 3f), new Vector3(7f, 2.4f, 0.6f), new Vector3(0f, 24f, 4f));
        Losa(o, centro + new Vector3(7f, 1f, 4f), new Vector3(6f, 2f, 0.6f), new Vector3(3f, -30f, 0f));
        Losa(o, centro + new Vector3(0f, 0.8f, 8.5f), new Vector3(9f, 0.4f, 4f), new Vector3(12f, 8f, 0f));
        Roca(o, centro + new Vector3(-8f, 0.1f, -5f), 2.6f, 60f, 1);
        Roca(o, centro + new Vector3(8f, 0.1f, -4f), 2f, 200f, 3);
    }

    private static List<CaminoEnemigo> EnemigosNivel1()
    {
        return new List<CaminoEnemigo>
        {
            new CaminoEnemigo("Saqueador_Plaza", new[] { new Vector3(-7f, 0f, -7f), new Vector3(7f, 0f, -7f), new Vector3(7f, 0f, 7f), new Vector3(-7f, 0f, 7f) }),
            new CaminoEnemigo("Saqueador_Este", new[] { new Vector3(47f, 0f, -40f), new Vector3(47f, 0f, 2f), new Vector3(25f, 0f, 5f), new Vector3(25f, 0f, -38f) }),
            new CaminoEnemigo("Saqueador_Oeste", new[] { new Vector3(-47f, 0f, 8f), new Vector3(-47f, 0f, 44f), new Vector3(-25f, 0f, 46f), new Vector3(-25f, 0f, 8f) }),
            new CaminoEnemigo("Saqueador_Norte", new[] { new Vector3(-16f, 0f, 46f), new Vector3(24f, 0f, 46f), new Vector3(24f, 0f, 30f), new Vector3(8f, 0f, 30f) })
        };
    }
}
