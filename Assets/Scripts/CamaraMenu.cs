using UnityEngine;

/// <summary>Movimiento lento de la cámara del menú para que el fondo se sienta vivo.</summary>
public class CamaraMenu : MonoBehaviour
{
    public float amplitudGrados = 3f;
    public float amplitudAltura = 0.15f;
    public float velocidad = 0.25f;

    private Vector3 posicionBase;
    private Quaternion rotacionBase;

    private void Start()
    {
        posicionBase = transform.position;
        rotacionBase = transform.rotation;
    }

    private void Update()
    {
        float t = Time.unscaledTime * velocidad;
        transform.rotation = rotacionBase * Quaternion.Euler(0f, Mathf.Sin(t) * amplitudGrados, 0f);
        transform.position = posicionBase + Vector3.up * (Mathf.Sin(t * 0.7f) * amplitudAltura);
    }
}
