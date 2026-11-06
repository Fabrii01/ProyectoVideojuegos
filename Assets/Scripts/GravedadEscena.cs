using UnityEngine;

/// <summary>Cambia la gravedad mientras dura la escena y la restaura al salir.</summary>
public class GravedadEscena : MonoBehaviour
{
    private static readonly Vector3 GravedadNormal = new Vector3(0f, -9.81f, 0f);

    public float gravedad = -5f;

    private void Awake()
    {
        Physics.gravity = new Vector3(0f, gravedad, 0f);
    }

    private void OnDestroy()
    {
        Physics.gravity = GravedadNormal;
    }
}
