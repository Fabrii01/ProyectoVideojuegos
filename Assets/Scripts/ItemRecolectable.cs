using UnityEngine;

public enum TipoItemSalud
{
    Jeringa,
    EquipajeSalud,
    Botiquin,
    Medicina
}

/// <summary>
/// Comportamiento para elementos recolectables de salud (Jeringas, Equipajes, Botiquines, etc.).
/// Proporciona animación de levitación, rotación continua, luz de aura y retroalimentación al jugador.
/// </summary>
[RequireComponent(typeof(Collider))]
public class ItemRecolectable : MonoBehaviour
{
    [Header("Configuración del Ítem")]
    public TipoItemSalud tipoItem = TipoItemSalud.Jeringa;
    public int valorSuministro = 1;
    public int cantidadCuracion = 15;

    [Header("Efectos Visuales")]
    public float velocidadRotacion = 90f;
    public float amplitudFlotacion = 0.25f;
    public float frecuenciaFlotacion = 2.5f;
    public Light luzAura;
    public Color colorAura = new Color(0.2f, 0.8f, 1f, 1f);

    private Vector3 posicionInicial;
    private float offsetTiempoAleatorio;
    private bool recolectado = false;

    private void Start()
    {
        posicionInicial = transform.position;
        offsetTiempoAleatorio = Random.Range(0f, 10f);

        // Asegurar que el collider esté configurado como Trigger
        Collider col = GetComponent<Collider>();
        if (col != null)
        {
            col.isTrigger = true;
        }

        // Si no tiene luz y se desea, se crea una luz suave de aura
        if (luzAura == null)
        {
            GameObject luzGO = new GameObject("LuzAura");
            luzGO.transform.SetParent(transform);
            luzGO.transform.localPosition = Vector3.up * 0.2f;
            luzAura = luzGO.AddComponent<Light>();
            luzAura.type = LightType.Point;
            luzAura.range = 2.5f;
            luzAura.intensity = 1.2f;

            switch (tipoItem)
            {
                case TipoItemSalud.Jeringa:
                    colorAura = new Color(0.1f, 0.9f, 1f); // Azul cian médico
                    break;
                case TipoItemSalud.EquipajeSalud:
                    colorAura = new Color(1f, 0.85f, 0.2f); // Ámbar dorado de rescate
                    break;
                case TipoItemSalud.Botiquin:
                    colorAura = new Color(0.2f, 1f, 0.4f); // Verde vida
                    break;
                case TipoItemSalud.Medicina:
                    colorAura = new Color(1f, 0.3f, 0.4f); // Rojo salud
                    break;
            }

            luzAura.color = colorAura;
        }
    }

    private void Update()
    {
        if (recolectado) return;

        // 1. Rotación continua
        transform.Rotate(Vector3.up, velocidadRotacion * Time.deltaTime, Space.World);

        // 2. Flotación senoidal
        float nuevoY = posicionInicial.y + Mathf.Sin((Time.time + offsetTiempoAleatorio) * frecuenciaFlotacion) * amplitudFlotacion;
        transform.position = new Vector3(transform.position.x, nuevoY, transform.position.z);

        // 3. Pulsación sutil de luz
        if (luzAura != null)
        {
            luzAura.intensity = 1.0f + 0.4f * Mathf.Sin(Time.time * 3f);
        }
    }

    private void OnTriggerEnter(Collider otro)
    {
        if (recolectado) return;

        if (otro.CompareTag("Player"))
        {
            PlayerController jugador = otro.GetComponent<PlayerController>();
            if (jugador != null)
            {
                recolectado = true;
                jugador.RecolectarItemSalud(this);
                ReproducirAudio();
                CrearEfectoDesaparicion();
                Destroy(gameObject);
            }
        }
    }

    private void ReproducirAudio()
    {
        if (AudioManager.Instancia == null) return;

        switch (tipoItem)
        {
            case TipoItemSalud.Jeringa:
                AudioManager.Instancia.PlayJeringa();
                break;
            case TipoItemSalud.EquipajeSalud:
                AudioManager.Instancia.PlayEquipaje();
                break;
            case TipoItemSalud.Botiquin:
                AudioManager.Instancia.PlayBotiquin();
                break;
            case TipoItemSalud.Medicina:
                AudioManager.Instancia.PlayCurar();
                break;
        }
    }

    private void CrearEfectoDesaparicion()
    {
        // Pequeño destello visual que desaparece
        GameObject flashGO = new GameObject("DestelloRecoleccion");
        flashGO.transform.position = transform.position;
        Light flash = flashGO.AddComponent<Light>();
        flash.type = LightType.Point;
        flash.range = 4f;
        flash.intensity = 2.5f;
        flash.color = colorAura;
        Destroy(flashGO, 0.3f);
    }
}
