using System.Collections;
using UnityEngine;

/// <summary>
/// Cada cierto tiempo cae un meteorito cerca del jugador. Primero marca el suelo
/// para que alcance a apartarse y al impactar hiere a quien siga dentro.
/// </summary>
public class LluviaMeteoritos : MonoBehaviour
{
    public GameObject meteorito;
    public GameObject marcador;
    public float esperaMinima = 6f;
    public float esperaMaxima = 11f;
    public float distanciaAlJugador = 9f;
    public float radioImpacto = 3f;
    public float aviso = 1.8f;
    public int danio = 25;

    private PlayerController jugador;
    private float proximo;

    private void Start()
    {
        jugador = FindAnyObjectByType<PlayerController>();
        proximo = Time.time + esperaMaxima;
    }

    private void Update()
    {
        if (jugador == null || Time.time < proximo) return;

        proximo = Time.time + Random.Range(esperaMinima, esperaMaxima);

        Vector2 azar = Random.insideUnitCircle * distanciaAlJugador;
        Vector3 punto = jugador.transform.position + new Vector3(azar.x, 0f, azar.y);
        punto.y = 0.05f;
        StartCoroutine(Caer(punto));
    }

    private IEnumerator Caer(Vector3 punto)
    {
        GameObject marca = Instantiate(marcador, punto, Quaternion.identity);
        yield return new WaitForSeconds(aviso * 0.6f);

        GameObject roca = Instantiate(meteorito, punto + Vector3.up * 30f, Random.rotation);
        float tiempo = 0f;
        float duracion = aviso * 0.4f;

        while (tiempo < duracion)
        {
            tiempo += Time.deltaTime;
            roca.transform.position = Vector3.Lerp(punto + Vector3.up * 30f, punto, tiempo / duracion);
            yield return null;
        }

        Destroy(marca);
        Destroy(roca, 6f);

        if (AudioManager.Instancia != null) AudioManager.Instancia.PlayGolpe();

        if (jugador != null && Vector3.Distance(punto, jugador.transform.position) <= radioImpacto + 1f)
        {
            jugador.RecibirDanio(danio);
        }
    }
}
