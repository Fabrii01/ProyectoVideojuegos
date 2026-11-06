using UnityEngine;

/// <summary>
/// Caja de emergencia sellada. Solo se abre con la palanca de rescate equipada
/// y guarda monedas y un botiquín.
/// </summary>
public class CajaSellada : MonoBehaviour
{
    private const string ID_PALANCA = "palanca";

    public int monedas = 12;
    public GameObject botiquin;
    public float escalaPremio = 2.4f;
    public GameObject sellos;
    public Light luz;

    private bool abierta;
    private bool cerca;

    public bool Abierta => abierta;

    private static bool TienePalanca => DatosJugador.EstaEquipado(ID_PALANCA);

    private void OnTriggerEnter(Collider otro)
    {
        if (abierta || !otro.CompareTag("Player")) return;

        cerca = true;
        Avisar(TienePalanca ? "Pulsa E para forzarla con la palanca." : "Está sellada. Necesitas la palanca de rescate de la tienda.");
    }

    private void OnTriggerExit(Collider otro)
    {
        if (!otro.CompareTag("Player")) return;

        cerca = false;
        if (DialogoUI.Instancia != null) DialogoUI.Instancia.Ocultar(this);
    }

    private void Update()
    {
        if (cerca && !abierta && Time.timeScale > 0f && Input.GetKeyDown(KeyCode.E)) Intentar();
    }

    /// <summary>Abre la caja si el jugador lleva la palanca.</summary>
    public void Intentar()
    {
        if (abierta || !TienePalanca) return;

        abierta = true;
        DatosJugador.AgregarMonedas(monedas);

        if (sellos != null) sellos.SetActive(false);
        if (luz != null) luz.enabled = false;

        if (botiquin != null)
        {
            // Sobre la caja, grande como los demás recolectables.
            GameObject premio = Instantiate(botiquin, transform.position + Vector3.up * 2.2f, Quaternion.identity);
            premio.transform.localScale = Vector3.one * escalaPremio;
        }

        if (AudioManager.Instancia != null) AudioManager.Instancia.PlayCompra();

        Avisar($"Caja abierta: +{monedas} monedas y un botiquín.");
    }

    private void Avisar(string texto)
    {
        if (DialogoUI.Instancia != null) DialogoUI.Instancia.Mostrar(this, "Caja de emergencia", texto);
    }
}
