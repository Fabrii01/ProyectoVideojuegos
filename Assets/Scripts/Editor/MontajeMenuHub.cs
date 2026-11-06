using System.Collections.Generic;
using TMPro;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public static partial class MontajeRescate
{
    #region Menú principal

    private static void MontarMenu()
    {
        Scene escena = EditorSceneManager.OpenScene("Assets/Scenes/MenuPrincipal.unity", OpenSceneMode.Single);

        BorrarRaiz("DecoradoMenu");
        Camera camara = Object.FindAnyObjectByType<Camera>();
        Light sol = BuscarSol();
        AmbienteDesastre(camara, sol, new Color(0.17f, 0.15f, 0.18f), 0.02f, 0.5f);
        RenderSettings.ambientLight = new Color(0.3f, 0.3f, 0.36f);

        ConstruirDecoradoMenu();

        camara.transform.position = new Vector3(-3.4f, 1.35f, -5.4f);
        camara.transform.LookAt(new Vector3(-0.2f, 1.75f, 2f));
        camara.fieldOfView = 44f;
        if (camara.GetComponent<CamaraMenu>() == null) camara.gameObject.AddComponent<CamaraMenu>();

        Canvas canvas = PrepararCanvas();
        MenuPrincipalController control = Object.FindAnyObjectByType<MenuPrincipalController>();

        // Columna izquierda: la derecha queda libre para ver la escena de rescate.
        GameObject columna = PanelUI(canvas.transform, "PanelMenuPrincipal", new Vector2(0f, 0.5f), Vector2.zero, new Vector2(700f, 1400f), new Color(0.04f, 0.05f, 0.08f, 0.94f), false);
        control.panelMenuPrincipal = columna;

        GameObject franja = PanelUI(columna.transform, "Franja", new Vector2(1f, 0.5f), Vector2.zero, new Vector2(6f, 1400f), ColNaranja, false);
        franja.GetComponent<Image>().raycastTarget = false;

        GameObject etiqueta = PanelUI(columna.transform, "EtiquetaODS", new Vector2(0f, 0.5f), new Vector2(80f, 345f), new Vector2(330f, 42f), new Color(0.13f, 0.6f, 0.45f, 0.95f));
        TextoUI(etiqueta.transform, "Texto", "ODS 3  ·  SALUD Y BIENESTAR", 18f, Centro, Vector2.zero, new Vector2(320f, 40f), TextAlignmentOptions.Center, Color.white, true);

        TextoUI(columna.transform, "TituloA", "OPERACIÓN", 44f, new Vector2(0f, 0.5f), new Vector2(80f, 280f), new Vector2(560f, 58f), TextAlignmentOptions.Left, ColNaranja, true).characterSpacing = 10f;
        TextoUI(columna.transform, "TituloB", "RESCATE", 104f, new Vector2(0f, 0.5f), new Vector2(76f, 190f), new Vector2(580f, 120f), TextAlignmentOptions.Left, Color.white, true);
        TextoUI(columna.transform, "Subtitulo", "Llega a la zona de desastre, reúne los suministros y salva a quienes te necesitan.", 24f, new Vector2(0f, 0.5f), new Vector2(80f, 96f), new Vector2(540f, 72f), TextAlignmentOptions.TopLeft, ColTextoSuave);

        Vector2 izquierda = new Vector2(0f, 0.5f);
        Vector2 tamano = new Vector2(540f, 74f);
        BotonUI(columna.transform, "BotonJugar", "JUGAR", 30f, izquierda, new Vector2(80f, 2f), new Vector2(540f, 86f), ColNaranja, control.Jugar);
        BotonUI(columna.transform, "BotonInstrucciones", "CÓMO JUGAR", 24f, izquierda, new Vector2(80f, -86f), tamano, ColGris, control.AbrirInstrucciones);
        BotonUI(columna.transform, "BotonAjustes", "AJUSTES", 24f, izquierda, new Vector2(80f, -168f), tamano, ColGris, control.AbrirAjustes);
        BotonUI(columna.transform, "BotonSalir", "SALIR", 24f, izquierda, new Vector2(80f, -250f), tamano, new Color(0.17f, 0.19f, 0.24f), control.Salir);

        ChipMonedas(canvas.transform, new Vector2(1f, 1f), new Vector2(-32f, -30f));

        SaludGameBuilder.CrearPanelAjustes(canvas.transform, control);
        ConstruirInstrucciones(canvas.transform, control);
        MusicaDeEscena("MusicaMenu.ogg", null);

        EditorUtility.SetDirty(control);
        EditorSceneManager.MarkSceneDirty(escena);
        EditorSceneManager.SaveScene(escena);
    }

    private static void ConstruirDecoradoMenu()
    {
        Transform raiz = new GameObject("DecoradoMenu").transform;

        Primitiva(PrimitiveType.Plane, raiz, "Suelo", Vector3.zero, new Vector3(30f, 1f, 30f), MatSuelo());

        Poner(CIUDAD + "office_building_2.prefab", raiz, new Vector3(-12f, -1f, 58f), 8f, 1f, false);
        Poner(CIUDAD + "office_building_3.prefab", raiz, new Vector3(40f, -2f, 52f), -14f, 1f, false);
        Poner(CIUDAD + "mid_house_1.prefab", raiz, new Vector3(18f, 0f, 26f), 186f, 1f, false);

        Ambulancia(raiz, new Vector3(5.3f, 0f, 4.8f), 200f);
        Camilla(raiz, new Vector3(1.1f, 0f, 2.1f), 62f);

        GameObject rescatista = new GameObject("Rescatista");
        rescatista.transform.SetParent(raiz, false);
        GameObject modeloRescatista = CrearPersonaje(rescatista.transform, "Modelo", PielRescatista, 0f);
        rescatista.transform.position = new Vector3(1.9f, 0f, 0.4f);
        rescatista.transform.rotation = Quaternion.Euler(0f, 205f, 0f);
        foreach (string id in new[] { "casco", "mochila" })
        {
            EquipamientoJugador.Colocar(modeloRescatista.transform, Cargar<ObjetoTienda>(DIR_TIENDA + "/Objeto_" + id + ".asset"));
        }

        Roca(raiz, new Vector3(-3.5f, 0.2f, 9f), 3f, 20f, 1);
        Roca(raiz, new Vector3(-1f, 0.1f, 11f), 2.2f, 80f, 2);
        Roca(raiz, new Vector3(-0.6f, 0.1f, 5f), 1.1f, 140f, 3);
        Losa(raiz, new Vector3(-6f, 0.8f, 12f), new Vector3(7f, 0.35f, 4f), new Vector3(12f, 25f, 16f));
        Losa(raiz, new Vector3(3f, 1f, 14f), new Vector3(6f, 0.35f, 3.5f), new Vector3(-9f, -30f, 18f));

        Fuego(raiz, new Vector3(0.5f, 0f, 9.5f), false);
        Fuego(raiz, new Vector3(-4.5f, 0f, 13f), false);
        Fuego(raiz, new Vector3(11f, 0f, 8f), false);
        Humo(raiz, new Vector3(0.5f, 0.5f, 12f));
        Humo(raiz, new Vector3(-12f, 0.5f, 22f));
        Humo(raiz, new Vector3(16f, 1f, 22f));
    }

    private static void ConstruirInstrucciones(Transform canvas, MenuPrincipalController control)
    {
        GameObject velo = Velo(canvas, "PanelInstrucciones", new Color(0.02f, 0.03f, 0.05f, 0.78f));
        control.panelInstrucciones = velo;

        GameObject tarjeta = PanelUI(velo.transform, "Tarjeta", Centro, Vector2.zero, new Vector2(1180f, 760f), ColFondo);
        TextoUI(tarjeta.transform, "Titulo", "CÓMO JUGAR", 44f, new Vector2(0.5f, 1f), new Vector2(0f, -34f), new Vector2(1000f, 60f), TextAlignmentOptions.Center, Color.white, true);

        string mision =
            "<b><color=#F28019>TU MISIÓN</color></b>\n\n" +
            "Habla con el médico del Hub y elige una misión.\n\n" +
            "Busca <b>jeringas</b> y <b>equipajes de salud</b> y llévalos al paciente.\n\n" +
            "Junta <b>monedas</b> por el mapa y gástalas en la tienda del Hub.\n\n" +
            "Evita el fuego y a los <b>saqueadores</b>: si te ven, te persiguen. Escóndete o defiéndete con un arma de la tienda.";

        string controles =
            "<b><color=#F28019>CONTROLES</color></b>\n\n" +
            "<b>W A S D</b>   Caminar\n\n" +
            "<b>Shift</b>   Correr\n\n" +
            "<b>Espacio</b>   Saltar\n\n" +
            "<b>Ratón</b>   Cámara\n\n" +
            "<b>Clic izquierdo o F</b>   Atacar (con arma)\n\n" +
            "<b>E</b>   Hablar\n\n" +
            "<b>Esc</b>   Pausa";

        TextoUI(tarjeta.transform, "Mision", mision, 25f, new Vector2(0f, 1f), new Vector2(64f, -122f), new Vector2(600f, 520f), TextAlignmentOptions.TopLeft, ColTextoSuave);
        TextoUI(tarjeta.transform, "Controles", controles, 25f, new Vector2(1f, 1f), new Vector2(-64f, -122f), new Vector2(380f, 520f), TextAlignmentOptions.TopLeft, ColTextoSuave);

        BotonUI(tarjeta.transform, "BtnCerrarInst", "ENTENDIDO", 24f, new Vector2(0.5f, 0f), new Vector2(0f, 34f), new Vector2(320f, 66f), ColNaranja, control.CerrarInstrucciones);
        velo.SetActive(false);
    }

    #endregion

    #region Hub

    private static void MontarHub()
    {
        Scene escena = EditorSceneManager.OpenScene("Assets/Scenes/Hub.unity", OpenSceneMode.Single);

        BorrarRaiz("PuestoTienda");
        BorrarRaiz("NPC_Tienda");
        BorrarRaiz("LetrerosHub");
        BorrarRaiz("VidaHub");

        // Cámara suelta que estaba desactivada y duplicaba la del jugador.
        foreach (GameObject objeto in escena.GetRootGameObjects())
        {
            if (objeto.name == "Main Camera" && !objeto.activeSelf) Object.DestroyImmediate(objeto);
        }

        GameObject jugadorGO = GameObject.Find("JugadorHub");
        PlayerController jugador = PrepararJugador(jugadorGO);
        jugadorGO.transform.position = new Vector3(0f, 1.05f, -8f);

        Camera camara = jugador.camaraJugador;
        camara.transform.position = new Vector3(0f, 3f, -13f);
        camara.transform.rotation = Quaternion.Euler(12f, 0f, 0f);

        AmbienteHub(camara);
        AjustarEdificioHospital();

        GameObject medico = GameObject.Find("NPC_Medico");
        NPCMedicoInteractivo npcMisiones = PrepararNPC(medico, new Vector3(4.2f, 0f, 3f), 200f, PielMedico, jugador);

        GameObject vendedor = new GameObject("NPC_Tienda");
        NPCMedicoInteractivo npcTienda = PrepararNPC(vendedor, new Vector3(-9f, 0f, 3.2f), 150f, PielTendero, jugador);

        ConstruirPuestoTienda();

        Transform letreros = new GameObject("LetrerosHub").transform;
        Letrero(letreros, "LetreroMisiones", "MISIONES", new Vector3(4.2f, 2.5f, 3f), 5f, ColNaranja);
        Letrero(letreros, "LetreroTienda", "TIENDA", new Vector3(-9f, 3.9f, 3.2f), 5f, ColOro);

        ConstruirVidaHub(jugadorGO);

        Canvas canvas = PrepararCanvas();
        GestorNiveles gestor = Object.FindAnyObjectByType<GestorNiveles>();

        npcMisiones.textoInteraccion = Aviso(canvas.transform, "AvisoMisiones", "Pulsa  E  para ver las misiones");
        npcTienda.textoInteraccion = Aviso(canvas.transform, "AvisoTienda", "Pulsa  E  para abrir la tienda");

        npcMisiones.panelNiveles = ConstruirPanelMisiones(canvas.transform, gestor, npcMisiones);
        npcTienda.panelNiveles = ConstruirPanelTienda(canvas.transform, npcTienda);

        PanelPausa(canvas.transform, jugador, "VOLVER AL MENÚ", gestor.CargarMenuPrincipal);

        ConstruirDialogo(canvas.transform);
        MusicaDeEscena("MusicaHub.mp3", null);

        // Al final para que quede por encima de los paneles abiertos.
        ChipMonedas(canvas.transform, new Vector2(1f, 1f), new Vector2(-32f, -30f));

        EditorUtility.SetDirty(jugador);
        EditorSceneManager.MarkSceneDirty(escena);
        EditorSceneManager.SaveScene(escena);
    }

    private static void AmbienteHub(Camera camara)
    {
        Material cielo = AssetDatabase.LoadAssetAtPath<Material>(DIR_MAT + "/CieloHub.mat");
        if (cielo == null)
        {
            cielo = new Material(Shader.Find("Skybox/Panoramic"));
            AssetDatabase.CreateAsset(cielo, DIR_MAT + "/CieloHub.mat");
        }
        cielo.SetTexture("_MainTex", Cargar<Texture>(POLY + "kloofendal_48d_partly_cloudy_puresky_2k.hdr"));
        cielo.SetFloat("_Exposure", 1f);
        EditorUtility.SetDirty(cielo);

        RenderSettings.skybox = cielo;
        RenderSettings.fog = true;
        RenderSettings.fogMode = FogMode.Linear;
        RenderSettings.fogColor = new Color(0.74f, 0.82f, 0.92f);
        RenderSettings.fogStartDistance = 80f;
        RenderSettings.fogEndDistance = 340f;
        RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Trilight;
        RenderSettings.ambientSkyColor = new Color(0.66f, 0.76f, 0.9f);
        RenderSettings.ambientEquatorColor = new Color(0.6f, 0.62f, 0.58f);
        RenderSettings.ambientGroundColor = new Color(0.3f, 0.3f, 0.24f);

        camara.clearFlags = CameraClearFlags.Skybox;
        camara.farClipPlane = 800f;

        Light sol = BuscarSol();
        sol.color = new Color(1f, 0.95f, 0.86f);
        sol.intensity = 1.7f;
        sol.shadows = LightShadows.Soft;
        sol.transform.rotation = Quaternion.Euler(48f, -38f, 0f);

        GameObject suelo = GameObject.Find("SueloHub");
        suelo.transform.localScale = new Vector3(40f, 1f, 40f);
        suelo.GetComponent<Renderer>().sharedMaterial = MatTextura("CespedHub", "leafy_grass", new Color(0.82f, 0.92f, 0.72f), 110f);

        foreach (string nombre in new[] { "LimiteNorte", "LimiteSur", "LimiteEste", "LimiteOeste" })
        {
            GameObject.Find(nombre).GetComponent<Renderer>().enabled = false;
        }
    }

    /// <summary>El hospital estaba escalado para el personaje de 3,6 m; lo lleva a proporción humana y lo hace sólido.</summary>
    private static void AjustarEdificioHospital()
    {
        // Se busca en la raíz: GameObject.Find devolvería la malla interna con el mismo nombre.
        GameObject hospital = BuscarRaiz("Hospital");
        hospital.transform.localScale = Vector3.one * 0.6f;
        hospital.transform.position = new Vector3(0f, 0f, 16f);

        foreach (MeshFilter malla in hospital.GetComponentsInChildren<MeshFilter>(true))
        {
            if (malla.name == "Ground")
            {
                malla.GetComponent<Renderer>().enabled = false;
                continue;
            }

            // Los árboles y ambulancias tienen cajas; el edificio usa su forma real.
            if (malla.GetComponent<Collider>() != null) continue;
            if (malla.name == "Hospital") malla.gameObject.AddComponent<MeshCollider>();
            else malla.gameObject.AddComponent<BoxCollider>();
        }
    }

    private static NPCMedicoInteractivo PrepararNPC(GameObject npc, Vector3 pos, float giroY, Material piel, PlayerController jugador)
    {
        foreach (string viejo in new[] { "Doctor", "Modelo" })
        {
            Transform hijo = npc.transform.Find(viejo);
            if (hijo != null) Object.DestroyImmediate(hijo.gameObject);
        }

        npc.transform.position = pos;
        npc.transform.rotation = Quaternion.Euler(0f, giroY, 0f);
        CrearPersonaje(npc.transform, "Modelo", piel, 0f);

        foreach (Collider anterior in npc.GetComponents<Collider>()) Object.DestroyImmediate(anterior);

        SphereCollider zona = npc.AddComponent<SphereCollider>();
        zona.isTrigger = true;
        zona.center = Vector3.up;
        zona.radius = 3f;

        CapsuleCollider cuerpoSolido = npc.AddComponent<CapsuleCollider>();
        cuerpoSolido.center = Vector3.up * 0.95f;
        cuerpoSolido.height = 1.9f;
        cuerpoSolido.radius = 0.4f;

        NPCMedicoInteractivo interaccion = npc.GetComponent<NPCMedicoInteractivo>();
        if (interaccion == null) interaccion = npc.AddComponent<NPCMedicoInteractivo>();
        interaccion.controladorMovimiento = jugador;
        return interaccion;
    }

    private static void ConstruirPuestoTienda()
    {
        Transform raiz = new GameObject("PuestoTienda").transform;
        raiz.position = new Vector3(-9f, 0f, 3.2f);
        raiz.rotation = Quaternion.Euler(0f, 150f, 0f);

        Material madera = Mat("Madera", new Color(0.42f, 0.28f, 0.16f), 0.2f);
        Material lona = Mat("LonaNaranja", new Color(0.92f, 0.45f, 0.1f), 0.2f);

        // El mostrador queda delante del vendedor, entre él y el jugador.
        Primitiva(PrimitiveType.Cube, raiz, "Mostrador", new Vector3(0f, 0.55f, 1.1f), new Vector3(3.8f, 1.1f, 0.7f), madera, null, true);
        Primitiva(PrimitiveType.Cube, raiz, "Repisa", new Vector3(0f, 1.1f, 1.1f), new Vector3(4f, 0.08f, 0.9f), madera);

        GameObject carpa = Carpa(raiz, Vector3.zero, 0f, 4.8f, 3.4f, lona, false);
        carpa.transform.localPosition = new Vector3(0f, 0f, 0.1f);
        carpa.transform.localRotation = Quaternion.identity;

        Vector3 Mundo(float x, float y, float z) => raiz.TransformPoint(new Vector3(x, y, z));
        float giro = raiz.eulerAngles.y;

        Poner(OBJETOS + "HealthKit.prefab", raiz, Mundo(-1.2f, 1.15f, 1.1f), giro, 1f, false);
        Poner(OBJETOS + "ToolboxRed.prefab", raiz, Mundo(1.1f, 1.15f, 1.1f), giro + 90f, 0.45f, false);
        Poner(OBJETOS + "Flashlight.prefab", raiz, Mundo(0.1f, 1.2f, 1.2f), giro, 1f, false);
        Caja(raiz, Mundo(-1.9f, 0f, -1.2f), giro + 15f, 1.5f);
        Caja(raiz, Mundo(1.7f, 0f, -1.1f), giro - 10f, 1.5f);
        Caja(raiz, Mundo(1.7f, 0.72f, -1.1f), giro + 30f, 1.2f);
    }

    private static void ConstruirDialogo(Transform canvas)
    {
        RectTransform raiz = NuevoRect(canvas, "Dialogo", new Vector2(0.5f, 0f), Vector2.zero, Vector2.zero);
        DialogoUI dialogo = raiz.gameObject.AddComponent<DialogoUI>();

        GameObject panel = PanelUI(raiz, "Panel", new Vector2(0.5f, 0f), new Vector2(0f, 36f), new Vector2(1180f, 170f), new Color(0.05f, 0.07f, 0.1f, 0.96f));
        panel.GetComponent<RectTransform>().pivot = new Vector2(0.5f, 0f);
        panel.GetComponent<Image>().raycastTarget = false;

        GameObject franja = PanelUI(panel.transform, "Franja", new Vector2(0f, 0.5f), new Vector2(0f, 0f), new Vector2(10f, 170f), ColNaranja);
        franja.GetComponent<Image>().raycastTarget = false;

        dialogo.panel = panel;
        dialogo.textoNombre = TextoUI(panel.transform, "Nombre", "Nombre", 28f, new Vector2(0f, 1f), new Vector2(40f, -18f), new Vector2(1000f, 36f), TextAlignmentOptions.Left, ColNaranja, true);
        dialogo.textoFrase = TextoUI(panel.transform, "Frase", "", 34f, new Vector2(0f, 1f), new Vector2(40f, -60f), new Vector2(1100f, 96f), TextAlignmentOptions.TopLeft, Color.white);
    }

    private static void MusicaDeEscena(string principal, string combate)
    {
        BorrarRaiz("Musica");

        MusicaEscena musica = new GameObject("Musica").AddComponent<MusicaEscena>();
        musica.temaPrincipal = Cargar<AudioClip>("Assets/Externos/Musica/" + principal);
        if (combate != null) musica.temaCombate = Cargar<AudioClip>("Assets/Externos/Musica/" + combate);
    }

    private static GameObject Aviso(Transform canvas, string nombre, string texto)
    {
        GameObject aviso = PanelUI(canvas, nombre, new Vector2(0.5f, 0f), new Vector2(0f, 230f), new Vector2(560f, 66f), new Color(0.06f, 0.08f, 0.11f, 0.9f));
        aviso.GetComponent<Image>().raycastTarget = false;
        TextoUI(aviso.transform, "Texto", texto, 27f, Centro, Vector2.zero, new Vector2(540f, 60f), TextAlignmentOptions.Center, Color.white, true);
        aviso.SetActive(false);
        return aviso;
    }

    private static GameObject ConstruirPanelMisiones(Transform canvas, GestorNiveles gestor, NPCMedicoInteractivo npc)
    {
        GameObject velo = Velo(canvas, "PanelNiveles", new Color(0.02f, 0.03f, 0.05f, 0.78f));
        GameObject tarjeta = PanelUI(velo.transform, "Tarjeta", Centro, Vector2.zero, new Vector2(1420f, 800f), ColFondo);

        TextoUI(tarjeta.transform, "Antetitulo", "CENTRAL DE EMERGENCIAS", 22f, new Vector2(0f, 1f), new Vector2(60f, -38f), new Vector2(800f, 32f), TextAlignmentOptions.Left, ColNaranja, true).characterSpacing = 5f;
        TextoUI(tarjeta.transform, "Titulo", "Elige tu misión", 52f, new Vector2(0f, 1f), new Vector2(58f, -70f), new Vector2(900f, 70f), TextAlignmentOptions.Left, Color.white, true);

        TarjetaMision(tarjeta.transform, 1, -440f, "Terremoto en el centro",
            "Una réplica destruyó la ciudad. Hay dos heridos atrapados entre los escombros.",
            "Media", "60 monedas", new Color(0.88f, 0.42f, 0.1f), gestor);
        TarjetaMision(tarjeta.transform, 2, 0f, "Incendio en la reserva",
            "El fuego avanza por el bosque. Cruza el río y rescata a los campistas.",
            "Alta", "90 monedas", new Color(0.72f, 0.2f, 0.12f), gestor);
        TarjetaMision(tarjeta.transform, 3, 440f, "Emergencia en la base Selene",
            "Meteoritos sobre la base lunar. Tres heridos con gravedad baja y androides averiados.",
            "Muy alta", "150 monedas", new Color(0.24f, 0.3f, 0.62f), gestor);

        BotonUI(tarjeta.transform, "BtnVolver", "VOLVER", 22f, new Vector2(0.5f, 0f), new Vector2(0f, 30f), new Vector2(280f, 60f), ColGris, npc.CerrarMenu);
        velo.SetActive(false);
        return velo;
    }

    private static void TarjetaMision(Transform padre, int numero, float x, string titulo, string texto, string dificultad, string recompensa, Color color, GestorNiveles gestor)
    {
        GameObject tarjeta = PanelUI(padre, "Mision" + numero, Centro, new Vector2(x, -22f), new Vector2(420f, 500f), ColTarjeta);

        GameObject banda = PanelUI(tarjeta.transform, "Banda", new Vector2(0.5f, 1f), Vector2.zero, new Vector2(420f, 130f), color);
        banda.GetComponent<Image>().raycastTarget = false;
        TextoUI(banda.transform, "Numero", "MISIÓN " + numero, 20f, new Vector2(0f, 1f), new Vector2(26f, -20f), new Vector2(200f, 28f), TextAlignmentOptions.Left, new Color(1f, 1f, 1f, 0.85f), true).characterSpacing = 4f;
        TextMeshProUGUI estado = TextoUI(banda.transform, "Estado", "", 18f, new Vector2(1f, 1f), new Vector2(-22f, -20f), new Vector2(170f, 28f), TextAlignmentOptions.Right, Color.white, true);
        TextoUI(banda.transform, "Titulo", titulo, 30f, new Vector2(0f, 0f), new Vector2(26f, 14f), new Vector2(370f, 76f), TextAlignmentOptions.BottomLeft, Color.white, true);

        TextoUI(tarjeta.transform, "Descripcion", texto, 22f, new Vector2(0f, 1f), new Vector2(26f, -150f), new Vector2(368f, 130f), TextAlignmentOptions.TopLeft, ColTextoSuave);
        TextoUI(tarjeta.transform, "Datos", "<b>Dificultad:</b> " + dificultad + "\n<b>Recompensa:</b> " + recompensa, 21f, new Vector2(0f, 0f), new Vector2(26f, 112f), new Vector2(368f, 76f), TextAlignmentOptions.BottomLeft, ColTextoSuave);

        Button boton = BotonUI(tarjeta.transform, "BtnNivel" + numero, "INICIAR", 24f, new Vector2(0.5f, 0f), new Vector2(0f, 26f), new Vector2(368f, 68f), ColNaranja, null);
        UnityEventTools.AddStringPersistentListener(boton.onClick, gestor.CargarEscena, "Nivel" + numero);

        GameObject bloqueo = PanelUI(tarjeta.transform, "Bloqueado", new Vector2(0.5f, 0f), new Vector2(0f, 26f), new Vector2(368f, 68f), new Color(0.16f, 0.18f, 0.22f));
        bloqueo.GetComponent<Image>().raycastTarget = false;
        TextoUI(bloqueo.transform, "Texto", "COMPLETA LA ANTERIOR", 20f, Centro, Vector2.zero, new Vector2(360f, 60f), TextAlignmentOptions.Center, new Color(0.6f, 0.64f, 0.7f), true).characterSpacing = 2f;

        TarjetaNivel control = tarjeta.AddComponent<TarjetaNivel>();
        control.numeroNivel = numero;
        control.botonIniciar = boton.gameObject;
        control.bloqueo = bloqueo;
        control.textoEstado = estado;
    }

    private static GameObject ConstruirPanelTienda(Transform canvas, NPCMedicoInteractivo npc)
    {
        // Ocupa la derecha para que el jugador vea su personaje mientras compra.
        GameObject panel = PanelUI(canvas, "PanelTienda", new Vector2(1f, 0.5f), new Vector2(-30f, -20f), new Vector2(880f, 960f), ColFondo);

        TextoUI(panel.transform, "Antetitulo", "PUESTO DE SUMINISTROS", 20f, new Vector2(0f, 1f), new Vector2(40f, -30f), new Vector2(560f, 30f), TextAlignmentOptions.Left, ColOro, true).characterSpacing = 5f;
        TextoUI(panel.transform, "Titulo", "Equípate", 46f, new Vector2(0f, 1f), new Vector2(38f, -58f), new Vector2(560f, 62f), TextAlignmentOptions.Left, Color.white, true);

        RectTransform lista = NuevoRect(panel.transform, "Lista", new Vector2(0.5f, 1f), new Vector2(0f, -136f), new Vector2(800f, 720f));
        VerticalLayoutGroup columnas = lista.gameObject.AddComponent<VerticalLayoutGroup>();
        columnas.spacing = 8f;
        columnas.childControlHeight = false;
        columnas.childControlWidth = false;
        columnas.childForceExpandHeight = false;
        columnas.childForceExpandWidth = false;
        columnas.childAlignment = TextAnchor.UpperCenter;

        FilaTienda plantilla = ConstruirFila(lista);

        TiendaUI tienda = panel.AddComponent<TiendaUI>();
        tienda.catalogo = Cargar<CatalogoTienda>(DIR_TIENDA + "/CatalogoTienda.asset");
        tienda.plantillaFila = plantilla;
        tienda.contenedorFilas = lista;
        tienda.textoMensaje = TextoUI(panel.transform, "Mensaje", "", 21f, new Vector2(0f, 0f), new Vector2(40f, 42f), new Vector2(530f, 56f), TextAlignmentOptions.MidlineLeft, ColTextoSuave);

        BotonUI(panel.transform, "BtnCerrar", "CERRAR", 22f, new Vector2(1f, 0f), new Vector2(-40f, 38f), new Vector2(220f, 62f), ColGris, npc.CerrarMenu);

        plantilla.gameObject.SetActive(false);
        panel.SetActive(false);
        return panel;
    }

    private static FilaTienda ConstruirFila(Transform lista)
    {
        GameObject fila = PanelUI(lista, "PlantillaFila", new Vector2(0.5f, 1f), Vector2.zero, new Vector2(800f, 96f), ColTarjeta);
        FilaTienda datos = fila.AddComponent<FilaTienda>();

        GameObject icono = PanelUI(fila.transform, "Icono", new Vector2(0f, 0.5f), new Vector2(14f, 0f), new Vector2(68f, 68f), Color.white);
        datos.icono = icono.GetComponent<Image>();
        datos.textoIcono = TextoUI(icono.transform, "Letra", "A", 38f, Centro, Vector2.zero, new Vector2(70f, 70f), TextAlignmentOptions.Center, Color.white, true);

        datos.textoNombre = TextoUI(fila.transform, "Nombre", "Nombre", 25f, new Vector2(0f, 1f), new Vector2(100f, -8f), new Vector2(450f, 30f), TextAlignmentOptions.Left, Color.white, true);
        datos.textoDescripcion = TextoUI(fila.transform, "Descripcion", "Descripción", 19f, new Vector2(0f, 1f), new Vector2(100f, -38f), new Vector2(490f, 26f), TextAlignmentOptions.Left, ColTextoSuave);
        datos.textoPrecio = TextoUI(fila.transform, "Precio", "0 monedas", 20f, new Vector2(0f, 0f), new Vector2(100f, 6f), new Vector2(300f, 26f), TextAlignmentOptions.Left, ColOro, true);

        Button boton = BotonUI(fila.transform, "Boton", "COMPRAR", 19f, new Vector2(1f, 0.5f), new Vector2(-14f, 0f), new Vector2(176f, 58f), Color.white, null);
        datos.boton = boton;
        datos.fondoBoton = boton.GetComponent<Image>();
        datos.textoBoton = boton.GetComponentInChildren<TextMeshProUGUI>();

        return datos;
    }

    #endregion

    private static Light BuscarSol()
    {
        foreach (Light luz in Object.FindObjectsByType<Light>(FindObjectsInactive.Include))
        {
            if (luz.type == LightType.Directional) return luz;
        }
        return null;
    }

    private static GameObject BuscarRaiz(string nombre)
    {
        foreach (GameObject objeto in SceneManager.GetActiveScene().GetRootGameObjects())
        {
            if (objeto.name == nombre) return objeto;
        }
        return null;
    }

    private static void BorrarRaiz(string nombre)
    {
        foreach (GameObject objeto in SceneManager.GetActiveScene().GetRootGameObjects())
        {
            if (objeto.name == nombre) Object.DestroyImmediate(objeto);
        }
    }
}
