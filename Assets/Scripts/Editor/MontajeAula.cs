using TMPro;
using UnityEngine;

/// <summary>
/// Aula de salud del Hub: el espacio del NPC de preguntas, frente al hospital.
/// También guarda las conversaciones de los vecinos.
/// </summary>
public static partial class MontajeRescate
{
    private static readonly Vector3 PosTienda = new Vector3(-18f, 0f, 3f);
    private static readonly Vector3 PosAula = new Vector3(-1f, 0f, 1.5f);
    private static readonly Vector3 PosMando = new Vector3(17f, 0f, 5f);

    private static readonly string[][] Conversaciones =
    {
        new[] { "¿Vas a la ciudad? Tras el terremoto hay mucho polvo en el aire.", "El polvo puede provocar crisis de asma.", "Si a alguien le cuesta respirar, aléjalo del polvo y siéntalo erguido." },
        new[] { "¿Ya viste el mapa de arriba a la derecha?", "Los puntos rojos son enemigos y la cruz blanca es el paciente.", "Úsalo para encontrar suministros sin perderte." },
        new[] { "Yo viví un sismo fuerte el año pasado.", "Lo que me salvó fue recordar tres pasos: agáchate, cúbrete y sujétate.", "Y cuando pare, aléjate de ventanas y muros dañados." },
        new[] { "En las misiones hay gente peligrosa.", "En la tienda venden un bate para defenderte.", "Lo pagas con las monedas que encuentres en los niveles." },
        new[] { "Hice un curso de primeros auxilios.", "Si una herida sangra, presiona con un paño limpio.", "Si el paño se empapa, no lo quites: pon otro encima y sigue presionando." },
        new[] { "¿Tienes una mochila de emergencia en casa?", "Lleva agua, linterna, botiquín, medicinas y copias de tus documentos.", "Revísala cada cierto tiempo para que nada caduque." },
        new[] { "Mi hermano es brigadista forestal.", "Dice que el humo sube, por eso hay que avanzar agachado.", "Y hay que cubrirse la nariz y la boca con una tela." },
        new[] { "Con este calor hay que cuidarse.", "Bebe agua aunque no tengas sed y busca la sombra.", "Si alguien se confunde y tiene la piel muy caliente, puede ser un golpe de calor." },
        new[] { "Hoy vine a revisar mis vacunas.", "La del tétanos necesita refuerzos cada cierto tiempo.", "Pregunta en tu centro de salud si la tienes al día." },
        new[] { "¿Sabías que lavarse bien las manos evita muchas enfermedades?", "Usa agua y jabón por al menos 20 segundos.", "Hazlo antes de comer y después de ir al baño." },
        new[] { "Si llamas a emergencias, mantén la calma.", "Di dónde estás y qué pasó.", "No cuelgues hasta que te lo indiquen." },
        new[] { "Me dijeron que en la base lunar la gravedad es baja.", "Se salta mucho más alto, pero cuesta calcular la caída.", "Y si ves una marca roja en el suelo, apártate: viene un meteorito." },
        new[] { "Estuve en la reserva antes del incendio.", "El río solo se puede cruzar por los dos puentes.", "Mira el mapa antes de meterte entre los árboles." },
        new[] { "Después de un desastre es normal sentir miedo o tristeza.", "Hablar con alguien de confianza ayuda mucho.", "Cuidar la mente también es cuidar la salud." }
    };

    /// <summary>Aula abierta con pizarrón, carteles, escritorio y sillas. Se abre hacia el sur.</summary>
    private static Transform ConstruirAula(Transform raiz)
    {
        Transform aula = Grupo(raiz, "AulaSalud");
        aula.position = PosAula;

        Vector3 Mundo(float x, float y, float z) => aula.TransformPoint(new Vector3(x, y, z));

        Material tarima = Mat("TarimaAula", new Color(0.55f, 0.42f, 0.28f), 0.25f);
        Material lona = Mat("LonaVerde", new Color(0.13f, 0.5f, 0.42f), 0.2f);
        Material madera = Mat("Madera", new Color(0.42f, 0.28f, 0.16f), 0.2f);
        Material pizarra = Mat("Pizarra", new Color(0.09f, 0.22f, 0.17f), 0.25f);

        Primitiva(PrimitiveType.Cube, aula, "Tarima", new Vector3(0f, 0.05f, 1.4f), new Vector3(11f, 0.1f, 7.6f), tarima);

        GameObject carpa = Carpa(aula, new Vector3(0f, 0f, 2.6f), 0f, 10.5f, 5.4f, lona, false);
        carpa.transform.localPosition = new Vector3(0f, 0f, 2.6f);
        carpa.transform.localRotation = Quaternion.identity;

        // Pizarrón al fondo, con carteles a los lados.
        Primitiva(PrimitiveType.Cube, aula, "MarcoPizarra", new Vector3(0f, 1.6f, 5.05f), new Vector3(4.4f, 2.3f, 0.08f), madera, null, true);
        Primitiva(PrimitiveType.Cube, aula, "Pizarra", new Vector3(0f, 1.6f, 5f), new Vector3(4.1f, 2f, 0.04f), pizarra);
        TextoAula(aula, "¿Qué aprendiste hoy?\n\n+  Reconocer\n+  Tratar\n+  Prevenir", new Vector3(0f, 1.6f, 4.96f), 2.5f, Color.white, new Vector2(3.8f, 1.8f));

        Cartel(aula, new Vector3(-3.6f, 1.6f, 5f), new Color(0.2f, 0.5f, 0.75f), "Lávate\nlas manos");
        Cartel(aula, new Vector3(3.6f, 1.6f, 5f), new Color(0.8f, 0.3f, 0.2f), "Agáchate,\ncúbrete y\nsujétate");

        // Escritorio de la instructora.
        Primitiva(PrimitiveType.Cube, aula, "Escritorio", new Vector3(2.4f, 0.48f, 3.2f), new Vector3(2.2f, 0.76f, 0.9f), madera, null, true);
        Poner(OBJETOS + "MedBook.prefab", aula, Mundo(2f, 0.87f, 3.1f), 20f, 1f, false);
        Poner(OBJETOS + "PillBottle.prefab", aula, Mundo(2.6f, 0.87f, 3.3f), 0f, 1f, false);
        Poner(OBJETOS + "Bandages.prefab", aula, Mundo(3f, 0.87f, 3.05f), 40f, 1f, false);
        Poner(OBJETOS + "HealthKit.prefab", aula, Mundo(-4.2f, 0.1f, 3.6f), 200f, 1.2f, false);

        // Sillas en dos grupos, con pasillo central para llegar a la instructora.
        foreach (float x in new[] { -4f, -2.7f, 2.7f, 4f })
        {
            foreach (float z in new[] { -0.6f, 1f })
            {
                Poner(OBJETOS + "Chair.prefab", aula, Mundo(x, 0.1f, z), 0f, EscalaPara(OBJETOS + "Chair.prefab", 0.95f));
            }
        }

        // Plantas y banderines en la entrada.
        foreach (float x in new[] { -5.6f, 5.6f })
        {
            Primitiva(PrimitiveType.Cylinder, aula, "Maceta", new Vector3(x, 0.3f, -2.1f), new Vector3(0.8f, 0.3f, 0.8f), Mat("Barro", new Color(0.6f, 0.3f, 0.18f), 0.15f), null, true);
            Vegetal("plant_bushDetailed", aula, Mundo(x, 0.6f, -2.1f), x * 10f, 1.1f);
            Primitiva(PrimitiveType.Cylinder, aula, "Mastil", new Vector3(x, 2f, -3f), new Vector3(0.08f, 2f, 0.08f), Mat("PosteMetal", new Color(0.32f, 0.33f, 0.35f), 0.4f, 0.6f), null, true);
            Primitiva(PrimitiveType.Cube, aula, "Banderin", new Vector3(x + (x < 0f ? 0.55f : -0.55f), 3.5f, -3f), new Vector3(1f, 0.7f, 0.03f), lona);
        }

        return aula;
    }

    private static void Cartel(Transform aula, Vector3 pos, Color color, string texto)
    {
        Primitiva(PrimitiveType.Cube, aula, "Cartel", pos, new Vector3(2.2f, 1.9f, 0.06f), Mat("Cartel" + (int)(color.r * 100f), color, 0.25f));
        TextoAula(aula, texto, pos + Vector3.back * 0.05f, 3f, Color.white, new Vector2(2f, 1.7f));
    }

    private static void TextoAula(Transform aula, string texto, Vector3 pos, float tamano, Color color, Vector2 caja)
    {
        GameObject go = new GameObject("TextoAula");
        go.transform.SetParent(aula, false);
        go.transform.localPosition = pos;

        TextMeshPro tmp = CrearTextoMundo(go, texto, tamano, color);
        tmp.rectTransform.sizeDelta = caja;
        tmp.textWrappingMode = TextWrappingModes.Normal;
    }

    private static void ConstruirAlumnos(Transform aula)
    {
        string[] frases =
        {
            "La instructora me preguntó sobre lo que aprendí en la misión.",
            "Lee bien la ficha médica de cada paciente: de ahí salen las preguntas.",
            "Si fallas, te explica la respuesta y puedes volver a intentarlo."
        };

        Vector3 Mundo(float x, float z) => aula.TransformPoint(new Vector3(x, 0.1f, z));

        CrearResidente(aula, "Alumna", "Camila", PielVecino(5), Mundo(-5f, 2.4f), 60f, null, false).frases = frases;
        CrearResidente(aula, "Alumno", "Andrés", PielVecino(3), Mundo(5f, 2.2f), -60f, null, false).frases = frases;
    }
}
