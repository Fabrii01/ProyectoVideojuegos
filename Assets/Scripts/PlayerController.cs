using UnityEngine;
using TMPro;
using UnityEngine.UI; // Nueva librería necesaria para el Slider
using UnityEngine.SceneManagement;

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
        // Inicializar primero las físicas del jugador
        rb = GetComponent<Rigidbody>();

        // La interfaz es opcional en el Hub
        if (panelPausa != null)
        {
            panelPausa.SetActive(false);
        }

        vidaActual = vidaMaxima;

        if (barraDeVida != null)
        {
            barraDeVida.maxValue = vidaMaxima;
            barraDeVida.value = vidaActual;
        }
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
        if (panelPausa != null)
        {
            panelPausa.SetActive(true);
        }

        Time.timeScale = 0f;
        juegoPausado = true;
    }

    public void ReanudarJuego()
    {
        if (panelPausa != null)
        {
            panelPausa.SetActive(false);
        }

        Time.timeScale = 1f;
        juegoPausado = false;
    }


    public void SalirJuego()
    {
        Debug.Log("Saliendo del simulador médico...");
        Application.Quit(); // Esta función cierra el juego cuando está exportado
    }
    public void VolverAlHub()
    {
        // Es vital restaurar el tiempo a 1 antes de cambiar de escena, 
        // de lo contrario, el menú principal cargaría congelado.
        Time.timeScale = 1f; 
        
        // Cargamos la escena inicial usando su nombre exacto
        SceneManager.LoadScene("Hub");
    }

    // Detección de colisiones físicas (chocar contra paredes, suelo u obstáculos)
    
    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.name == "Terrain" ||
            collision.gameObject.name == "SueloHub")
        {
            enElSuelo = true;
        }

        if (collision.gameObject.CompareTag("Peligro"))
        {
            vidaActual = Mathf.Max(0, vidaActual - 20);

            if (barraDeVida != null)
                barraDeVida.value = vidaActual;

            Debug.Log("¡Recibiste daño! Vida actual: " + vidaActual);
        }
    }


    // Detección para coger objetos (triggers)
    void OnTriggerEnter(Collider otroObjeto)
    {
        if (otroObjeto.gameObject.CompareTag("Suministro"))
        {
            cantidadSuministros++; 
            if (textoSuministros != null)
            {
                textoSuministros.text = "Suministros: " + cantidadSuministros;
            }
            Destroy(otroObjeto.gameObject);
            Debug.Log("¡Suministro médico recogido!");
        }
    }
}