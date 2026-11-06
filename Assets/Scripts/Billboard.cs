using UnityEngine;

/// <summary>Mantiene un texto del mundo siempre de frente a la cámara.</summary>
public class Billboard : MonoBehaviour
{
    private void LateUpdate()
    {
        Camera camara = Camera.main;
        if (camara != null) transform.rotation = camara.transform.rotation;
    }
}
