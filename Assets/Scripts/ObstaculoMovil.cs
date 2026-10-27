using UnityEngine;

/// <summary>
/// Obstáculo móvil con patrullaje lateral, velocidades variables y baliza de peligro.
/// Causa daño al jugador al impactar.
/// </summary>
public class ObstaculoMovil : MonoBehaviour
{
    [Header("Patrullaje")]
    public float distancia = 5f;
    public float velocidadBase = 3.5f;
    public Vector3 direccionPatrullaje = Vector3.right;

    [Header("Peligro y Efectos")]
    public int danioAlJugador = 20;
    public Light balizaAlerta;
    public Color colorAlerta = new Color(1f, 0.35f, 0f);

    private Vector3 limiteA;
    private Vector3 limiteB;
    private Vector3 destinoActual;
    private float velocidadActual;

    private void Start()
    {
        gameObject.tag = "Peligro";

        Vector3 offset = direccionPatrullaje.normalized * distancia;
        limiteA = transform.position - offset;
        limiteB = transform.position + offset;

        destinoActual = limiteB;
        velocidadActual = velocidadBase;

        if (balizaAlerta == null)
        {
            GameObject luzGO = new GameObject("BalizaPeligro");
            luzGO.transform.SetParent(transform);
            luzGO.transform.localPosition = Vector3.up * 0.8f;
            balizaAlerta = luzGO.AddComponent<Light>();
            balizaAlerta.type = LightType.Point;
            balizaAlerta.range = 3.5f;
            balizaAlerta.intensity = 1.8f;
            balizaAlerta.color = colorAlerta;
        }
    }

    private void Update()
    {
        transform.position = Vector3.MoveTowards(transform.position, destinoActual, velocidadActual * Time.deltaTime);

        if (Vector3.Distance(transform.position, destinoActual) < 0.1f)
        {
            destinoActual = (destinoActual == limiteB) ? limiteA : limiteB;
            velocidadActual = Random.Range(velocidadBase * 0.8f, velocidadBase * 1.6f);
        }

        if (balizaAlerta != null)
        {
            balizaAlerta.intensity = 1.0f + 1.2f * Mathf.PingPong(Time.time * 4f, 1f);
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            PlayerController jugador = collision.gameObject.GetComponent<PlayerController>();
            if (jugador != null)
            {
                jugador.RecibirDanio(danioAlJugador);
            }
        }
    }
}
