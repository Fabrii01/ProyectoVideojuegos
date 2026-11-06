using UnityEngine;

/// <summary>Moneda del escenario. Al recogerla se suma al dinero de la tienda.</summary>
[RequireComponent(typeof(Collider))]
public class MonedaRecolectable : MonoBehaviour
{
    public int valor = 5;
    public float velocidadRotacion = 160f;
    public float amplitudFlotacion = 0.12f;

    private Vector3 posicionInicial;
    private float desfase;

    private void Start()
    {
        posicionInicial = transform.position;
        desfase = Random.Range(0f, 6f);
    }

    private void Update()
    {
        transform.Rotate(Vector3.up, velocidadRotacion * Time.deltaTime, Space.World);
        transform.position = posicionInicial + Vector3.up * (Mathf.Sin(Time.time * 2.5f + desfase) * amplitudFlotacion);
    }

    private void OnTriggerEnter(Collider otro)
    {
        if (!otro.CompareTag("Player")) return;

        DatosJugador.AgregarMonedas(valor);
        if (AudioManager.Instancia != null) AudioManager.Instancia.PlayMoneda();
        Destroy(gameObject);
    }
}
