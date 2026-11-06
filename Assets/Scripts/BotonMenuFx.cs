using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>Agranda un poco el botón al pasar el ratón. Funciona también con el juego en pausa.</summary>
public class BotonMenuFx : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public float escalaHover = 1.04f;
    public float velocidad = 14f;

    private Vector3 escalaBase;
    private float objetivo = 1f;

    private void Awake()
    {
        escalaBase = transform.localScale;
    }

    private void OnDisable()
    {
        objetivo = 1f;
        transform.localScale = escalaBase;
    }

    private void Update()
    {
        transform.localScale = Vector3.Lerp(transform.localScale, escalaBase * objetivo, Time.unscaledDeltaTime * velocidad);
    }

    public void OnPointerEnter(PointerEventData datos)
    {
        objetivo = escalaHover;
    }

    public void OnPointerExit(PointerEventData datos)
    {
        objetivo = 1f;
    }
}
