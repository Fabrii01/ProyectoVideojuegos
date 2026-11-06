using UnityEngine;

/// <summary>Zona que hiere al jugador mientras está dentro, como un incendio.</summary>
[RequireComponent(typeof(Collider))]
public class ZonaPeligro : MonoBehaviour
{
    public int danio = 10;

    private void OnTriggerStay(Collider otro)
    {
        if (!otro.CompareTag("Player")) return;

        PlayerController jugador = otro.GetComponent<PlayerController>();
        if (jugador != null) jugador.RecibirDanio(danio);
    }
}
