using UnityEngine;

/// <summary>
/// Cámara en tercera persona con órbita por ratón, sensibilidad desde AjustesManager
/// y evitación de colisiones con el entorno.
/// </summary>
public class CameraController : MonoBehaviour
{
    [Header("Objetivo")]
    public Transform objetivo;
    public Vector3 offsetObjetivo = new Vector3(0f, 1.6f, 0f);

    [Header("Distancias")]
    public float distancia = 4.5f;
    public float distanciaMinima = 1.5f;
    public float distanciaMaxima = 7.0f;

    [Header("Ángulos")]
    public float pitchMin = -15f;
    public float pitchMax = 65f;

    [Header("Suavizado")]
    public float suavizadoMovimiento = 10f;
    public float suavizadoRotacion = 15f;

    [Header("Colisión")]
    public LayerMask capasColision = ~0;
    public float radioColision = 0.25f;

    private float rotacionX = 20f;
    private float rotacionY = 0f;
    private float distanciaActual;

    private void Start()
    {
        distanciaActual = distancia;
        if (objetivo == null)
        {
            GameObject jugador = GameObject.FindGameObjectWithTag("Player");
            if (jugador != null)
            {
                objetivo = jugador.transform;
            }
        }

        Vector3 angulos = transform.eulerAngles;
        rotacionX = angulos.x;
        rotacionY = angulos.y;
    }

    private void LateUpdate()
    {
        if (objetivo == null) return;

        // No orbitar con ratón si el cursor está desbloqueado (menús o pausa)
        if (Cursor.lockState == CursorLockMode.Locked)
        {
            float sensibilidad = (AjustesManager.Instancia != null) ? AjustesManager.Instancia.SensibilidadRaton : 3.0f;

            float mouseX = Input.GetAxis("Mouse X") * sensibilidad;
            float mouseY = -Input.GetAxis("Mouse Y") * sensibilidad;

            rotacionY += mouseX;
            rotacionX = Mathf.Clamp(rotacionX + mouseY, pitchMin, pitchMax);
        }

        // Rotación objetivo
        Quaternion rotacion = Quaternion.Euler(rotacionX, rotacionY, 0f);
        Vector3 puntoObjetivo = objetivo.position + offsetObjetivo;

        // Evitación de colisiones con el entorno
        Vector3 direccionDeseada = rotacion * -Vector3.forward;
        float distanciaDeseada = distancia;

        RaycastHit hit;
        if (Physics.SphereCast(puntoObjetivo, radioColision, direccionDeseada, out hit, distancia, capasColision, QueryTriggerInteraction.Ignore))
        {
            distanciaDeseada = Mathf.Clamp(hit.distance, distanciaMinima, distancia);
        }

        distanciaActual = Mathf.Lerp(distanciaActual, distanciaDeseada, Time.deltaTime * suavizadoMovimiento);

        Vector3 posicionFinal = puntoObjetivo + direccionDeseada * distanciaActual;

        transform.position = Vector3.Lerp(transform.position, posicionFinal, Time.deltaTime * suavizadoMovimiento);
        transform.rotation = Quaternion.Slerp(transform.rotation, rotacion, Time.deltaTime * suavizadoRotacion);
    }
}
