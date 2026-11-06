using TMPro;
using UnityEditor;
using UnityEditor.Events;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Contenido de salud del juego: enfermedades, fichas médicas y evaluación del NPC.
/// Para cambiar textos o preguntas, edítalos aquí y vuelve a montar el juego.
/// </summary>
public static partial class MontajeRescate
{
    private const string DIR_SALUD = DIR + "/Salud";

    private static void CrearBancoSalud()
    {
        System.IO.Directory.CreateDirectory(DIR_SALUD);
        AssetDatabase.Refresh();

        BancoSalud banco = ObtenerAsset<BancoSalud>(DIR_SALUD + "/BancoSalud.asset");
        banco.enfermedades.Clear();

        banco.enfermedades.Add(CrearEnfermedad("asma", "Crisis de asma por polvo",
            "Tos, silbidos al respirar, falta de aire y opresión en el pecho.",
            "Aleja a la persona del polvo.\nSiéntala erguida y ayúdala a calmarse.\nAyúdala a usar su inhalador de rescate.\nPide ayuda urgente si no mejora o tiene los labios azulados.",
            P("¿En qué postura debe estar alguien con una crisis de asma?", 0, "Sentado y erguido respira mejor.", "Sentado y erguido", "Acostado boca arriba", "Caminando rápido"),
            P("¿Qué medicamento ayuda rápido en una crisis de asma?", 1, "El inhalador de rescate abre las vías respiratorias.", "Un antibiótico", "El inhalador de rescate", "Una pastilla para dormir"),
            P("¿Qué señal indica que se necesita ayuda urgente?", 2, "Los labios azulados indican que falta oxígeno.", "Un poco de tos", "Sentir sed", "Labios azulados")));

        banco.enfermedades.Add(CrearEnfermedad("tetano", "Riesgo de tétanos",
            "Herida sucia con tierra u óxido. Más tarde puede dar rigidez en la mandíbula y espasmos.",
            "Lava la herida con agua limpia y jabón.\nCúbrela con un apósito limpio.\nRevisa si la vacuna antitetánica está al día.\nAcude a un centro de salud para valorar la herida.",
            P("¿Qué es lo primero que se hace con una herida sucia?", 0, "Lavarla con agua limpia y jabón retira la suciedad.", "Lavarla con agua limpia y jabón", "Taparla sin limpiarla", "Echarle tierra para secarla"),
            P("¿Qué previene el tétanos?", 1, "La vacuna antitetánica protege, y necesita refuerzos.", "Tomar agua", "La vacuna antitetánica", "Descansar mucho"),
            P("¿Cuál es un síntoma típico del tétanos?", 0, "El tétanos causa rigidez, muchas veces primero en la mandíbula.", "Rigidez en la mandíbula", "Estornudos", "Picazón en los ojos")));

        banco.enfermedades.Add(CrearEnfermedad("humo", "Intoxicación por humo",
            "Tos, dolor de cabeza, mareo, confusión y hollín en la nariz o la boca.",
            "Lleva a la persona a un lugar con aire fresco.\nNo la dejes sola y vigila su respiración.\nEn el hospital se le da oxígeno.\nDebe ser revisada aunque parezca mejorar.",
            P("¿Qué se hace primero con alguien intoxicado por humo?", 2, "Salir del humo hacia el aire fresco es lo primero.", "Darle de comer", "Dejarlo dormir ahí", "Llevarlo al aire fresco"),
            P("¿Qué gas peligroso del humo no se ve ni se huele?", 0, "El monóxido de carbono no tiene color ni olor.", "Monóxido de carbono", "Oxígeno", "Vapor de agua"),
            P("¿Con qué se trata en el hospital?", 1, "El oxígeno ayuda a eliminar el gas y a recuperar la respiración.", "Con vendajes", "Con oxígeno", "Con hielo en la cabeza")));

        banco.enfermedades.Add(CrearEnfermedad("calor", "Golpe de calor",
            "Temperatura corporal muy alta, piel caliente, confusión y hasta desmayo.",
            "Llama a emergencias: es grave.\nLleva a la persona a la sombra.\nEnfríala con agua y paños en cuello, axilas e ingles.\nDale sorbos de agua solo si está consciente.",
            P("¿Dónde conviene poner paños fríos en un golpe de calor?", 1, "En esas zonas pasan vasos grandes y el cuerpo se enfría más rápido.", "Solo en las manos", "Cuello, axilas e ingles", "En la espalda baja"),
            P("¿Cuándo se le puede dar agua a la persona?", 0, "Si no está consciente podría ahogarse.", "Solo si está consciente", "Siempre, aunque esté desmayada", "Nunca"),
            P("¿Qué se debe hacer además de enfriarla?", 2, "Es una emergencia médica y necesita atención profesional.", "Abrigarla", "Hacerla correr", "Llamar a emergencias")));

        banco.enfermedades.Add(CrearEnfermedad("hipoxia", "Hipoxia",
            "Falta de oxígeno: confusión, mareo, respiración rápida y labios azulados.",
            "Lleva a la persona a una zona con aire seguro.\nDale oxígeno si hay disponible.\nVigila su respiración sin dejarla sola.\nEl pulsioxímetro mide el oxígeno en la sangre.",
            P("¿Qué significa hipoxia?", 0, "Hipoxia es que al cuerpo le llega poco oxígeno.", "Falta de oxígeno", "Exceso de azúcar", "Fiebre alta"),
            P("¿Qué aparato mide el oxígeno en la sangre?", 2, "El pulsioxímetro se coloca normalmente en un dedo.", "Un termómetro", "Una báscula", "Un pulsioxímetro"),
            P("¿Cuál es el tratamiento principal?", 1, "Se le da oxígeno y se lleva a una zona con aire seguro.", "Darle agua con azúcar", "Darle oxígeno", "Hacerle correr")));

        banco.enfermedades.Add(CrearEnfermedad("descompresion", "Enfermedad por descompresión",
            "Burbujas de gas en el cuerpo por un cambio brusco de presión. Dolor en articulaciones, mareo y hormigueo.",
            "Mantén a la persona en reposo.\nDale oxígeno al 100 % si hay disponible.\nTrasládala a una cámara hiperbárica.\nAfecta a buzos y astronautas.",
            P("¿Qué causa la enfermedad por descompresión?", 1, "Al bajar rápido la presión se forman burbujas de gas en el cuerpo.", "Comer demasiado", "Un cambio brusco de presión", "El frío"),
            P("¿Dónde se trata esta enfermedad?", 0, "La cámara hiperbárica sube la presión para reducir las burbujas.", "En una cámara hiperbárica", "En una sauna", "En una piscina"),
            P("¿Qué síntoma es común?", 2, "Es típico el dolor en las articulaciones, además de mareo y hormigueo.", "Hambre", "Hipo", "Dolor en las articulaciones")));

        EditorUtility.SetDirty(banco);
    }

    private static PreguntaSalud P(string texto, int correcta, string explicacion, params string[] opciones)
    {
        return new PreguntaSalud { texto = texto, correcta = correcta, explicacion = explicacion, opciones = opciones };
    }

    private static Enfermedad CrearEnfermedad(string id, string nombre, string sintomas, string tratamiento, params PreguntaSalud[] preguntas)
    {
        Enfermedad enfermedad = ObtenerAsset<Enfermedad>(DIR_SALUD + "/" + id + ".asset");
        enfermedad.id = id;
        enfermedad.nombre = nombre;
        enfermedad.sintomas = sintomas;
        enfermedad.tratamiento = tratamiento;
        enfermedad.preguntas = preguntas;
        EditorUtility.SetDirty(enfermedad);
        return enfermedad;
    }

    private static void ConstruirAvisoCentral(Transform canvas)
    {
        RectTransform raiz = NuevoRect(canvas, "AvisoCentral", new Vector2(0.5f, 0.72f), Vector2.zero, new Vector2(760f, 92f));
        CanvasGroup grupo = raiz.gameObject.AddComponent<CanvasGroup>();
        grupo.blocksRaycasts = false;
        grupo.interactable = false;

        Image fondo = raiz.gameObject.AddComponent<Image>();
        fondo.color = new Color(0.55f, 0.06f, 0.05f, 0.92f);
        fondo.sprite = SpriteRedondeado;
        fondo.type = Image.Type.Sliced;
        fondo.pixelsPerUnitMultiplier = 0.45f;
        fondo.raycastTarget = false;

        AvisoCentral aviso = raiz.gameObject.AddComponent<AvisoCentral>();
        aviso.grupo = grupo;
        aviso.texto = TextoUI(raiz, "Texto", "", 42f, Centro, Vector2.zero, new Vector2(730f, 80f), TextAlignmentOptions.Center, Color.white, true);
        aviso.texto.characterSpacing = 4f;
    }

    private static void ConstruirFichaMedica(Transform canvas, PlayerController jugador)
    {
        RectTransform raiz = NuevoRect(canvas, "FichaMedica", Centro, Vector2.zero, Vector2.zero);
        FichaMedicaUI ficha = raiz.gameObject.AddComponent<FichaMedicaUI>();
        ficha.jugador = jugador;

        GameObject velo = Velo(raiz, "Panel", new Color(0.02f, 0.03f, 0.05f, 0.8f));
        RectTransform rtVelo = (RectTransform)velo.transform;
        rtVelo.anchorMin = Centro;
        rtVelo.anchorMax = Centro;
        rtVelo.sizeDelta = new Vector2(4000f, 3000f);
        ficha.panel = velo;

        GameObject tarjeta = PanelUI(velo.transform, "Tarjeta", Centro, Vector2.zero, new Vector2(1100f, 760f), ColFondo);

        TextoUI(tarjeta.transform, "Antetitulo", "FICHA MÉDICA", 22f, new Vector2(0.5f, 1f), new Vector2(0f, -34f), new Vector2(1000f, 32f), TextAlignmentOptions.Center, ColVerde, true).characterSpacing = 6f;
        ficha.textoNombre = TextoUI(tarjeta.transform, "Nombre", "", 46f, new Vector2(0.5f, 1f), new Vector2(0f, -68f), new Vector2(1020f, 64f), TextAlignmentOptions.Center, Color.white, true);

        TextoUI(tarjeta.transform, "TituloSintomas", "CÓMO RECONOCERLA", 22f, new Vector2(0f, 1f), new Vector2(60f, -156f), new Vector2(980f, 30f), TextAlignmentOptions.Left, ColNaranja, true).characterSpacing = 2f;
        ficha.textoSintomas = TextoUI(tarjeta.transform, "Sintomas", "", 28f, new Vector2(0f, 1f), new Vector2(60f, -190f), new Vector2(980f, 90f), TextAlignmentOptions.TopLeft, ColTextoSuave);

        TextoUI(tarjeta.transform, "TituloTratamiento", "CÓMO TRATARLA", 22f, new Vector2(0f, 1f), new Vector2(60f, -292f), new Vector2(980f, 30f), TextAlignmentOptions.Left, ColNaranja, true).characterSpacing = 2f;
        ficha.textoTratamiento = TextoUI(tarjeta.transform, "Tratamiento", "", 28f, new Vector2(0f, 1f), new Vector2(60f, -326f), new Vector2(980f, 230f), TextAlignmentOptions.TopLeft, Color.white);

        TextoUI(tarjeta.transform, "Recuerda", "Memorízala: en el Hub te harán preguntas sobre esto.", 22f, new Vector2(0.5f, 0f), new Vector2(0f, 112f), new Vector2(980f, 32f), TextAlignmentOptions.Center, ColOro);
        BotonUI(tarjeta.transform, "BtnFichaEntendido", "ENTENDIDO", 24f, new Vector2(0.5f, 0f), new Vector2(0f, 30f), new Vector2(340f, 70f), ColNaranja, ficha.Cerrar);
    }

    private static GameObject ConstruirPanelPreguntas(Transform canvas, NPCMedicoInteractivo npc)
    {
        GameObject velo = Velo(canvas, "PanelPreguntas", new Color(0.02f, 0.03f, 0.05f, 0.8f));
        GameObject tarjeta = PanelUI(velo.transform, "Tarjeta", Centro, Vector2.zero, new Vector2(1240f, 820f), ColFondo);

        PreguntasUI preguntas = velo.AddComponent<PreguntasUI>();
        preguntas.banco = Cargar<BancoSalud>(DIR_SALUD + "/BancoSalud.asset");

        TextoUI(tarjeta.transform, "Antetitulo", "REPASO DE SALUD", 22f, new Vector2(0f, 1f), new Vector2(60f, -36f), new Vector2(600f, 32f), TextAlignmentOptions.Left, ColNaranja, true).characterSpacing = 5f;
        preguntas.textoProgreso = TextoUI(tarjeta.transform, "Progreso", "", 22f, new Vector2(1f, 1f), new Vector2(-60f, -36f), new Vector2(400f, 32f), TextAlignmentOptions.Right, ColTextoSuave, true);
        preguntas.textoPregunta = TextoUI(tarjeta.transform, "Pregunta", "", 38f, new Vector2(0f, 1f), new Vector2(60f, -86f), new Vector2(1120f, 110f), TextAlignmentOptions.TopLeft, Color.white, true);

        preguntas.opciones = new Button[3];
        preguntas.textosOpciones = new TextMeshProUGUI[3];
        for (int i = 0; i < 3; i++)
        {
            Button opcion = BotonUI(tarjeta.transform, "Opcion" + i, "", 26f, new Vector2(0.5f, 1f), new Vector2(0f, -212f - i * 88f), new Vector2(1120f, 76f), preguntas.colorNormal, null);
            preguntas.opciones[i] = opcion;
            preguntas.textosOpciones[i] = opcion.GetComponentInChildren<TextMeshProUGUI>();
            preguntas.textosOpciones[i].characterSpacing = 0f;
            preguntas.textosOpciones[i].fontStyle = FontStyles.Normal;
        }

        preguntas.textoExplicacion = TextoUI(tarjeta.transform, "Explicacion", "", 26f, new Vector2(0f, 0f), new Vector2(60f, 118f), new Vector2(1120f, 150f), TextAlignmentOptions.TopLeft, ColTextoSuave);

        preguntas.botonSiguiente = BotonUI(tarjeta.transform, "BtnSiguiente", "SIGUIENTE", 22f, new Vector2(1f, 0f), new Vector2(-60f, 34f), new Vector2(300f, 64f), ColNaranja, null);
        preguntas.textoSiguiente = preguntas.botonSiguiente.GetComponentInChildren<TextMeshProUGUI>();
        BotonUI(tarjeta.transform, "BtnCerrarPreguntas", "CERRAR", 22f, new Vector2(0f, 0f), new Vector2(60f, 34f), new Vector2(240f, 64f), ColGris, npc.CerrarMenu);

        velo.SetActive(false);
        return velo;
    }
}
