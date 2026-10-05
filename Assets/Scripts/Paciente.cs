using UnityEngine;

public class Paciente : MonoBehaviour
{
    public int suministrosRequeridos = 3; // Lo que pide este paciente para curarse

    void OnTriggerEnter(Collider otro)
    {
        // Verificamos si el que entró a la zona es el jugador usando la etiqueta que le pusimos
        if (otro.CompareTag("Player"))
        {
            // Accedemos al script del jugador para leer su variable de suministros
            PlayerController jugador = otro.GetComponent<PlayerController>();
            
            if (jugador != null && jugador.cantidadSuministros >= suministrosRequeridos)
            {
                Debug.Log("¡Paciente curado! Nivel completado.");
                // Más adelante programaremos que esto te regrese al Hub automáticamente
            }
            else
            {
                Debug.Log("Aún te faltan suministros. Necesito: " + suministrosRequeridos);
            }
        }
    }
}