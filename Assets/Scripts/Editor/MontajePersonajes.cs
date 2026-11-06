using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

/// <summary>
/// Personajes del juego: todos usan el modelo Animated Human de Quaternius (CC0).
/// Sus uniformes son paletas generadas aquí, porque el modelo se colorea por franjas.
/// </summary>
public static partial class MontajeRescate
{
    private const string HUMANO_FBX = "Assets/Externos/Quaternius/AnimatedHuman.fbx";
    private const string RUTA_HUMANO = DIR + "/Humano.controller";
    private const string DIR_PIELES = DIR + "/Pieles";
    private const float ESCALA_HUMANO = 0.335f;

    private static readonly HashSet<string> pielesCreadas = new HashSet<string>();

    /// <summary>Crea un material con los colores de pantalón, camisa, pelo y piel.</summary>
    private static Material Piel(string nombre, string pantalon, string camisa, string pelo, string piel)
    {
        string ruta = DIR_PIELES + "/" + nombre + ".png";

        if (pielesCreadas.Add(nombre))
        {
            Directory.CreateDirectory(DIR_PIELES);

            Texture2D textura = new Texture2D(32, 32, TextureFormat.RGBA32, false);
            for (int y = 0; y < 32; y++)
            {
                string color = y <= 8 ? pantalon : y <= 14 ? camisa : y <= 20 ? pelo : y <= 25 ? "#292929" : piel;
                ColorUtility.TryParseHtmlString(color, out Color franja);
                for (int x = 0; x < 32; x++) textura.SetPixel(x, y, franja);
            }

            File.WriteAllBytes(ruta, textura.EncodeToPNG());
            AssetDatabase.ImportAsset(ruta, ImportAssetOptions.ForceUpdate);

            TextureImporter importador = (TextureImporter)AssetImporter.GetAtPath(ruta);
            importador.filterMode = FilterMode.Point;
            importador.mipmapEnabled = false;
            importador.wrapMode = TextureWrapMode.Clamp;
            importador.textureCompression = TextureImporterCompression.Uncompressed;
            importador.SaveAndReimport();
        }

        Material material = Mat("Piel_" + nombre, Color.white, 0.15f);
        material.SetTexture("_BaseMap", Cargar<Texture2D>(ruta));
        EditorUtility.SetDirty(material);
        return material;
    }

    private static Material PielRescatista => Piel("Rescatista", "#26324A", "#F0781A", "#3A2A1E", "#E5B58A");
    private static Material PielMedico => Piel("Medico", "#DDE4E8", "#F4F7F8", "#7A7A7A", "#C89459");
    private static Material PielParamedico => Piel("Paramedico", "#1E3F5A", "#2FA3C4", "#2B211B", "#E5B58A");
    private static Material PielEnfermera => Piel("Enfermera", "#3C9D8F", "#54BFAE", "#4A2C1A", "#9C7248");
    private static Material PielTendero => Piel("Tendero", "#5A4128", "#3E8E5A", "#2B211B", "#D9A97A");
    private static Material PielBrigadista => Piel("Brigadista", "#3A3026", "#D9B21E", "#3A2A1E", "#E5B58A");
    private static Material PielAstronauta => Piel("Astronauta", "#D7DBE2", "#EEF1F5", "#3A2A1E", "#E5B58A");
    private static Material PielFurtivo => Piel("Furtivo", "#2F3A22", "#4B5A2E", "#1A1A12", "#C89459");
    private static Material PielAndroide => Piel("Androide", "#4A505A", "#7A8290", "#23262C", "#A7B0BC");
    private static Material PielInstructora => Piel("Instructora", "#2B3A67", "#D9E2F2", "#5A3A22", "#C89459");
    private static Material PielSaqueador => Piel("Saqueador", "#2A1518", "#26262B", "#141414", "#D9A97A");

    private static Material PielVecino(int indice)
    {
        switch (indice % 6)
        {
            case 0: return Piel("Vecino1", "#2F5D9B", "#D94F4F", "#3A2A1E", "#FFD19F");
            case 1: return Piel("Vecino2", "#6B6B73", "#E8C24A", "#141414", "#9C7248");
            case 2: return Piel("Vecino3", "#3B3B44", "#7A5BC4", "#7C5823", "#E5B58A");
            case 3: return Piel("Vecino4", "#8A6A3F", "#4FA3D9", "#292929", "#C89459");
            case 4: return Piel("Vecino5", "#22543D", "#E8E2D0", "#A0522D", "#FFD19F");
            default: return Piel("Vecino6", "#2B2F4A", "#E07AA0", "#141414", "#6E4B2E");
        }
    }

    /// <summary>Prepara las animaciones del modelo y crea el controlador que comparten todos.</summary>
    private static void ConfigurarAnimaciones()
    {
        ModelImporter importador = (ModelImporter)AssetImporter.GetAtPath(HUMANO_FBX);
        if (importador.clipAnimations.Length == 0)
        {
            string[] buscados = { "Idle", "Walk", "Run", "Jump", "Punch", "Death", "Working" };
            List<ModelImporterClipAnimation> clips = new List<ModelImporterClipAnimation>();

            foreach (ModelImporterClipAnimation clip in importador.defaultClipAnimations)
            {
                foreach (string nombre in buscados)
                {
                    if (!clip.name.EndsWith("|" + nombre)) continue;

                    clip.name = nombre;
                    clip.loopTime = nombre == "Idle" || nombre == "Walk" || nombre == "Run" || nombre == "Working";
                    clips.Add(clip);
                }
            }

            importador.clipAnimations = clips.ToArray();
            importador.SaveAndReimport();
        }

        AssetDatabase.DeleteAsset(RUTA_HUMANO);
        AnimatorController controlador = AnimatorController.CreateAnimatorControllerAtPath(RUTA_HUMANO);
        controlador.AddParameter("Velocidad", AnimatorControllerParameterType.Float);
        controlador.AddParameter("EnSuelo", AnimatorControllerParameterType.Bool);
        controlador.AddParameter("Trabajando", AnimatorControllerParameterType.Bool);
        controlador.AddParameter("Saltar", AnimatorControllerParameterType.Trigger);
        controlador.AddParameter("Golpear", AnimatorControllerParameterType.Trigger);
        controlador.AddParameter("Morir", AnimatorControllerParameterType.Trigger);

        AnimatorControllerParameter[] parametros = controlador.parameters;
        parametros[1].defaultBool = true;
        controlador.parameters = parametros;

        AnimatorState movimiento = controlador.CreateBlendTreeInController("Movimiento", out BlendTree mezcla, 0);
        mezcla.blendParameter = "Velocidad";
        mezcla.useAutomaticThresholds = false;
        mezcla.AddChild(ClipHumano("Idle"), 0f);
        mezcla.AddChild(ClipHumano("Walk"), 3.6f);
        mezcla.AddChild(ClipHumano("Run"), 8f);

        AnimatorStateMachine maquina = controlador.layers[0].stateMachine;
        maquina.defaultState = movimiento;

        AnimatorState salto = maquina.AddState("Salto");
        salto.motion = ClipHumano("Jump");
        AnimatorState golpe = maquina.AddState("Golpe");
        golpe.motion = ClipHumano("Punch");
        golpe.speed = 1.6f;
        AnimatorState muerte = maquina.AddState("Muerte");
        muerte.motion = ClipHumano("Death");
        muerte.speed = 1.6f;
        AnimatorState trabajo = maquina.AddState("Trabajo");
        trabajo.motion = ClipHumano("Working");

        DesdeCualquiera(maquina, salto, "Saltar", 0.08f);
        DesdeCualquiera(maquina, golpe, "Golpear", 0.05f);
        DesdeCualquiera(maquina, muerte, "Morir", 0.1f);

        AnimatorStateTransition finSalto = salto.AddTransition(movimiento);
        finSalto.hasExitTime = true;
        finSalto.exitTime = 0.55f;
        finSalto.duration = 0.12f;
        finSalto.AddCondition(AnimatorConditionMode.If, 0f, "EnSuelo");

        AnimatorStateTransition finGolpe = golpe.AddTransition(movimiento);
        finGolpe.hasExitTime = true;
        finGolpe.exitTime = 0.85f;
        finGolpe.duration = 0.12f;

        AnimatorStateTransition aTrabajo = movimiento.AddTransition(trabajo);
        aTrabajo.hasExitTime = false;
        aTrabajo.duration = 0.25f;
        aTrabajo.AddCondition(AnimatorConditionMode.If, 0f, "Trabajando");

        AnimatorStateTransition finTrabajo = trabajo.AddTransition(movimiento);
        finTrabajo.hasExitTime = false;
        finTrabajo.duration = 0.25f;
        finTrabajo.AddCondition(AnimatorConditionMode.IfNot, 0f, "Trabajando");
    }

    private static void DesdeCualquiera(AnimatorStateMachine maquina, AnimatorState estado, string disparador, float duracion)
    {
        AnimatorStateTransition transicion = maquina.AddAnyStateTransition(estado);
        transicion.hasExitTime = false;
        transicion.duration = duracion;
        transicion.canTransitionToSelf = false;
        transicion.AddCondition(AnimatorConditionMode.If, 0f, disparador);
    }

    private static AnimationClip ClipHumano(string nombre)
    {
        foreach (Object recurso in AssetDatabase.LoadAllAssetsAtPath(HUMANO_FBX))
        {
            if (recurso is AnimationClip clip && clip.name == nombre) return clip;
        }

        Debug.LogError("[Montaje] No se encontró la animación " + nombre);
        return null;
    }

    /// <summary>Crea un personaje con animaciones. piesY es la altura local de sus pies.</summary>
    private static GameObject CrearPersonaje(Transform padre, string nombre, Material piel, float piesY)
    {
        GameObject modelo = (GameObject)PrefabUtility.InstantiatePrefab(Cargar<GameObject>(HUMANO_FBX), padre);
        modelo.name = nombre;
        modelo.transform.localScale = Vector3.one * ESCALA_HUMANO;
        modelo.transform.localPosition = new Vector3(0f, piesY, 0f);
        modelo.transform.localRotation = Quaternion.identity;

        foreach (SkinnedMeshRenderer parte in modelo.GetComponentsInChildren<SkinnedMeshRenderer>(true))
        {
            parte.sharedMaterial = piel;
            parte.updateWhenOffscreen = true;
        }

        Animator animador = modelo.GetComponent<Animator>();
        if (animador == null) animador = modelo.AddComponent<Animator>();
        animador.runtimeAnimatorController = Cargar<RuntimeAnimatorController>(RUTA_HUMANO);
        animador.applyRootMotion = false;
        return modelo;
    }

    private static Transform Hueso(GameObject modelo, string nombre)
    {
        foreach (Transform hueso in modelo.GetComponentsInChildren<Transform>(true))
        {
            if (hueso.name == nombre) return hueso;
        }
        return null;
    }

    /// <summary>Convierte la posición pensada desde el personaje a la del hueso, usando la postura de reposo.</summary>
    private static void CalcularEnHueso(ObjetoTienda objeto, Vector3 desplazamiento, Vector3 giro)
    {
        GameObject modelo = Object.Instantiate(Cargar<GameObject>(HUMANO_FBX));
        modelo.transform.localScale = Vector3.one * ESCALA_HUMANO;
        ClipHumano("Idle").SampleAnimation(modelo, 0f);

        Transform hueso = Hueso(modelo, EquipamientoJugador.NombreHueso(objeto.punto));
        Quaternion inversa = Quaternion.Inverse(hueso.rotation);
        objeto.posicion = inversa * desplazamiento;
        objeto.rotacion = (inversa * Quaternion.Euler(giro)).eulerAngles;

        Object.DestroyImmediate(modelo);
    }

    /// <summary>Tiende a un personaje con la pose final de la animación de muerte.</summary>
    private static GameObject Tendido(Transform padre, Vector3 centroLocal, Material piel)
    {
        GameObject modelo = CrearPersonaje(padre, "Herido", piel, 0f);
        Animator animador = modelo.GetComponent<Animator>();
        animador.enabled = false;

        AnimationClip muerte = ClipHumano("Death");
        muerte.SampleAnimation(modelo, muerte.length);

        // Se alinea cabeza y cadera con el largo de la camilla.
        Vector3 cadera = Hueso(modelo, "Hips").position;
        Vector3 cabeza = Hueso(modelo, "Head").position;
        Vector3 largo = cabeza - cadera;
        largo.y = 0f;
        modelo.transform.rotation = Quaternion.FromToRotation(largo.normalized, padre.forward) * modelo.transform.rotation;

        Vector3 centro = (Hueso(modelo, "Hips").position + Hueso(modelo, "Head").position) * 0.5f;
        modelo.transform.position += padre.TransformPoint(centroLocal) - centro;
        return modelo;
    }
}
