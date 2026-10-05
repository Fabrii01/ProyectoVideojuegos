using UnityEngine;
using TMPro;
using UnityEngine.UI; // Nueva librería necesaria para el Slider

public class PlayerController : MonoBehaviour
{
    public float velocidadMovimiento = 5f;
    public float fuerzaSalto = 5f;
    public int cantidadSuministros = 0;
    public TextMeshProUGUI textoSuministros;
    public GameObject panelPausa;
    private bool juegoPausado = false;
    // Nuevas variables para la barra de vida de personaje
    public int vidaMaxima = 100;
    public int vidaActual;
    public Slider barraDeVida;

    private Rigidbody rb;
    private bool enElSuelo = true;

    void Start()
    {
        panelPausa.SetActive(false);
        rb = GetComponent<Rigidbody>();
        
        // Inicializamos la vida al empezar el juego
        vidaActual = vidaMaxima;
        barraDeVida.maxValue = vidaMaxima;
        barraDeVida.value = vidaActual;
    }

    void Update()
    {
        float movimientoHorizontal = Input.GetAxis("Horizontal");
        float movimientoVertical = Input.GetAxis("Vertical");

        Vector3 movimiento = new Vector3(movimientoHorizontal, 0.0f, movimientoVertical);
        rb.linearVelocity = new Vector3(movimiento.x * velocidadMovimiento, rb.linearVelocity.y, movimiento.z * velocidadMovimiento);

        if (Input.GetButtonDown("Jump") && enElSuelo)
        {
            rb.AddForce(Vector3.up * fuerzaSalto, ForceMode.Impulse);
            enElSuelo = false;
        }
        // Detectar si presionamos ESC para pausar o reanudar el juego
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (juegoPausado)
            {
                ReanudarJuego();
            }
            else
            {
                PausarJuego();
            }
        }
    }
    public void PausarJuego()
    {
        panelPausa.SetActive(true);
        Time.timeScale = 0f; // Congela el motor de físicas y el tiempo
        juegoPausado = true;
    }

    public void ReanudarJuego()
    {
        panelPausa.SetActive(false);
        Time.timeScale = 1f; // Restaura el tiempo a la normalidad
        juegoPausado = false;
    }

    public void SalirJuego()
    {
        Debug.Log("Saliendo del simulador médico...");
        Application.Quit(); // Esta función cierra el juego cuando está exportado
    }

    // Detección de colisiones físicas (chocar contra paredes, suelo u obstáculos)
    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.name == "Terrain")
        {
            enElSuelo = true;
        }
        
        // Lógica de daño al chocar con un obstáculo
        if (collision.gameObject.CompareTag("Peligro"))
        {
            vidaActual -= 20; // Restamos 20 puntos de vida
            barraDeVida.value = vidaActual; // Actualizamos la interfaz
            Debug.Log("¡Recibiste daño! Vida actual: " + vidaActual);
        }
    }

    // Detección para coger objetos (triggers)
    void OnTriggerEnter(Collider otroObjeto)
    {
        if (otroObjeto.gameObject.CompareTag("Suministro"))
        {
            cantidadSuministros++; 
            textoSuministros.text = "Suministros: " + cantidadSuministros; 
            Destroy(otroObjeto.gameObject);
            Debug.Log("¡Suministro médico recogido!");
        }
    }
}