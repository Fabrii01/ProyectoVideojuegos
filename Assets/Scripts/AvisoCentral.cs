using TMPro;
using UnityEngine;

/// <summary>Mensaje breve en el centro de la pantalla, como "Enemigo encontrado".</summary>
public class AvisoCentral : MonoBehaviour
{
    public static AvisoCentral Instancia { get; private set; }

    public CanvasGroup grupo;
    public TextMeshProUGUI texto;
    public float duracion = 1.8f;
    public float fundido = 0.3f;
    public float esperaEntreAvisos = 4f;

    private float inicio = float.NegativeInfinity;

    public bool Visible => grupo != null && grupo.alpha > 0f;

    private void Awake()
    {
        Instancia = this;
        grupo.alpha = 0f;
    }

    private void OnDestroy()
    {
        if (Instancia == this) Instancia = null;
    }

    /// <summary>Si varios enemigos te ven a la vez, solo avisa una vez.</summary>
    public void Mostrar(string mensaje)
    {
        if (Time.time - inicio < esperaEntreAvisos) return;

        inicio = Time.time;
        texto.text = mensaje;
    }

    private void Update()
    {
        float pasado = Time.time - inicio;
        float entrada = Mathf.Clamp01(pasado / fundido);
        float salida = Mathf.Clamp01((duracion - pasado) / fundido);
        grupo.alpha = Mathf.Min(entrada, salida);
    }
}
