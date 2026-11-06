using System.Collections.Generic;
using System.IO;
using TMPro;
using Unity.AI.Navigation;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Nivel 1: avenida destruida por un terremoto. Se recorre de sur a norte.
/// Entrada con muro de concreto, tramo de incendios, plaza con saqueadores
/// y callejón final hasta el puesto de triage donde espera el paciente.
/// </summary>
public static partial class MontajeRescate
{
    private static readonly Vector3 OrigenNivel = new Vector3(500f, 0f, 500f);
    private const int JERINGAS_META = 3;
    private const int EQUIPAJES_META = 2;

    private static Vector3 N(float x, float z, float y = 0f)
    {
        return OrigenNivel + new Vector3(x, y, z);
    }

    private static void MontarNivel1()
    {
        Scene escena = EditorSceneManager.OpenScene("Assets/Scenes/Nivel1.unity", OpenSceneMode.Single);

        foreach (string nombre in new[] { "EntornoUrbanoSalud", "GrupoRecolectablesSalud", "Obstaculo", "Obstaculo (1)", "Obstaculo (2)", "NivelDesastre" })
        {
            BorrarRaiz(nombre);
        }

        GameObject jugadorGO = GameObject.Find("Jugador");
        PlayerController jugador = PrepararJugador(jugadorGO);
        jugador.jeringasMeta = JERINGAS_META;
        jugador.equipajesMeta = EQUIPAJES_META;
        jugadorGO.transform.position = N(0f, -54f, 1.05f);
        jugadorGO.transform.rotation = Quaternion.identity;

        Camera camara = jugador.camaraJugador;
        camara.transform.position = N(0f, -59f, 3f);
        camara.transform.rotation = Quaternion.Euler(12f, 0f, 0f);

        AmbienteDesastre(camara, BuscarSol(), new Color(0.5f, 0.46f, 0.43f), 0.012f, 1.25f);

        Transform nivel = new GameObject("NivelDesastre").transform;
        Transform escenario = Grupo(nivel, "Escenario");
        Transform obstaculos = Grupo(nivel, "Obstaculos");
        Transform peligros = Grupo(nivel, "Peligros");
        Transform suministros = Grupo(nivel, "Suministros");
        Transform monedas = Grupo(nivel, "Monedas");
        Transform enemigos = Grupo(nivel, "Enemigos");

        ConstruirEscenario(escenario);
        ConstruirObstaculos(obstaculos);
        ConstruirPeligros(peligros);
        ConstruirSuministros(suministros);
        ConstruirMonedas(monedas);

        Paciente paciente = PrepararPaciente(nivel);

        Canvas canvas = PrepararCanvas();
        ConstruirHud(canvas.transform, jugador);
        PanelPausa(canvas.transform, jugador, "VOLVER AL HUB", jugador.VolverAlHub);
        ConstruirVictoria(canvas.transform, paciente);

        // Los enemigos se colocan después de hornear para que no se tapen su propio camino.
        HornearNavMesh(nivel, jugadorGO, paciente);
        Texture2D mapa = HornearMapa(nivel, jugadorGO);
        ConstruirEnemigos(enemigos);
        ConstruirMapa(canvas.transform, jugador, mapa);

        EditorUtility.SetDirty(jugador);
        EditorUtility.SetDirty(paciente);
        EditorSceneManager.MarkSceneDirty(escena);
        EditorSceneManager.SaveScene(escena);
    }

    private static Transform Grupo(Transform padre, string nombre)
    {
        Transform grupo = new GameObject(nombre).transform;
        grupo.SetParent(padre, false);
        return grupo;
    }

    private static void ConstruirEscenario(Transform raiz)
    {
        GameObject plano = Primitiva(PrimitiveType.Plane, raiz, "SueloZona", Vector3.zero, new Vector3(36f, 1f, 36f), MatSuelo(), null, true);
        plano.transform.position = N(0f, 0f, 0.02f);

        // Edificios a ambos lados: cierran la avenida y dejan un ensanche para la plaza.
        float[] filas = { -50f, -31f, -12f };
        foreach (float z in filas)
        {
            Poner(CIUDAD + "mid_house_1.prefab", raiz, N(-19f, z), 90f);
            Poner(CIUDAD + "mid_house_3.prefab", raiz, N(19f, z), -90f);
        }

        Poner(CIUDAD + "mid_house_3.prefab", raiz, N(-19f, 42f), 90f);
        Poner(CIUDAD + "mid_house_1.prefab", raiz, N(19f, 42f), -90f);

        Poner(CIUDAD + "office_building_2.prefab", raiz, N(-46f, 16f), 90f);
        Poner(CIUDAD + "office_building_3.prefab", raiz, N(46f, 16f), -90f);
        Poner(CIUDAD + "office_building_1.prefab", raiz, N(0f, 86f), 180f);
        Poner(CIUDAD + "factory_building_small.prefab", raiz, N(0f, -88f), 0f);

        Poner(CIUDAD + "tree_2.prefab", raiz, N(-24f, 8f), 40f, 0.45f, false);
        Poner(CIUDAD + "tree_3.prefab", raiz, N(24f, 26f), 120f, 0.45f, false);

        // Límites invisibles para no salir de la zona.
        Limite(raiz, "LimiteOeste", N(-30.5f, 0f, 5f), new Vector3(1f, 12f, 130f));
        Limite(raiz, "LimiteEste", N(30.5f, 0f, 5f), new Vector3(1f, 12f, 130f));
        Limite(raiz, "LimiteSur", N(0f, -62f, 5f), new Vector3(62f, 12f, 1f));
        Limite(raiz, "LimiteNorte", N(0f, 62f, 5f), new Vector3(62f, 12f, 1f));
        Limite(raiz, "LimiteCalleOeste", N(-12f, -32f, 5f), new Vector3(1f, 12f, 62f));
        Limite(raiz, "LimiteCalleEste", N(12f, -32f, 5f), new Vector3(1f, 12f, 62f));
        Limite(raiz, "LimiteCallejonOeste", N(-12f, 46f, 5f), new Vector3(1f, 12f, 32f));
        Limite(raiz, "LimiteCallejonEste", N(12f, 46f, 5f), new Vector3(1f, 12f, 32f));

        // Puesto de avanzada donde empieza el jugador.
        Ambulancia(raiz, N(-6.5f, -54f), 95f);
        Carpa(raiz, N(6.8f, -54f), 180f, 4f, 3.4f, Mat("LonaBlanca", new Color(0.86f, 0.86f, 0.83f), 0.15f), true);

        // Humo en el horizonte.
        foreach (Vector3 pos in new[] { N(-26f, -24f), N(27f, 2f), N(-22f, 36f), N(24f, 50f), N(-40f, 18f), N(4f, 72f) })
        {
            Humo(raiz, pos);
        }
    }

    private static void Limite(Transform padre, string nombre, Vector3 pos, Vector3 tam)
    {
        GameObject limite = new GameObject(nombre);
        limite.transform.SetParent(padre, false);
        limite.transform.position = pos;
        limite.AddComponent<BoxCollider>().size = tam;
    }

    private static void ConstruirObstaculos(Transform raiz)
    {
        // Muro de bloques: se salta, o se rodea por el hueco de la derecha.
        for (float x = -10f; x <= 3f; x += 3.1f)
        {
            Poner(CIUDAD + "concrete_block.prefab", raiz, N(x, -44f), 0f);
        }
        Caja(raiz, N(10f, -44f), 12f);
        Roca(raiz, N(-9f, -38f, 0.15f), 2.4f, 30f, 1);
        Roca(raiz, N(-5.5f, -36f, 0.1f), 1.5f, 110f, 2);
        Roca(raiz, N(7.5f, -35f, 0.2f), 2.8f, 200f, 3);
        Caja(raiz, N(3f, -36f), 25f);
        Caja(raiz, N(4.2f, -35f), -15f);
        Caja(raiz, N(3.6f, -35.5f, 0.77f), 40f);

        // Segundo tramo: muro derrumbado que deja dos pasos estrechos.
        Losa(raiz, N(-6.5f, -28f, 1.3f), new Vector3(9f, 3f, 0.6f), new Vector3(0f, 6f, 3f));
        Losa(raiz, N(6.8f, -28f, 1.1f), new Vector3(7.5f, 2.4f, 0.6f), new Vector3(0f, -5f, -4f));
        Roca(raiz, N(1f, -29.5f, 0.05f), 1.2f, 15f, 3);

        // Rampa de techo caído: sube a una plataforma con monedas.
        Losa(raiz, N(-7.2f, -9.2f, 0.93f), new Vector3(4f, 0.35f, 9f), new Vector3(-15f, 0f, 0f));
        Losa(raiz, N(-7.2f, -2.4f, 2.1f), new Vector3(5f, 0.35f, 5.6f), Vector3.zero);
        Poner(CIUDAD + "concrete_block.prefab", raiz, N(-9f, -0.4f, 0.4f), 90f, 1.7f);
        Poner(CIUDAD + "concrete_block.prefab", raiz, N(-5.3f, -0.4f, 0.4f), 90f, 1.7f);

        // Poste caído que corta la calle a la altura de un salto.
        GameObject poste = Primitiva(PrimitiveType.Cylinder, raiz, "PosteCaido", Vector3.zero, new Vector3(0.35f, 5.5f, 0.35f), Mat("PosteMetal", new Color(0.32f, 0.33f, 0.35f), 0.4f, 0.6f), new Vector3(4f, 0f, 86f), true);
        poste.transform.position = N(5f, -8f, 0.55f);
        Roca(raiz, N(10f, -6f, 0.1f), 2.2f, 60f, 1);

        // Coberturas de la plaza, útiles para esconderse de los saqueadores.
        Losa(raiz, N(-9f, 12f, 1.25f), new Vector3(7f, 2.5f, 0.7f), new Vector3(0f, 18f, 3f));
        Losa(raiz, N(9.5f, 14f, 1.2f), new Vector3(6f, 2.4f, 0.7f), new Vector3(0f, -24f, -3f));
        Losa(raiz, N(-11f, 25f, 1.2f), new Vector3(0.7f, 2.4f, 6.5f), new Vector3(3f, 12f, 0f));
        Losa(raiz, N(11f, 25f, 1.2f), new Vector3(0.7f, 2.4f, 6.5f), new Vector3(-3f, -16f, 0f));
        Roca(raiz, N(0f, 17.5f, 0.2f), 3.4f, 15f, 1);
        Roca(raiz, N(-3.2f, 19f, 0.05f), 1.8f, 80f, 2);
        Roca(raiz, N(3.4f, 15.7f, 0.1f), 1.6f, 150f, 3);
        Caja(raiz, N(-20f, 10f), 18f);
        Caja(raiz, N(-20f, 11.2f), -8f);
        Caja(raiz, N(21f, 15f), 50f);
        Caja(raiz, N(21.2f, 15f, 0.77f), 10f);
        Poner(CIUDAD + "bench.prefab", raiz, N(-16f, 28f), 70f);
        Poner(CIUDAD + "bench.prefab", raiz, N(16f, 6f), 200f);

        // Callejón final.
        Poner(CIUDAD + "concrete_block.prefab", raiz, N(-7f, 34f), 90f);
        Poner(CIUDAD + "concrete_block.prefab", raiz, N(7f, 34f), 90f);
        Roca(raiz, N(-3f, 40f, 0.2f), 2.8f, 35f, 2);
        Roca(raiz, N(5.5f, 44f, 0.2f), 2.6f, 120f, 1);
        Caja(raiz, N(-8.5f, 46f), 8f);
        Caja(raiz, N(-8.3f, 47.2f), 30f);
    }

    private static void ConstruirPeligros(Transform raiz)
    {
        // Línea de incendios en la avenida con dos pasos seguros.
        foreach (float x in new[] { -10f, -8f, 1f, 3f, 9.8f })
        {
            Fuego(raiz, N(x, -18f));
        }
        Fuego(raiz, N(-2f, 8f));
        Fuego(raiz, N(0f, 30f));
        Humo(raiz, N(-2f, 8f, 0.5f));
    }

    private static void ConstruirSuministros(Transform raiz)
    {
        const string JERINGA = "Assets/Prefabs/Recolectables/JeringaRecolectable.prefab";
        const string EQUIPAJE = "Assets/Prefabs/Recolectables/EquipajeSaludRecolectable.prefab";
        const string BOTIQUIN = "Assets/Prefabs/Recolectables/BotiquinRecolectable.prefab";

        Vector3[] jeringas = { N(9.5f, -40f, 0.9f), N(-7.2f, -2.4f, 3.1f), N(-22f, 16f, 0.9f), N(23f, 22f, 0.9f), N(8f, 49f, 0.9f) };
        Vector3[] equipajes = { N(-9.5f, -24f, 0.9f), N(0f, 24f, 0.9f), N(-9f, 40f, 0.9f) };
        Vector3[] botiquines = { N(9f, -12f, 0.9f), N(24f, 9f, 0.9f) };

        for (int i = 0; i < jeringas.Length; i++) Poner(JERINGA, raiz, jeringas[i], 0f, 1f, false).name = "Jeringa_" + (i + 1);
        for (int i = 0; i < equipajes.Length; i++) Poner(EQUIPAJE, raiz, equipajes[i], 0f, 1f, false).name = "Equipaje_" + (i + 1);
        for (int i = 0; i < botiquines.Length; i++) Poner(BOTIQUIN, raiz, botiquines[i], 0f, 1f, false).name = "Botiquin_" + (i + 1);
    }

    private static void ConstruirMonedas(Transform raiz)
    {
        // Rastros que enseñan el camino y premian tomar los desvíos.
        Linea(raiz, N(0f, -50f), N(0f, -46f), 3);
        Arco(raiz, N(0f, -45.8f), N(0f, -42.2f), 3, 1.8f);
        Linea(raiz, N(0f, -40f), N(-2f, -31f), 4);
        Linea(raiz, N(1f, -26f), N(5.5f, -19.5f), 4);
        Linea(raiz, N(5.5f, -16f), N(1f, -11f), 3);
        Arco(raiz, N(5f, -9.5f), N(5f, -6.2f), 3, 1.9f);
        Linea(raiz, N(-7.2f, -11f, 1.1f), N(-7.2f, -6.2f, 2.3f), 3);
        Linea(raiz, N(-9f, -2.4f, 2.6f), N(-5f, -2.4f, 2.6f), 3);
        Linea(raiz, N(-18f, 7f), N(-23f, 14f), 4);
        Linea(raiz, N(18f, 19f), N(23f, 16f), 3);
        Linea(raiz, N(-5f, 22f), N(5f, 22f), 4);
        Arco(raiz, N(-7f, 32.4f), N(-7f, 35.6f), 3, 1.8f);
        Linea(raiz, N(2f, 38f), N(2f, 47f), 4);
    }

    private static void Linea(Transform raiz, Vector3 desde, Vector3 hasta, int cantidad)
    {
        for (int i = 0; i < cantidad; i++)
        {
            float t = cantidad == 1 ? 0.5f : i / (float)(cantidad - 1);
            Moneda(raiz, Vector3.Lerp(desde, hasta, t) + Vector3.up * 0.9f);
        }
    }

    private static void Arco(Transform raiz, Vector3 desde, Vector3 hasta, int cantidad, float altura)
    {
        for (int i = 0; i < cantidad; i++)
        {
            float t = i / (float)(cantidad - 1);
            Vector3 punto = Vector3.Lerp(desde, hasta, t) + Vector3.up * (0.9f + Mathf.Sin(t * Mathf.PI) * altura);
            Moneda(raiz, punto);
        }
    }

    private static void Moneda(Transform raiz, Vector3 pos)
    {
        Poner(RUTA_MONEDA, raiz, pos, 0f, 1f, false);
    }

    private static Paciente PrepararPaciente(Transform nivel)
    {
        Transform triage = Grupo(nivel, "PuestoTriage");
        Carpa(triage, N(0f, 54f), 0f, 5.6f, 4.6f, Mat("LonaBlanca", new Color(0.86f, 0.86f, 0.83f), 0.15f), true);
        Poner(OBJETOS + "HealthKit.prefab", triage, N(1.9f, 54.6f, 0.02f), 25f, 1f, false);
        Letrero(triage, "LetreroTriage", "PUESTO DE TRIAGE", N(0f, 51.3f, 4.4f), 3.4f, new Color(1f, 0.92f, 0.8f));

        GameObject objeto = Object.FindAnyObjectByType<Paciente>().gameObject;
        Paciente paciente = objeto.GetComponent<Paciente>();
        paciente.jeringasRequeridas = JERINGAS_META;
        paciente.equipajesRequeridos = EQUIPAJES_META;
        paciente.recompensaMonedas = 30;

        objeto.transform.position = N(0f, 54f);
        objeto.transform.rotation = Quaternion.identity;

        MeshRenderer capsula = objeto.GetComponent<MeshRenderer>();
        if (capsula != null) capsula.enabled = false;

        foreach (Collider anterior in objeto.GetComponents<Collider>()) Object.DestroyImmediate(anterior);

        BoxCollider zona = objeto.AddComponent<BoxCollider>();
        zona.isTrigger = true;
        zona.center = Vector3.up;
        zona.size = new Vector3(6f, 2.5f, 6f);

        for (int i = objeto.transform.childCount - 1; i >= 0; i--)
        {
            Object.DestroyImmediate(objeto.transform.GetChild(i).gameObject);
        }

        GameObject camilla = Camilla(objeto.transform, N(0f, 54f), 0f);
        BoxCollider solido = camilla.AddComponent<BoxCollider>();
        solido.center = Vector3.up * 0.45f;
        solido.size = new Vector3(0.9f, 0.9f, 2.2f);

        GameObject luzGO = new GameObject("LuzTriage");
        luzGO.transform.SetParent(objeto.transform, false);
        luzGO.transform.localPosition = Vector3.up * 2.3f;
        Light luz = luzGO.AddComponent<Light>();
        luz.type = LightType.Point;
        luz.range = 8f;
        paciente.luzEstado = luz;

        GameObject estado = new GameObject("EstadoPaciente");
        estado.transform.SetParent(objeto.transform, false);
        estado.transform.localPosition = Vector3.up * 2.05f;
        paciente.textoEstado = CrearTextoMundo(estado, "", 3f, Color.white);
        estado.AddComponent<Billboard>();

        return paciente;
    }

    private static void ConstruirHud(Transform canvas, PlayerController jugador)
    {
        GameObject tarjeta = PanelUI(canvas, "HUD_Estado", new Vector2(0f, 1f), new Vector2(30f, -28f), new Vector2(430f, 236f), new Color(0.06f, 0.08f, 0.11f, 0.86f));
        tarjeta.GetComponent<Image>().raycastTarget = false;

        TextoUI(tarjeta.transform, "EtiquetaSalud", "SALUD", 19f, new Vector2(0f, 1f), new Vector2(24f, -14f), new Vector2(200f, 26f), TextAlignmentOptions.Left, ColTextoSuave, true).characterSpacing = 3f;
        jugador.barraDeVida = BarraSalud(tarjeta.transform);

        TextoUI(tarjeta.transform, "EtiquetaMision", "SUMINISTROS PARA EL PACIENTE", 19f, new Vector2(0f, 1f), new Vector2(24f, -84f), new Vector2(390f, 26f), TextAlignmentOptions.Left, ColNaranja, true).characterSpacing = 2f;
        jugador.textoJeringas = TextoUI(tarjeta.transform, "TextoJeringas", "Jeringas  0 / " + JERINGAS_META, 25f, new Vector2(0f, 1f), new Vector2(24f, -118f), new Vector2(380f, 34f), TextAlignmentOptions.Left, new Color(0.45f, 0.88f, 1f), true);
        jugador.textoEquipajes = TextoUI(tarjeta.transform, "TextoEquipajes", "Equipajes  0 / " + EQUIPAJES_META, 25f, new Vector2(0f, 1f), new Vector2(24f, -154f), new Vector2(380f, 34f), TextAlignmentOptions.Left, new Color(1f, 0.86f, 0.4f), true);
        TextoUI(tarjeta.transform, "Consejo", "Lleva todo al puesto de triage, al norte.", 18f, new Vector2(0f, 1f), new Vector2(24f, -192f), new Vector2(390f, 28f), TextAlignmentOptions.Left, ColTextoSuave);

        jugador.textoSuministros = null;
        jugador.textoObjetivo = null;

        ChipMonedas(canvas, new Vector2(1f, 1f), new Vector2(-258f, -30f));
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
        imagen.pixelsPerUnitMultiplier = 0.45f;
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

    private static void ConstruirVictoria(Transform canvas, Paciente paciente)
    {
        GameObject velo = Velo(canvas, "PanelVictoria", new Color(0.02f, 0.03f, 0.05f, 0.75f));
        GameObject tarjeta = PanelUI(velo.transform, "Tarjeta", Centro, Vector2.zero, new Vector2(860f, 540f), ColFondo);

        TextoUI(tarjeta.transform, "Antetitulo", "MISIÓN CUMPLIDA", 24f, new Vector2(0.5f, 1f), new Vector2(0f, -44f), new Vector2(760f, 34f), TextAlignmentOptions.Center, ColVerde, true).characterSpacing = 6f;
        TextoUI(tarjeta.transform, "Titulo", "Paciente rescatado", 54f, new Vector2(0.5f, 1f), new Vector2(0f, -84f), new Vector2(780f, 74f), TextAlignmentOptions.Center, Color.white, true);
        paciente.textoMensajeVictoria = TextoUI(tarjeta.transform, "Mensaje", "", 26f, Centro, new Vector2(0f, -10f), new Vector2(720f, 180f), TextAlignmentOptions.Center, ColTextoSuave);

        BotonUI(tarjeta.transform, "BtnVolverHubVictoria", "VOLVER AL HUB", 24f, new Vector2(0.5f, 0f), new Vector2(0f, 40f), new Vector2(380f, 72f), ColNaranja, paciente.VolverAlHub);

        paciente.panelVictoria = velo;
        velo.SetActive(false);
    }

    private const float MAPA_ALTO = 63f;
    private const int MAPA_PIXELES_X = 512;
    private const int MAPA_PIXELES_Y = 1040;

    private static float MapaAncho => MAPA_ALTO * MAPA_PIXELES_X / MAPA_PIXELES_Y;

    /// <summary>Saca una foto del nivel desde arriba y la guarda como imagen del mapa.</summary>
    private static Texture2D HornearMapa(Transform nivel, GameObject jugador)
    {
        const string RUTA = DIR + "/MapaNivel1.png";

        GameObject camaraGO = new GameObject("CamaraMapa");
        camaraGO.transform.position = OrigenNivel + Vector3.up * 80f;
        camaraGO.transform.rotation = Quaternion.Euler(90f, 0f, 0f);

        Camera camara = camaraGO.AddComponent<Camera>();
        camara.orthographic = true;
        camara.orthographicSize = MAPA_ALTO;
        camara.nearClipPlane = 1f;
        camara.farClipPlane = 120f;
        camara.clearFlags = CameraClearFlags.SolidColor;
        camara.backgroundColor = new Color(0.09f, 0.09f, 0.1f);

        // Se ocultan lo que se mueve y lo que tendrá su propia marca.
        bool niebla = RenderSettings.fog;
        RenderSettings.fog = false;
        jugador.SetActive(false);
        Transform monedas = nivel.Find("Monedas");
        Transform suministros = nivel.Find("Suministros");
        monedas.gameObject.SetActive(false);
        suministros.gameObject.SetActive(false);
        List<ParticleSystemRenderer> particulas = new List<ParticleSystemRenderer>(nivel.GetComponentsInChildren<ParticleSystemRenderer>());
        particulas.ForEach(p => p.enabled = false);

        RenderTexture destino = new RenderTexture(MAPA_PIXELES_X, MAPA_PIXELES_Y, 24, RenderTextureFormat.ARGB32);
        camara.targetTexture = destino;
        camara.Render();

        RenderTexture.active = destino;
        Texture2D imagen = new Texture2D(MAPA_PIXELES_X, MAPA_PIXELES_Y, TextureFormat.RGB24, false);
        imagen.ReadPixels(new Rect(0, 0, MAPA_PIXELES_X, MAPA_PIXELES_Y), 0, 0);
        imagen.Apply();
        File.WriteAllBytes(RUTA, imagen.EncodeToPNG());

        RenderTexture.active = null;
        camara.targetTexture = null;
        Object.DestroyImmediate(destino);
        Object.DestroyImmediate(imagen);
        Object.DestroyImmediate(camaraGO);

        particulas.ForEach(p => p.enabled = true);
        suministros.gameObject.SetActive(true);
        monedas.gameObject.SetActive(true);
        jugador.SetActive(true);
        RenderSettings.fog = niebla;

        AssetDatabase.ImportAsset(RUTA, ImportAssetOptions.ForceUpdate);
        return Cargar<Texture2D>(RUTA);
    }

    private static void ConstruirMapa(Transform canvas, PlayerController jugador, Texture2D mapa)
    {
        const float ANCHO = 196f;
        float alto = ANCHO * MAPA_PIXELES_Y / MAPA_PIXELES_X;

        GameObject panel = PanelUI(canvas, "Mapa", new Vector2(1f, 1f), new Vector2(-30f, -28f), new Vector2(ANCHO + 16f, alto + 74f), new Color(0.06f, 0.08f, 0.11f, 0.9f));
        panel.GetComponent<Image>().raycastTarget = false;

        RectTransform area = NuevoRect(panel.transform, "Area", new Vector2(0.5f, 1f), new Vector2(0f, -8f), new Vector2(ANCHO, alto));
        RawImage fondo = area.gameObject.AddComponent<RawImage>();
        fondo.texture = mapa;
        fondo.color = new Color(0.72f, 0.72f, 0.74f);
        fondo.raycastTarget = false;

        MinimapaUI minimapa = panel.AddComponent<MinimapaUI>();
        minimapa.area = area;
        minimapa.jugador = jugador;
        minimapa.mundoMin = new Vector2(OrigenNivel.x - MapaAncho, OrigenNivel.z - MAPA_ALTO);
        minimapa.mundoMax = new Vector2(OrigenNivel.x + MapaAncho, OrigenNivel.z + MAPA_ALTO);
        minimapa.spritePunto = SpriteCirculo;
        minimapa.spriteFlecha = Cargar<Sprite>(DIR + "/IconoFlecha.png");
        minimapa.spriteCruz = Cargar<Sprite>(DIR + "/IconoCruz.png");

        Leyenda(panel.transform, 0, "Enemigo", minimapa.colorEnemigo);
        Leyenda(panel.transform, 1, "Jeringa", minimapa.colorJeringa);
        Leyenda(panel.transform, 2, "Equipaje", minimapa.colorEquipaje);
        Leyenda(panel.transform, 3, "Botiquín", minimapa.colorBotiquin);
        Leyenda(panel.transform, 4, "Moneda", minimapa.colorMoneda);

        // Debajo de los paneles de pausa y victoria.
        panel.transform.SetSiblingIndex(1);
        EditorUtility.SetDirty(minimapa);
    }

    private static void Leyenda(Transform panel, int indice, string texto, Color color)
    {
        float x = 12f + (indice % 2) * 98f;
        float y = 12f + (2 - indice / 2) * 19f;

        RectTransform punto = NuevoRect(panel, "Punto" + texto, Vector2.zero, new Vector2(x, y + 3f), new Vector2(10f, 10f));
        Image imagen = punto.gameObject.AddComponent<Image>();
        imagen.sprite = SpriteCirculo;
        imagen.color = color;
        imagen.raycastTarget = false;

        TextoUI(panel, "Texto" + texto, texto, 14f, Vector2.zero, new Vector2(x + 14f, y - 1f), new Vector2(82f, 18f), TextAlignmentOptions.MidlineLeft, ColTextoSuave);
    }

    private static void HornearNavMesh(Transform nivel, GameObject jugador, Paciente paciente)
    {
        NavMeshSurface superficie = nivel.gameObject.AddComponent<NavMeshSurface>();
        superficie.collectObjects = CollectObjects.Volume;
        superficie.center = new Vector3(OrigenNivel.x, 2f, OrigenNivel.z);
        superficie.size = new Vector3(64f, 8f, 128f);
        superficie.useGeometry = NavMeshCollectGeometry.PhysicsColliders;

        jugador.SetActive(false);
        superficie.BuildNavMesh();
        jugador.SetActive(true);

        string ruta = DIR + "/NavMeshNivel1.asset";
        AssetDatabase.DeleteAsset(ruta);
        AssetDatabase.CreateAsset(superficie.navMeshData, ruta);
        EditorUtility.SetDirty(superficie);
    }

    private static void ConstruirEnemigos(Transform raiz)
    {
        CrearEnemigo(raiz, "Saqueador_Plaza1", N(-14f, 8f), new[] { N(-14f, 8f), N(14f, 8f), N(15f, 21f), N(-6f, 22f) });
        CrearEnemigo(raiz, "Saqueador_Plaza2", N(16f, 28f), new[] { N(16f, 28f), N(-16f, 30f), N(-22f, 20f), N(-22f, 6f) });
        CrearEnemigo(raiz, "Saqueador_Callejon", N(-7f, 38f), new[] { N(-7f, 38f), N(8f, 38f), N(9f, 48f), N(-5f, 47f) });
    }

    private static void CrearEnemigo(Transform raiz, string nombre, Vector3 inicio, Vector3[] puntos)
    {
        GameObject enemigo = (GameObject)PrefabUtility.InstantiatePrefab(Cargar<GameObject>(RUTA_ENEMIGO), raiz);
        enemigo.name = nombre;
        enemigo.transform.position = inicio;

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
