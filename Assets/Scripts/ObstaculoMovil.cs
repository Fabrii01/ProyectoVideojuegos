using UnityEngine;

public class ObstaculoMovil : MonoBehaviour
{
    public float distancia = 4f; // Qué tan lejos viaja hacia los lados
    public float velocidadBase = 3f; // Velocidad inicial
    
    private Vector3 limiteIzquierdo;
    private Vector3 limiteDerecho;
    private Vector3 destinoActual;
    private float velocidadActual;

    void Start()
    {
        // Calculamos los límites del patrullaje usando la posición inicial del cubo en la escena
        limiteIzquierdo = transform.position + Vector3.left * distancia;
        limiteDerecho = transform.position + Vector3.right * distancia;
        
        destinoActual = limiteDerecho; // Comienza moviéndose a la derecha
        velocidadActual = velocidadBase;
    }

    void Update()
    {
        // Movemos el obstáculo constantemente hacia su destino
        transform.position = Vector3.MoveTowards(transform.position, destinoActual, velocidadActual * Time.deltaTime);

        // Si el cubo llega a su destino (con un margen de 0.1 para evitar fallos de precisión)
        if (Vector3.Distance(transform.position, destinoActual) < 0.1f)
        {
            // Cambiamos la dirección hacia el lado contrario
            if (destinoActual == limiteDerecho)
            {
                destinoActual = limiteIzquierdo;
            }
            else
            {
                destinoActual = limiteDerecho;
            }

            // REQUISITO DEL SÍLABO: Comportamientos aleatorios
            // Asignamos una nueva velocidad al azar cada vez que da la vuelta para hacerlo impredecible
            velocidadActual = Random.Range(2f, 7f); 
        }
    }
}