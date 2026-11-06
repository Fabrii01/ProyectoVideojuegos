using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public static class SaludGameBuilder
{
    private const string PATH_DOCTOR_FBX = "Assets/Models/Personajes/Medico/Doctor.fbx";
    private const string PATH_MAT_BODY = "Assets/Models/Personajes/Medico/textures 2/Materials/M_Doctor_Body.mat";
    private const string PATH_MAT_HEAD = "Assets/Models/Personajes/Medico/textures 2/Materials/M_Doctor_Head.mat";
    private const string PATH_MAT_ITEMS = "Assets/Kabungus/HouseholdItems/Scenes/URP/M_HouseholdItems_URP.mat";

    private const string PATH_SYRINGE_FBX = "Assets/Kabungus/HouseholdItems/Models/Syringe.fbx";
    private const string PATH_BRIEFCASE_FBX = "Assets/Kabungus/HouseholdItems/Models/Briefcase.fbx";
    private const string PATH_MEDKIT_FBX = "Assets/Kabungus/HouseholdItems/Models/Medkit.fbx";

    private const string PATH_PREFAB_JERINGA = "Assets/Prefabs/Recolectables/JeringaRecolectable.prefab";
    private const string PATH_PREFAB_EQUIPAJE = "Assets/Prefabs/Recolectables/EquipajeSaludRecolectable.prefab";
    private const string PATH_PREFAB_BOTIQUIN = "Assets/Prefabs/Recolectables/BotiquinRecolectable.prefab";

    /// <summary>Regenera los prefabs de jeringa, equipaje y botiquín.</summary>
    [MenuItem("SaludGame/Regenerar recolectables")]
    public static void RegenerarRecolectables()
    {
        ConstruirPrefabsRecolectables();
        AssetDatabase.SaveAssets();
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

    /// <summary>Crea el panel de ajustes del menú principal.</summary>
    public static void CrearPanelAjustes(Transform canvas, MenuPrincipalController controller)
    {

        GameObject panelAjustes = new GameObject("PanelAjustes");
        panelAjustes.transform.SetParent(canvas, false);
        RectTransform rtAj = panelAjustes.AddComponent<RectTransform>();
        rtAj.anchorMin = new Vector2(0.15f, 0.08f);
        rtAj.anchorMax = new Vector2(0.85f, 0.92f);
        rtAj.sizeDelta = Vector2.zero;

        Image imgAj = panelAjustes.AddComponent<Image>();
        imgAj.color = new Color(0.06f, 0.08f, 0.11f, 0.98f);
        imgAj.sprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd");
        imgAj.type = Image.Type.Sliced;
        imgAj.pixelsPerUnitMultiplier = 0.45f;

        PanelAjustesUI uiAjustes = panelAjustes.AddComponent<PanelAjustesUI>();
        uiAjustes.panelAjustes = panelAjustes;
        controller.panelAjustes = panelAjustes;

        CrearTextoUI(panelAjustes.transform, "TituloAjustes", "AJUSTES", 42, new Vector2(0.5f, 0.92f), new Vector2(700, 56), TextAlignmentOptions.Center, Color.white).GetComponent<TextMeshProUGUI>().fontStyle = FontStyles.Bold;

        // Audio
        CrearTextoUI(panelAjustes.transform, "HeaderAudio", "AUDIO", 24, new Vector2(0.28f, 0.82f), new Vector2(300, 35), TextAlignmentOptions.Left, Color.white);
        uiAjustes.sliderMaster = CrearSliderConEtiqueta(panelAjustes.transform, "SliderMaster", "Volumen General", 0.8f, new Vector2(0.28f, 0.74f), out uiAjustes.textoMaster);
        uiAjustes.sliderSFX = CrearSliderConEtiqueta(panelAjustes.transform, "SliderSFX", "Efectos (SFX)", 0.9f, new Vector2(0.28f, 0.64f), out uiAjustes.textoSFX);
        uiAjustes.sliderMusica = CrearSliderConEtiqueta(panelAjustes.transform, "SliderMusica", "Música Ambiente", 0.7f, new Vector2(0.28f, 0.54f), out uiAjustes.textoMusica);

        // Video / Pantalla
        CrearTextoUI(panelAjustes.transform, "HeaderVideo", "PANTALLA Y GRÁFICOS", 24, new Vector2(0.72f, 0.82f), new Vector2(300, 35), TextAlignmentOptions.Left, Color.white);
        uiAjustes.dropdownCalidad = CrearDropdownConEtiqueta(panelAjustes.transform, "DropdownCalidad", "Calidad Gráfica", new Vector2(0.72f, 0.74f));
        uiAjustes.togglePantallaCompleta = CrearToggleConEtiqueta(panelAjustes.transform, "TogglePantallaCompleta", "Pantalla Completa", true, new Vector2(0.72f, 0.64f));
        uiAjustes.dropdownResolucion = CrearDropdownConEtiqueta(panelAjustes.transform, "DropdownResolucion", "Resolución de Pantalla", new Vector2(0.72f, 0.54f));

        // Controles
        CrearTextoUI(panelAjustes.transform, "HeaderControles", "CONTROLES", 24, new Vector2(0.5f, 0.42f), new Vector2(500, 35), TextAlignmentOptions.Center, Color.white);
        uiAjustes.sliderSensibilidad = CrearSliderConEtiqueta(panelAjustes.transform, "SliderSensibilidad", "Sensibilidad Ratón", 3.0f, new Vector2(0.35f, 0.32f), out uiAjustes.textoSensibilidad, 0.5f, 10f);
        uiAjustes.toggleInvertirY = CrearToggleConEtiqueta(panelAjustes.transform, "ToggleInvertirY", "Invertir Eje Y", false, new Vector2(0.72f, 0.32f));

        GameObject btnRestablecer = CrearBotonUI(panelAjustes.transform, "BtnRestablecer", "Restablecer valores", 20, new Vector2(0.35f, 0.12f), new Vector2(240, 48), new Color(0.4f, 0.4f, 0.4f));
        uiAjustes.botonRestablecer = btnRestablecer.GetComponent<Button>();

        GameObject btnCerrar = CrearBotonUI(panelAjustes.transform, "BtnCerrar", "Guardar y volver", 22, new Vector2(0.65f, 0.12f), new Vector2(260, 48), new Color(0.15f, 0.6f, 0.4f));
        uiAjustes.botonCerrar = btnCerrar.GetComponent<Button>();
        UnityEditor.Events.UnityEventTools.AddPersistentListener(btnCerrar.GetComponent<Button>().onClick, controller.CerrarAjustes);

        panelAjustes.SetActive(false);
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
