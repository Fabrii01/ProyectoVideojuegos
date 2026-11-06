using UnityEngine;

/// <summary>Sonidos sueltos del Hub, como pájaros de fondo.</summary>
public class AmbienteHub : MonoBehaviour
{
    public float esperaMinima = 3f;
    public float esperaMaxima = 9f;

    private float proximoCanto;

    private void Start()
    {
        proximoCanto = Time.time + esperaMinima;
    }

    private void Update()
    {
        if (Time.time < proximoCanto) return;

        proximoCanto = Time.time + Random.Range(esperaMinima, esperaMaxima);
        if (AudioManager.Instancia != null) AudioManager.Instancia.PlayPajaro();
    }
}
