using UnityEngine;
using UnityEditor;

[InitializeOnLoad]
public static class GameSetupUtility
{
    [InitializeOnLoadMethod]
    public static void AlCargar()
    {
        DiagnosticarDoctor();
    }

    [MenuItem("SaludGame/Diagnosticar Modelo Doctor")]
    public static void DiagnosticarDoctor()
    {
        string fbxPath = "Assets/Models/Personajes/Medico/Doctor.fbx";
        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(fbxPath);
        if (prefab == null)
        {
            Debug.LogError("No se encontró el modelo en: " + fbxPath);
            return;
        }

        Debug.Log("--- DIAGNÓSTICO DOCTOR FBX ---");
        Renderer[] renderers = prefab.GetComponentsInChildren<Renderer>(true);
        foreach (var r in renderers)
        {
            Debug.Log($"Renderer: {r.name} ({r.GetType().Name}), Materiales: {r.sharedMaterials.Length}");
            for (int i = 0; i < r.sharedMaterials.Length; i++)
            {
                var mat = r.sharedMaterials[i];
                Debug.Log($"  Slot [{i}]: {(mat != null ? mat.name : "null")}");
            }
        }
    }
}
