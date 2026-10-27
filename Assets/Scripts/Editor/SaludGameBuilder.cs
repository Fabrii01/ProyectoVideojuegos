using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

[InitializeOnLoad]
public static class SaludGameBuilder
{
    private const string PATH_DOCTOR_FBX = "Assets/Models/Personajes/Medico/Doctor.fbx";
    private const string PATH_MAT_BODY = "Assets/Models/Personajes/Medico/textures 2/Materials/M_Doctor_Body.mat";
    private const string PATH_MAT_HEAD = "Assets/Models/Personajes/Medico/textures 2/Materials/M_Doctor_Head.mat";
    private const string PATH_MAT_ITEMS = "Assets/Kabungus/HouseholdItems/Scenes/URP/M_HouseholdItems_URP.mat";

    private const string PATH_SYRINGE_FBX = "Assets/Kabungus/HouseholdItems/Models/Syringe.fbx";
    private const string PATH_BRIEFCASE_FBX = "Assets/Kabungus/HouseholdItems/Models/Briefcase.fbx";
    private const string PATH_MEDKIT_FBX = "Assets/Kabungus/HouseholdItems/Models/Medkit.fbx";

    private const string PATH_PREFAB_JUGADOR = "Assets/Prefabs/PersonajeDoctorJugador.prefab";
    private const string PATH_PREFAB_JERINGA = "Assets/Prefabs/Recolectables/JeringaRecolectable.prefab";
    private const string PATH_PREFAB_EQUIPAJE = "Assets/Prefabs/Recolectables/EquipajeSaludRecolectable.prefab";
    private const string PATH_PREFAB_BOTIQUIN = "Assets/Prefabs/Recolectables/BotiquinRecolectable.prefab";

    [InitializeOnLoadMethod]
    private static void AutoEjecutarAlCompilar()
    {
        EditorApplication.delayCall += () =>
        {
            if (!SessionState.GetBool("SaludGameBuilder_EjecutadoV1", false))
            {
                SessionState.SetBool("SaludGameBuilder_EjecutadoV1", true);
                EjecutarMontajeCompleto();
            }
        };
    }

    [MenuItem("SaludGame/★ EJECUTAR MONTAJE COMPLETO DEL VIDEOJUEGO ★")]
    public static void EjecutarMontajeCompleto()
    {
        Debug.Log("=================================================");
        Debug.Log(">> INICIANDO MONTAJE INTEGRAL DEL VIDEOJUEGO DE SALUD");
        Debug.Log("=================================================");

        ConstruirPrefabs();
        ConfigurarMenuPrincipal();
        ConfigurarHub();
        ConfigurarNivel1();
        ConfigurarBuildSettings();

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log("=================================================");
        Debug.Log(">> ¡MONTAJE INTEGRAL COMPLETADO EXITOSAMENTE! <<");
        Debug.Log("=================================================");
    }

    [MenuItem("SaludGame/1. Construir Prefabs")]
    public static void ConstruirPrefabs()
    {
        CrearCarpetaSiNoExiste("Assets/Prefabs");
        CrearCarpetaSiNoExiste("Assets/Prefabs/Recolectables");

        ConstruirPrefabDoctorJugador();
        ConstruirPrefabsRecolectables();

        AssetDatabase.SaveAssets();
    }

    private static void CrearCarpetaSiNoExiste(string ruta)
    {
        if (!AssetDatabase.IsValidFolder(ruta))
        {
            string padre = System.IO.Path.GetDirectoryName(ruta).Replace('\\', '/');
            string sub = System.IO.Path.GetFileName(ruta);
            AssetDatabase.CreateFolder(padre, sub);
        }
    }

    public static void ConstruirPrefabDoctorJugador()
    {
        GameObject root = new GameObject("PersonajeDoctorJugador");
        root.tag = "Player";

        // Rigidbody
        Rigidbody rb = root.AddComponent<Rigidbody>();
        rb.mass = 1f;
        rb.linearDamping = 0.05f;
        rb.angularDamping = 0.05f;
        rb.constraints = RigidbodyConstraints.FreezeRotation;

        // Capsule Collider
        CapsuleCollider col = root.AddComponent<CapsuleCollider>();
        col.center = new Vector3(0f, 0.9f, 0f);
        col.radius = 0.35f;
        col.height = 1.8f;

        // Audio Source
        AudioSource audioSource = root.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;

        // Instanciar modelo FBX
        GameObject fbxAsset = AssetDatabase.LoadAssetAtPath<GameObject>(PATH_DOCTOR_FBX);
        if (fbxAsset != null)
        {
            GameObject visual = (GameObject)PrefabUtility.InstantiatePrefab(fbxAsset, root.transform);
            visual.name = "Doctor";
            visual.transform.localPosition = Vector3.zero;
            visual.transform.localRotation = Quaternion.Euler(0f, 180f, 0f);
            visual.transform.localScale = new Vector3(0.14f, 0.14f, 0.14f);

            Material matBody = AssetDatabase.LoadAssetAtPath<Material>(PATH_MAT_BODY);
            Material matHead = AssetDatabase.LoadAssetAtPath<Material>(PATH_MAT_HEAD);

            Renderer[] rends = visual.GetComponentsInChildren<Renderer>(true);
            foreach (var r in rends)
            {
                Material[] mats = new Material[r.sharedMaterials.Length];
                for (int i = 0; i < mats.Length; i++)
                {
                    // Slot 3 es la cabeza/rostro, los demás son cuerpo y vestimenta
                    mats[i] = (i == 3 && matHead != null) ? matHead : (matBody != null ? matBody : matHead);
                }
                r.sharedMaterials = mats;
            }
        }

        // Script PlayerController
        PlayerController pc = root.AddComponent<PlayerController>();
        pc.velocidadMovimiento = 6f;
        pc.velocidadCorrer = 9.5f;
        pc.fuerzaSalto = 6f;
        Transform docTransform = root.transform.Find("Doctor");
        if (docTransform != null) pc.modeloVisualDoctor = docTransform;

        PrefabUtility.SaveAsPrefabAsset(root, PATH_PREFAB_JUGADOR);
        GameObject.DestroyImmediate(root);
        Debug.Log(">> [SaludGame] Prefab Doctor Jugador generado.");
    }

    public static void ConstruirPrefabsRecolectables()
    {
        Material matItems = AssetDatabase.LoadAssetAtPath<Material>(PATH_MAT_ITEMS);

        // Jeringa
        ConstruirItemPrefab(
            PATH_PREFAB_JERINGA,
            PATH_SYRINGE_FBX,
            TipoItemSalud.Jeringa,
            1,
            10,
            new Color(0.1f, 0.85f, 1f),
            new Vector3(1.2f, 1.2f, 1.2f),
            matItems
        );

        // Equipaje de Salud
        ConstruirItemPrefab(
            PATH_PREFAB_EQUIPAJE,
            PATH_BRIEFCASE_FBX,
            TipoItemSalud.EquipajeSalud,
            1,
            20,
            new Color(1f, 0.85f, 0.2f),
            new Vector3(0.85f, 0.85f, 0.85f),
            matItems
        );

        // Botiquín
        ConstruirItemPrefab(
            PATH_PREFAB_BOTIQUIN,
            PATH_MEDKIT_FBX,
            TipoItemSalud.Botiquin,
            1,
            35,
            new Color(0.2f, 1f, 0.4f),
            new Vector3(0.9f, 0.9f, 0.9f),
            matItems
        );
    }

    private static void ConstruirItemPrefab(string outputPath, string fbxPath, TipoItemSalud tipo, int valor, int curacion, Color colorLuz, Vector3 escala, Material mat)
    {
        GameObject root = new GameObject(System.IO.Path.GetFileNameWithoutExtension(outputPath));
        root.tag = "Suministro";

        SphereCollider col = root.AddComponent<SphereCollider>();
        col.isTrigger = true;
        col.radius = 0.55f;

        ItemRecolectable item = root.AddComponent<ItemRecolectable>();
        item.tipoItem = tipo;
        item.valorSuministro = valor;
        item.cantidadCuracion = curacion;
        item.velocidadRotacion = 85f;
        item.amplitudFlotacion = 0.2f;
        item.frecuenciaFlotacion = 2.4f;

        GameObject luzGO = new GameObject("LuzAura");
        luzGO.transform.SetParent(root.transform);
        luzGO.transform.localPosition = Vector3.up * 0.25f;
        Light luz = luzGO.AddComponent<Light>();
        luz.type = LightType.Point;
        luz.range = 2.6f;
        luz.intensity = 1.4f;
        luz.color = colorLuz;
        item.luzAura = luz;

        GameObject fbx = AssetDatabase.LoadAssetAtPath<GameObject>(fbxPath);
        if (fbx != null)
        {
            GameObject model = (GameObject)PrefabUtility.InstantiatePrefab(fbx, root.transform);
            model.name = "Modelo";
            model.transform.localPosition = Vector3.zero;
            model.transform.localRotation = (tipo == TipoItemSalud.Jeringa) ? Quaternion.Euler(35f, 0f, 45f) : Quaternion.identity;
            model.transform.localScale = escala;

            if (mat != null)
            {
                Renderer[] rends = model.GetComponentsInChildren<Renderer>(true);
                foreach (var r in rends)
                {
                    r.sharedMaterial = mat;
                }
            }
        }

        PrefabUtility.SaveAsPrefabAsset(root, outputPath);
        GameObject.DestroyImmediate(root);
    }

    [MenuItem("SaludGame/2. Configurar Menú Principal y Ajustes")]
    public static void ConfigurarMenuPrincipal()
    {
        string scenePath = "Assets/Scenes/MenuPrincipal.unity";
        var scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);

        // 1. Asegurar EventSystem
        if (Object.FindFirstObjectByType<UnityEngine.EventSystems.EventSystem>() == null)
        {
            GameObject esGO = new GameObject("EventSystem");
            esGO.AddComponent<UnityEngine.EventSystems.EventSystem>();
            esGO.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
        }

        // 2. Configurar Canvas
        Canvas canvas = Object.FindFirstObjectByType<Canvas>();
        if (canvas == null)
        {
            GameObject canvasGO = new GameObject("Canvas");
            canvas = canvasGO.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasGO.AddComponent<CanvasScaler>();
            canvasGO.AddComponent<GraphicRaycaster>();
        }

        CanvasScaler scaler = canvas.GetComponent<CanvasScaler>();
        if (scaler != null)
        {
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.matchWidthOrHeight = 0.5f;
        }

        // 3. Gestor de Menú Principal
        MenuPrincipalController controller = Object.FindFirstObjectByType<MenuPrincipalController>();
        if (controller == null)
        {
            GameObject gestorGO = new GameObject("GestorMenu");
            controller = gestorGO.AddComponent<MenuPrincipalController>();
        }

        // 4. Panel Menú Principal
        Transform tMenuPrincipal = canvas.transform.Find("PanelMenuPrincipal");
        GameObject panelMenu;
        if (tMenuPrincipal == null)
        {
            panelMenu = new GameObject("PanelMenuPrincipal");
            panelMenu.transform.SetParent(canvas.transform, false);
            RectTransform rt = panelMenu.AddComponent<RectTransform>();
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.sizeDelta = Vector2.zero;
        }
        else
        {
            panelMenu = tMenuPrincipal.gameObject;
        }
        controller.panelMenuPrincipal = panelMenu;

        for (int i = panelMenu.transform.childCount - 1; i >= 0; i--)
        {
            Object.DestroyImmediate(panelMenu.transform.GetChild(i).gameObject);
        }

        Image bgImg = panelMenu.GetComponent<Image>();
        if (bgImg == null) bgImg = panelMenu.AddComponent<Image>();
        bgImg.color = new Color(0.04f, 0.08f, 0.14f, 0.92f);

        CrearTextoUI(panelMenu.transform, "TituloPrincipal", "OPERACIÓN RESCATE Y SALUD", 54, new Vector2(0.5f, 0.82f), new Vector2(1000, 80), TextAlignmentOptions.Center, new Color(0.3f, 0.9f, 1f));
        CrearTextoUI(panelMenu.transform, "Subtitulo", "Simulador Médico: Recolección de Jeringas y Equipajes de Salud", 24, new Vector2(0.5f, 0.74f), new Vector2(1000, 50), TextAlignmentOptions.Center, new Color(0.8f, 0.9f, 1f));

        GameObject btnJugar = CrearBotonUI(panelMenu.transform, "BotonJugar", "▶ INICIAR MISIÓN (JUGAR)", 28, new Vector2(0.5f, 0.56f), new Vector2(420, 65), new Color(0.12f, 0.55f, 0.4f));
        btnJugar.GetComponent<Button>().onClick.AddListener(controller.Jugar);

        GameObject btnAjustes = CrearBotonUI(panelMenu.transform, "BotonAjustes", "⚙ DESPLIEGUE DE AJUSTES", 26, new Vector2(0.5f, 0.45f), new Vector2(420, 60), new Color(0.18f, 0.35f, 0.55f));
        btnAjustes.GetComponent<Button>().onClick.AddListener(controller.AbrirAjustes);

        GameObject btnInstrucciones = CrearBotonUI(panelMenu.transform, "BotonInstrucciones", "📖 MISIÓN E INSTRUCCIONES", 24, new Vector2(0.5f, 0.35f), new Vector2(420, 60), new Color(0.25f, 0.3f, 0.45f));
        btnInstrucciones.GetComponent<Button>().onClick.AddListener(controller.AbrirInstrucciones);

        GameObject btnSalir = CrearBotonUI(panelMenu.transform, "BotonSalir", "✖ SALIR DEL JUEGO", 24, new Vector2(0.5f, 0.24f), new Vector2(420, 55), new Color(0.6f, 0.2f, 0.2f));
        btnSalir.GetComponent<Button>().onClick.AddListener(controller.Salir);

        // 5. Panel de Despliegue de Ajustes
        Transform tAjustes = canvas.transform.Find("PanelAjustes");
        if (tAjustes != null) Object.DestroyImmediate(tAjustes.gameObject);

        GameObject panelAjustes = new GameObject("PanelAjustes");
        panelAjustes.transform.SetParent(canvas.transform, false);
        RectTransform rtAj = panelAjustes.AddComponent<RectTransform>();
        rtAj.anchorMin = new Vector2(0.15f, 0.08f);
        rtAj.anchorMax = new Vector2(0.85f, 0.92f);
        rtAj.sizeDelta = Vector2.zero;

        Image imgAj = panelAjustes.AddComponent<Image>();
        imgAj.color = new Color(0.06f, 0.1f, 0.18f, 0.98f);

        PanelAjustesUI uiAjustes = panelAjustes.AddComponent<PanelAjustesUI>();
        uiAjustes.panelAjustes = panelAjustes;
        controller.panelAjustes = panelAjustes;

        CrearTextoUI(panelAjustes.transform, "TituloAjustes", "CONFIGURACIÓN Y DESPLIEGUE DE AJUSTES", 36, new Vector2(0.5f, 0.92f), new Vector2(700, 50), TextAlignmentOptions.Center, new Color(0.2f, 0.9f, 1f));

        // Audio
        CrearTextoUI(panelAjustes.transform, "HeaderAudio", "🔊 AJUSTES DE AUDIO", 24, new Vector2(0.28f, 0.82f), new Vector2(300, 35), TextAlignmentOptions.Left, Color.white);
        uiAjustes.sliderMaster = CrearSliderConEtiqueta(panelAjustes.transform, "SliderMaster", "Volumen General", 0.8f, new Vector2(0.28f, 0.74f), out uiAjustes.textoMaster);
        uiAjustes.sliderSFX = CrearSliderConEtiqueta(panelAjustes.transform, "SliderSFX", "Efectos (SFX)", 0.9f, new Vector2(0.28f, 0.64f), out uiAjustes.textoSFX);
        uiAjustes.sliderMusica = CrearSliderConEtiqueta(panelAjustes.transform, "SliderMusica", "Música Ambiente", 0.7f, new Vector2(0.28f, 0.54f), out uiAjustes.textoMusica);

        // Video / Pantalla
        CrearTextoUI(panelAjustes.transform, "HeaderVideo", "🖥 PANTALLA Y GRÁFICOS", 24, new Vector2(0.72f, 0.82f), new Vector2(300, 35), TextAlignmentOptions.Left, Color.white);
        uiAjustes.dropdownCalidad = CrearDropdownConEtiqueta(panelAjustes.transform, "DropdownCalidad", "Calidad Gráfica", new Vector2(0.72f, 0.74f));
        uiAjustes.togglePantallaCompleta = CrearToggleConEtiqueta(panelAjustes.transform, "TogglePantallaCompleta", "Pantalla Completa", true, new Vector2(0.72f, 0.64f));
        uiAjustes.dropdownResolucion = CrearDropdownConEtiqueta(panelAjustes.transform, "DropdownResolucion", "Resolución de Pantalla", new Vector2(0.72f, 0.54f));

        // Controles
        CrearTextoUI(panelAjustes.transform, "HeaderControles", "🎮 JUGABILIDAD Y CONTROLES", 24, new Vector2(0.5f, 0.42f), new Vector2(500, 35), TextAlignmentOptions.Center, Color.white);
        uiAjustes.sliderSensibilidad = CrearSliderConEtiqueta(panelAjustes.transform, "SliderSensibilidad", "Sensibilidad Ratón", 3.0f, new Vector2(0.35f, 0.32f), out uiAjustes.textoSensibilidad, 0.5f, 10f);
        uiAjustes.toggleInvertirY = CrearToggleConEtiqueta(panelAjustes.transform, "ToggleInvertirY", "Invertir Eje Y", false, new Vector2(0.72f, 0.32f));

        GameObject btnRestablecer = CrearBotonUI(panelAjustes.transform, "BtnRestablecer", "↺ Restablecer Valores", 20, new Vector2(0.35f, 0.12f), new Vector2(240, 48), new Color(0.4f, 0.4f, 0.4f));
        uiAjustes.botonRestablecer = btnRestablecer.GetComponent<Button>();

        GameObject btnCerrar = CrearBotonUI(panelAjustes.transform, "BtnCerrar", "✔ Guardar y Volver", 22, new Vector2(0.65f, 0.12f), new Vector2(260, 48), new Color(0.15f, 0.6f, 0.4f));
        uiAjustes.botonCerrar = btnCerrar.GetComponent<Button>();
        btnCerrar.GetComponent<Button>().onClick.AddListener(controller.CerrarAjustes);

        panelAjustes.SetActive(false);

        // 6. Panel de Misión e Instrucciones
        Transform tInstrucciones = canvas.transform.Find("PanelInstrucciones");
        if (tInstrucciones != null) Object.DestroyImmediate(tInstrucciones.gameObject);

        GameObject panelInst = new GameObject("PanelInstrucciones");
        panelInst.transform.SetParent(canvas.transform, false);
        RectTransform rtInst = panelInst.AddComponent<RectTransform>();
        rtInst.anchorMin = new Vector2(0.2f, 0.12f);
        rtInst.anchorMax = new Vector2(0.8f, 0.88f);
        rtInst.sizeDelta = Vector2.zero;

        Image imgInst = panelInst.AddComponent<Image>();
        imgInst.color = new Color(0.05f, 0.08f, 0.15f, 0.98f);
        controller.panelInstrucciones = panelInst;

        CrearTextoUI(panelInst.transform, "TituloInst", "INSTRUCCIONES DE MISIÓN: RESCATE MÉDICO", 32, new Vector2(0.5f, 0.9f), new Vector2(650, 45), TextAlignmentOptions.Center, new Color(0.2f, 0.85f, 1f));

        string cuerpoInstrucciones =
            "<b>OBJETIVOS DE LA OPERACIÓN:</b>\n" +
            "• Recolecta las <b>Jeringas Médicas</b> (luz azul) y <b>Equipajes de Salud</b> (luz dorada) esparcidos por la zona.\n" +
            "• Recoge botiquines para restaurar tu salud si eres impactado.\n" +
            "• Ten cuidado con los <b>obstáculos móviles y balizas de peligro</b>.\n" +
            "• Acércate al <b>Paciente Herido</b> en el puesto de triage para estabilizarlo y completar el rescate.\n\n" +
            "<b>CONTROLES DEL DOCTOR:</b>\n" +
            "• <b>W, A, S, D</b>: Moverse en el escenario.\n" +
            "• <b>Shift Izquierdo</b>: Correr / Acelerar paso.\n" +
            "• <b>Espacio</b>: Saltar obstáculos.\n" +
            "• <b>Ratón</b>: Mover y orientar la cámara en tercera persona.\n" +
            "• <b>ESC</b>: Pausa y acceso instantáneo al Menú de Ajustes.";

        CrearTextoUI(panelInst.transform, "CuerpoInst", cuerpoInstrucciones, 21, new Vector2(0.5f, 0.52f), new Vector2(750, 360), TextAlignmentOptions.TopLeft, Color.white);

        GameObject btnCerrarInst = CrearBotonUI(panelInst.transform, "BtnCerrarInst", "ENTENDIDO", 22, new Vector2(0.5f, 0.12f), new Vector2(220, 50), new Color(0.15f, 0.55f, 0.45f));
        btnCerrarInst.GetComponent<Button>().onClick.AddListener(controller.CerrarInstrucciones);

        panelInst.SetActive(false);

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        Debug.Log(">> [SaludGame] Escena MenuPrincipal configurada correctamente.");
    }

    [MenuItem("SaludGame/3. Configurar Escenario Hub")]
    public static void ConfigurarHub()
    {
        string scenePath = "Assets/Scenes/Hub.unity";
        var scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);

        GameObject jugadorViejo = GameObject.Find("JugadorHub");
        if (jugadorViejo != null)
        {
            MeshRenderer mr = jugadorViejo.GetComponent<MeshRenderer>();
            if (mr != null) mr.enabled = false;

            Transform docExistente = jugadorViejo.transform.Find("Doctor");
            if (docExistente == null)
            {
                GameObject fbxAsset = AssetDatabase.LoadAssetAtPath<GameObject>(PATH_DOCTOR_FBX);
                if (fbxAsset != null)
                {
                    GameObject visual = (GameObject)PrefabUtility.InstantiatePrefab(fbxAsset, jugadorViejo.transform);
                    visual.name = "Doctor";
                    visual.transform.localPosition = Vector3.zero;
                    visual.transform.localRotation = Quaternion.Euler(0f, 180f, 0f);
                    visual.transform.localScale = new Vector3(0.14f, 0.14f, 0.14f);

                    Material matBody = AssetDatabase.LoadAssetAtPath<Material>(PATH_MAT_BODY);
                    Material matHead = AssetDatabase.LoadAssetAtPath<Material>(PATH_MAT_HEAD);

                    Renderer[] rends = visual.GetComponentsInChildren<Renderer>(true);
                    foreach (var r in rends)
                    {
                        Material[] mats = new Material[r.sharedMaterials.Length];
                        for (int i = 0; i < mats.Length; i++)
                        {
                            mats[i] = (i == 3 && matHead != null) ? matHead : (matBody != null ? matBody : matHead);
                        }
                        r.sharedMaterials = mats;
                    }
                }
            }

            PlayerController pc = jugadorViejo.GetComponent<PlayerController>();
            if (pc != null)
            {
                Transform docTr = jugadorViejo.transform.Find("Doctor");
                if (docTr != null) pc.modeloVisualDoctor = docTr;
                pc.velocidadMovimiento = 6f;
                pc.velocidadCorrer = 9.5f;
            }

            Camera cam = jugadorViejo.GetComponentInChildren<Camera>();
            if (cam != null)
            {
                CameraController camCtrl = cam.GetComponent<CameraController>();
                if (camCtrl == null) camCtrl = cam.gameObject.AddComponent<CameraController>();
                camCtrl.objetivo = jugadorViejo.transform;
            }
        }

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        Debug.Log(">> [SaludGame] Escena Hub configurada correctamente.");
    }

    [MenuItem("SaludGame/4. Configurar Escenario Nivel 1")]
    public static void ConfigurarNivel1()
    {
        string scenePath = "Assets/Scenes/Nivel1.unity";
        var scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);

        Vector3 posCentro = new Vector3(500f, 1.2f, 500f);

        // 1. Mejorar el Jugador
        GameObject jugadorViejo = GameObject.Find("Jugador");
        if (jugadorViejo != null)
        {
            MeshRenderer mr = jugadorViejo.GetComponent<MeshRenderer>();
            if (mr != null) mr.enabled = false;

            Transform docExistente = jugadorViejo.transform.Find("Doctor");
            if (docExistente == null)
            {
                GameObject fbxAsset = AssetDatabase.LoadAssetAtPath<GameObject>(PATH_DOCTOR_FBX);
                if (fbxAsset != null)
                {
                    GameObject visual = (GameObject)PrefabUtility.InstantiatePrefab(fbxAsset, jugadorViejo.transform);
                    visual.name = "Doctor";
                    visual.transform.localPosition = Vector3.zero;
                    visual.transform.localRotation = Quaternion.Euler(0f, 180f, 0f);
                    visual.transform.localScale = new Vector3(0.14f, 0.14f, 0.14f);

                    Material matBody = AssetDatabase.LoadAssetAtPath<Material>(PATH_MAT_BODY);
                    Material matHead = AssetDatabase.LoadAssetAtPath<Material>(PATH_MAT_HEAD);

                    Renderer[] rends = visual.GetComponentsInChildren<Renderer>(true);
                    foreach (var r in rends)
                    {
                        Material[] mats = new Material[r.sharedMaterials.Length];
                        for (int i = 0; i < mats.Length; i++)
                        {
                            mats[i] = (i == 3 && matHead != null) ? matHead : (matBody != null ? matBody : matHead);
                        }
                        r.sharedMaterials = mats;
                    }
                }
            }

            PlayerController pc = jugadorViejo.GetComponent<PlayerController>();
            if (pc != null)
            {
                Transform docTr = jugadorViejo.transform.Find("Doctor");
                if (docTr != null) pc.modeloVisualDoctor = docTr;
                pc.velocidadMovimiento = 6f;
                pc.velocidadCorrer = 9.5f;
            }

            Camera cam = jugadorViejo.GetComponentInChildren<Camera>();
            if (cam != null)
            {
                CameraController cc = cam.GetComponent<CameraController>();
                if (cc == null) cc = cam.gameObject.AddComponent<CameraController>();
                cc.objetivo = jugadorViejo.transform;
            }
        }

        // 2. Incorporar Diseño de Escenario (Calles, Edificios, Farolas, Árboles)
        GameObject entornoPadre = GameObject.Find("EntornoUrbanoSalud");
        if (entornoPadre == null)
        {
            entornoPadre = new GameObject("EntornoUrbanoSalud");
            entornoPadre.transform.position = posCentro;
        }

        if (entornoPadre.transform.childCount == 0)
        {
            GameObject planoSuelo = GameObject.CreatePrimitive(PrimitiveType.Plane);
            planoSuelo.name = "ZonaAsfaltoOperacion";
            planoSuelo.transform.SetParent(entornoPadre.transform);
            planoSuelo.transform.position = posCentro + new Vector3(0f, 0.05f, 0f);
            planoSuelo.transform.localScale = new Vector3(6f, 1f, 6f);

            Material matAsfalto = AssetDatabase.LoadAssetAtPath<Material>("Assets/Versatile Studio Assets/Demo City By Versatile Studio/Materials/asphalt_2_tracks.mat");
            if (matAsfalto != null) planoSuelo.GetComponent<Renderer>().sharedMaterial = matAsfalto;

            GameObject prefabEdificio1 = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Versatile Studio Assets/Demo City By Versatile Studio/Prefabs/office_building_1.prefab");
            GameObject prefabEdificio2 = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Versatile Studio Assets/Demo City By Versatile Studio/Prefabs/mid_house_1.prefab");
            GameObject prefabFarola = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Versatile Studio Assets/Demo City By Versatile Studio/Prefabs/lamp_pole_single_blue.prefab");
            GameObject prefabArbol = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Versatile Studio Assets/Demo City By Versatile Studio/Prefabs/tree_1.prefab");
            GameObject prefabBloque = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Versatile Studio Assets/Demo City By Versatile Studio/Prefabs/concrete_block.prefab");

            if (prefabEdificio1 != null)
            {
                InstanciarElemento(prefabEdificio1, entornoPadre.transform, posCentro + new Vector3(-22f, 0f, 25f), Quaternion.identity, Vector3.one * 0.8f);
                InstanciarElemento(prefabEdificio1, entornoPadre.transform, posCentro + new Vector3(22f, 0f, 25f), Quaternion.identity, Vector3.one * 0.8f);
            }

            if (prefabEdificio2 != null)
            {
                InstanciarElemento(prefabEdificio2, entornoPadre.transform, posCentro + new Vector3(-25f, 0f, -10f), Quaternion.Euler(0f, 90f, 0f), Vector3.one * 0.85f);
                InstanciarElemento(prefabEdificio2, entornoPadre.transform, posCentro + new Vector3(25f, 0f, -10f), Quaternion.Euler(0f, -90f, 0f), Vector3.one * 0.85f);
            }

            if (prefabFarola != null)
            {
                InstanciarElemento(prefabFarola, entornoPadre.transform, posCentro + new Vector3(-12f, 0f, 15f), Quaternion.Euler(0f, 90f, 0f), Vector3.one);
                InstanciarElemento(prefabFarola, entornoPadre.transform, posCentro + new Vector3(12f, 0f, 15f), Quaternion.Euler(0f, -90f, 0f), Vector3.one);
                InstanciarElemento(prefabFarola, entornoPadre.transform, posCentro + new Vector3(-12f, 0f, -15f), Quaternion.Euler(0f, 90f, 0f), Vector3.one);
                InstanciarElemento(prefabFarola, entornoPadre.transform, posCentro + new Vector3(12f, 0f, -15f), Quaternion.Euler(0f, -90f, 0f), Vector3.one);
            }

            if (prefabArbol != null)
            {
                InstanciarElemento(prefabArbol, entornoPadre.transform, posCentro + new Vector3(-18f, 0f, 5f), Quaternion.identity, Vector3.one);
                InstanciarElemento(prefabArbol, entornoPadre.transform, posCentro + new Vector3(18f, 0f, 5f), Quaternion.identity, Vector3.one);
            }

            if (prefabBloque != null)
            {
                for (int b = -4; b <= 4; b++)
                {
                    InstanciarElemento(prefabBloque, entornoPadre.transform, posCentro + new Vector3(b * 5f, 0f, -28f), Quaternion.identity, Vector3.one);
                }
            }
        }

        // 3. Reemplazar cubos primitivos por JERINGAS y EQUIPAJES DE SALUD
        GameObject botViejo0 = GameObject.Find("Botiquin");
        GameObject botViejo1 = GameObject.Find("Botiquin (1)");
        GameObject botViejo2 = GameObject.Find("Botiquin (2)");
        if (botViejo0 != null) Object.DestroyImmediate(botViejo0);
        if (botViejo1 != null) Object.DestroyImmediate(botViejo1);
        if (botViejo2 != null) Object.DestroyImmediate(botViejo2);

        GameObject grupoRecolectables = GameObject.Find("GrupoRecolectablesSalud");
        if (grupoRecolectables != null) Object.DestroyImmediate(grupoRecolectables);

        grupoRecolectables = new GameObject("GrupoRecolectablesSalud");

        GameObject pJeringa = AssetDatabase.LoadAssetAtPath<GameObject>(PATH_PREFAB_JERINGA);
        GameObject pEquipaje = AssetDatabase.LoadAssetAtPath<GameObject>(PATH_PREFAB_EQUIPAJE);
        GameObject pBotiquin = AssetDatabase.LoadAssetAtPath<GameObject>(PATH_PREFAB_BOTIQUIN);

        Vector3[] posJeringas = new Vector3[]
        {
            posCentro + new Vector3(4f, 0.4f, 5f),
            posCentro + new Vector3(-6f, 0.4f, 8f),
            posCentro + new Vector3(10f, 0.4f, 14f),
            posCentro + new Vector3(-11f, 0.4f, 17f),
            posCentro + new Vector3(0f, 0.4f, 22f)
        };

        if (pJeringa != null)
        {
            for (int j = 0; j < posJeringas.Length; j++)
            {
                GameObject jer = (GameObject)PrefabUtility.InstantiatePrefab(pJeringa, grupoRecolectables.transform);
                jer.name = $"Jeringa_{j + 1}";
                jer.transform.position = posJeringas[j];
            }
        }

        Vector3[] posEquipajes = new Vector3[]
        {
            posCentro + new Vector3(-8f, 0.5f, 4f),
            posCentro + new Vector3(8f, 0.5f, 10f),
            posCentro + new Vector3(-4f, 0.5f, 19f)
        };

        if (pEquipaje != null)
        {
            for (int e = 0; e < posEquipajes.Length; e++)
            {
                GameObject eq = (GameObject)PrefabUtility.InstantiatePrefab(pEquipaje, grupoRecolectables.transform);
                eq.name = $"EquipajeSalud_{e + 1}";
                eq.transform.position = posEquipajes[e];
            }
        }

        Vector3[] posBotiquines = new Vector3[]
        {
            posCentro + new Vector3(14f, 0.4f, 2f),
            posCentro + new Vector3(-14f, 0.4f, 12f)
        };

        if (pBotiquin != null)
        {
            for (int b = 0; b < posBotiquines.Length; b++)
            {
                GameObject bot = (GameObject)PrefabUtility.InstantiatePrefab(pBotiquin, grupoRecolectables.transform);
                bot.name = $"BotiquinCuracion_{b + 1}";
                bot.transform.position = posBotiquines[b];
            }
        }

        // 4. Mejorar Obstáculos Móviles
        ObstaculoMovil[] obstaculos = Object.FindObjectsByType<ObstaculoMovil>(FindObjectsSortMode.None);
        foreach (var obs in obstaculos)
        {
            obs.velocidadBase = 3.8f;
            obs.distancia = 6f;
            obs.danioAlJugador = 25;

            Renderer rend = obs.GetComponent<Renderer>();
            if (rend != null)
            {
                Material matPeligro = AssetDatabase.LoadAssetAtPath<Material>("Assets/Versatile Studio Assets/Demo City By Versatile Studio/Materials/orange_light.mat");
                if (matPeligro != null) rend.sharedMaterial = matPeligro;
            }
        }

        // 5. Configurar Paciente y Victoria
        Paciente paciente = Object.FindFirstObjectByType<Paciente>();
        if (paciente != null)
        {
            paciente.jeringasRequeridas = 3;
            paciente.equipajesRequeridos = 2;
            paciente.suministrosRequeridos = 5;

            Light luzPac = paciente.GetComponentInChildren<Light>();
            if (luzPac == null)
            {
                GameObject luzPacGO = new GameObject("LuzTriage");
                luzPacGO.transform.SetParent(paciente.transform);
                luzPacGO.transform.localPosition = Vector3.up * 1.5f;
                luzPac = luzPacGO.AddComponent<Light>();
                luzPac.type = LightType.Point;
                luzPac.range = 5f;
            }
            paciente.luzEstado = luzPac;
        }

        // 6. Actualizar HUD del Nivel 1
        Canvas canvasNivel = Object.FindFirstObjectByType<Canvas>();
        if (canvasNivel != null && jugadorViejo != null)
        {
            PlayerController pc = jugadorViejo.GetComponent<PlayerController>();

            Transform tHUD = canvasNivel.transform.Find("HUD_Salud");
            GameObject hudGO;
            if (tHUD == null)
            {
                hudGO = new GameObject("HUD_Salud");
                hudGO.transform.SetParent(canvasNivel.transform, false);
                RectTransform rtHud = hudGO.AddComponent<RectTransform>();
                rtHud.anchorMin = Vector2.zero;
                rtHud.anchorMax = Vector2.one;
                rtHud.sizeDelta = Vector2.zero;
            }
            else
            {
                hudGO = tHUD.gameObject;
            }

            CrearTextoUI(hudGO.transform, "BannerObjetivo", "🚑 MISIÓN: Recolecta 3 Jeringas y 2 Equipajes de Salud para salvar al paciente", 20, new Vector2(0.5f, 0.96f), new Vector2(900, 40), TextAlignmentOptions.Center, new Color(1f, 0.95f, 0.4f));

            GameObject panelInv = new GameObject("PanelInventario");
            panelInv.transform.SetParent(hudGO.transform, false);
            RectTransform rtInv = panelInv.AddComponent<RectTransform>();
            rtInv.anchorMin = new Vector2(0.78f, 0.82f);
            rtInv.anchorMax = new Vector2(0.98f, 0.95f);
            rtInv.sizeDelta = Vector2.zero;
            Image imgInv = panelInv.AddComponent<Image>();
            imgInv.color = new Color(0.05f, 0.1f, 0.18f, 0.85f);

            GameObject txtJer = CrearTextoUI(panelInv.transform, "TextoJeringas", "💉 Jeringas: 0 / 3", 20, new Vector2(0.5f, 0.65f), new Vector2(250, 30), TextAlignmentOptions.Left, new Color(0.3f, 0.9f, 1f));
            GameObject txtEq = CrearTextoUI(panelInv.transform, "TextoEquipajes", "💼 Equipajes: 0 / 2", 20, new Vector2(0.5f, 0.25f), new Vector2(250, 30), TextAlignmentOptions.Left, new Color(1f, 0.85f, 0.3f));

            if (pc != null)
            {
                pc.textoJeringas = txtJer.GetComponent<TextMeshProUGUI>();
                pc.textoEquipajes = txtEq.GetComponent<TextMeshProUGUI>();
            }

            Transform tVictoria = canvasNivel.transform.Find("PanelVictoria");
            GameObject panelVic;
            if (tVictoria == null)
            {
                panelVic = new GameObject("PanelVictoria");
                panelVic.transform.SetParent(canvasNivel.transform, false);
                RectTransform rtVic = panelVic.AddComponent<RectTransform>();
                rtVic.anchorMin = new Vector2(0.25f, 0.2f);
                rtVic.anchorMax = new Vector2(0.75f, 0.8f);
                rtVic.sizeDelta = Vector2.zero;

                Image imgVic = panelVic.AddComponent<Image>();
                imgVic.color = new Color(0.04f, 0.12f, 0.22f, 0.97f);

                CrearTextoUI(panelVic.transform, "TituloVictoria", "¡MISIÓN DE RESCATE CUMPLIDA!", 38, new Vector2(0.5f, 0.82f), new Vector2(600, 60), TextAlignmentOptions.Center, new Color(0.2f, 1f, 0.5f));

                GameObject txtMsg = CrearTextoUI(panelVic.transform, "MensajeVictoria", "¡Excelente trabajo Doctor!\nHas recolectado todos los suministros y estabilizado al paciente con éxito.", 22, new Vector2(0.5f, 0.52f), new Vector2(550, 150), TextAlignmentOptions.Center, Color.white);

                GameObject btnVolver = CrearBotonUI(panelVic.transform, "BtnVolverHubVictoria", "VOLVER AL HUB HOSPITALARIO", 22, new Vector2(0.5f, 0.2f), new Vector2(380, 55), new Color(0.15f, 0.6f, 0.4f));

                if (paciente != null)
                {
                    paciente.panelVictoria = panelVic;
                    paciente.textoMensajeVictoria = txtMsg.GetComponent<TextMeshProUGUI>();
                    btnVolver.GetComponent<Button>().onClick.AddListener(paciente.VolverAlHub);
                }

                panelVic.SetActive(false);
            }
        }

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        Debug.Log(">> [SaludGame] Escena Nivel1 configurada con entorno urbano, jeringas y equipajes.");
    }

    private static void InstanciarElemento(GameObject prefab, Transform padre, Vector3 pos, Quaternion rot, Vector3 escala)
    {
        GameObject instancia = (GameObject)PrefabUtility.InstantiatePrefab(prefab, padre);
        instancia.transform.position = pos;
        instancia.transform.rotation = rot;
        instancia.transform.localScale = escala;
    }

    public static void ConfigurarBuildSettings()
    {
        EditorBuildSettingsScene[] scenes = new EditorBuildSettingsScene[]
        {
            new EditorBuildSettingsScene("Assets/Scenes/MenuPrincipal.unity", true),
            new EditorBuildSettingsScene("Assets/Scenes/Hub.unity", true),
            new EditorBuildSettingsScene("Assets/Scenes/Nivel1.unity", true)
        };
        EditorBuildSettings.scenes = scenes;
        Debug.Log(">> [SaludGame] EditorBuildSettings actualizado con MenuPrincipal como escena inicial (0).");
    }

    #region Helpers de Creación UI

    public static GameObject CrearTextoUI(Transform padre, string nombre, string texto, float tamano, Vector2 anclaPos, Vector2 tamanoDelta, TextAlignmentOptions alineacion, Color color)
    {
        GameObject go = new GameObject(nombre);
        go.transform.SetParent(padre, false);
        RectTransform rt = go.AddComponent<RectTransform>();
        rt.anchorMin = anclaPos;
        rt.anchorMax = anclaPos;
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.sizeDelta = tamanoDelta;

        TextMeshProUGUI tmp = go.AddComponent<TextMeshProUGUI>();
        tmp.text = texto;
        tmp.fontSize = tamano;
        tmp.alignment = alineacion;
        tmp.color = color;
        tmp.raycastTarget = false;

        TMP_FontAsset font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/TextMesh Pro/Resources/Fonts & Materials/LiberationSans SDF.asset");
        if (font != null) tmp.font = font;

        return go;
    }

    public static GameObject CrearBotonUI(Transform padre, string nombre, string texto, float tamanoTexto, Vector2 anclaPos, Vector2 tamanoDelta, Color colorFondo)
    {
        GameObject go = new GameObject(nombre);
        go.transform.SetParent(padre, false);
        RectTransform rt = go.AddComponent<RectTransform>();
        rt.anchorMin = anclaPos;
        rt.anchorMax = anclaPos;
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.sizeDelta = tamanoDelta;

        Image img = go.AddComponent<Image>();
        img.color = colorFondo;

        Button btn = go.AddComponent<Button>();
        btn.targetGraphic = img;

        ColorBlock cb = btn.colors;
        cb.highlightedColor = colorFondo * 1.25f;
        cb.pressedColor = colorFondo * 0.8f;
        btn.colors = cb;

        GameObject txtGO = new GameObject("Texto");
        txtGO.transform.SetParent(go.transform, false);
        RectTransform rtTxt = txtGO.AddComponent<RectTransform>();
        rtTxt.anchorMin = Vector2.zero;
        rtTxt.anchorMax = Vector2.one;
        rtTxt.sizeDelta = Vector2.zero;

        TextMeshProUGUI tmp = txtGO.AddComponent<TextMeshProUGUI>();
        tmp.text = texto;
        tmp.fontSize = tamanoTexto;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.color = Color.white;
        tmp.raycastTarget = false;

        TMP_FontAsset font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/TextMesh Pro/Resources/Fonts & Materials/LiberationSans SDF.asset");
        if (font != null) tmp.font = font;

        return go;
    }

    public static Slider CrearSliderConEtiqueta(Transform padre, string nombre, string etiqueta, float valorInicial, Vector2 anclaPos, out TextMeshProUGUI textoValor, float min = 0f, float max = 1f)
    {
        GameObject contenedor = new GameObject(nombre + "_Contenedor");
        contenedor.transform.SetParent(padre, false);
        RectTransform rtCont = contenedor.AddComponent<RectTransform>();
        rtCont.anchorMin = anclaPos;
        rtCont.anchorMax = anclaPos;
        rtCont.pivot = new Vector2(0.5f, 0.5f);
        rtCont.sizeDelta = new Vector2(340, 50);

        CrearTextoUI(contenedor.transform, "Etiqueta", etiqueta, 18, new Vector2(0.35f, 0.75f), new Vector2(240, 24), TextAlignmentOptions.Left, Color.white);

        GameObject txtValGO = CrearTextoUI(contenedor.transform, "TextoValor", $"{Mathf.RoundToInt(valorInicial * 100)}%", 18, new Vector2(0.85f, 0.75f), new Vector2(80, 24), TextAlignmentOptions.Right, new Color(0.3f, 0.9f, 1f));
        textoValor = txtValGO.GetComponent<TextMeshProUGUI>();

        GameObject sliderGO = new GameObject(nombre);
        sliderGO.transform.SetParent(contenedor.transform, false);
        RectTransform rtSlider = sliderGO.AddComponent<RectTransform>();
        rtSlider.anchorMin = new Vector2(0.05f, 0.15f);
        rtSlider.anchorMax = new Vector2(0.95f, 0.45f);
        rtSlider.sizeDelta = Vector2.zero;

        Slider slider = sliderGO.AddComponent<Slider>();
        slider.minValue = min;
        slider.maxValue = max;
        slider.value = valorInicial;

        GameObject bgGO = new GameObject("Background");
        bgGO.transform.SetParent(sliderGO.transform, false);
        RectTransform rtBg = bgGO.AddComponent<RectTransform>();
        rtBg.anchorMin = Vector2.zero;
        rtBg.anchorMax = Vector2.one;
        rtBg.sizeDelta = Vector2.zero;
        Image bgImg = bgGO.AddComponent<Image>();
        bgImg.color = new Color(0.2f, 0.25f, 0.35f);

        GameObject fillArea = new GameObject("Fill Area");
        fillArea.transform.SetParent(sliderGO.transform, false);
        RectTransform rtFillArea = fillArea.AddComponent<RectTransform>();
        rtFillArea.anchorMin = Vector2.zero;
        rtFillArea.anchorMax = Vector2.one;
        rtFillArea.sizeDelta = Vector2.zero;

        GameObject fill = new GameObject("Fill");
        fill.transform.SetParent(fillArea.transform, false);
        RectTransform rtFill = fill.AddComponent<RectTransform>();
        rtFill.sizeDelta = Vector2.zero;
        Image fillImg = fill.AddComponent<Image>();
        fillImg.color = new Color(0.2f, 0.75f, 1f);

        slider.fillRect = rtFill;

        return slider;
    }

    public static Toggle CrearToggleConEtiqueta(Transform padre, string nombre, string etiqueta, bool valorInicial, Vector2 anclaPos)
    {
        GameObject contenedor = new GameObject(nombre + "_Contenedor");
        contenedor.transform.SetParent(padre, false);
        RectTransform rtCont = contenedor.AddComponent<RectTransform>();
        rtCont.anchorMin = anclaPos;
        rtCont.anchorMax = anclaPos;
        rtCont.pivot = new Vector2(0.5f, 0.5f);
        rtCont.sizeDelta = new Vector2(340, 40);

        CrearTextoUI(contenedor.transform, "Etiqueta", etiqueta, 18, new Vector2(0.4f, 0.5f), new Vector2(240, 30), TextAlignmentOptions.Left, Color.white);

        GameObject toggleGO = new GameObject(nombre);
        toggleGO.transform.SetParent(contenedor.transform, false);
        RectTransform rtTog = toggleGO.AddComponent<RectTransform>();
        rtTog.anchorMin = new Vector2(0.85f, 0.5f);
        rtTog.anchorMax = new Vector2(0.85f, 0.5f);
        rtTog.sizeDelta = new Vector2(28, 28);

        Image imgBg = toggleGO.AddComponent<Image>();
        imgBg.color = new Color(0.2f, 0.3f, 0.45f);

        Toggle toggle = toggleGO.AddComponent<Toggle>();
        toggle.isOn = valorInicial;

        GameObject checkGO = new GameObject("Checkmark");
        checkGO.transform.SetParent(toggleGO.transform, false);
        RectTransform rtCheck = checkGO.AddComponent<RectTransform>();
        rtCheck.anchorMin = new Vector2(0.15f, 0.15f);
        rtCheck.anchorMax = new Vector2(0.85f, 0.85f);
        rtCheck.sizeDelta = Vector2.zero;
        Image checkImg = checkGO.AddComponent<Image>();
        checkImg.color = new Color(0.2f, 0.9f, 0.5f);

        toggle.graphic = checkImg;
        checkImg.enabled = valorInicial;

        return toggle;
    }

    public static TMP_Dropdown CrearDropdownConEtiqueta(Transform padre, string nombre, string etiqueta, Vector2 anclaPos)
    {
        GameObject contenedor = new GameObject(nombre + "_Contenedor");
        contenedor.transform.SetParent(padre, false);
        RectTransform rtCont = contenedor.AddComponent<RectTransform>();
        rtCont.anchorMin = anclaPos;
        rtCont.anchorMax = anclaPos;
        rtCont.pivot = new Vector2(0.5f, 0.5f);
        rtCont.sizeDelta = new Vector2(340, 50);

        CrearTextoUI(contenedor.transform, "Etiqueta", etiqueta, 18, new Vector2(0.5f, 0.8f), new Vector2(340, 24), TextAlignmentOptions.Left, Color.white);

        GameObject dropGO = new GameObject(nombre);
        dropGO.transform.SetParent(contenedor.transform, false);
        RectTransform rtDrop = dropGO.AddComponent<RectTransform>();
        rtDrop.anchorMin = new Vector2(0.05f, 0.1f);
        rtDrop.anchorMax = new Vector2(0.95f, 0.55f);
        rtDrop.sizeDelta = Vector2.zero;

        Image imgBg = dropGO.AddComponent<Image>();
        imgBg.color = new Color(0.15f, 0.22f, 0.35f);

        TMP_Dropdown dropdown = dropGO.AddComponent<TMP_Dropdown>();

        GameObject captionGO = CrearTextoUI(dropGO.transform, "CaptionText", "Opción", 16, new Vector2(0.45f, 0.5f), new Vector2(250, 25), TextAlignmentOptions.Left, Color.white);
        dropdown.captionText = captionGO.GetComponent<TextMeshProUGUI>();

        return dropdown;
    }

    #endregion
}
