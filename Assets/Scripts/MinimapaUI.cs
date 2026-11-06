using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Mapa del nivel en la esquina superior derecha. Muestra al jugador, los enemigos,
/// los suministros, las monedas y al paciente. Las marcas desaparecen al recoger cada cosa.
/// </summary>
public class MinimapaUI : MonoBehaviour
{
    private class Marca
    {
        public Transform objetivo;
        public RectTransform icono;
        public EnemigoPatrulla enemigo;
    }

    public RectTransform area;
    public PlayerController jugador;
    public Vector2 mundoMin;
    public Vector2 mundoMax;

    [Header("Iconos")]
    public Sprite spritePunto;
    public Sprite spriteFlecha;
    public Sprite spriteCruz;

    [Header("Colores")]
    public Color colorJugador = Color.white;
    public Color colorEnemigo = new Color(1f, 0.2f, 0.16f);
    public Color colorJeringa = new Color(0.3f, 0.85f, 1f);
    public Color colorEquipaje = new Color(1f, 0.6f, 0.15f);
    public Color colorBotiquin = new Color(0.3f, 1f, 0.45f);
    public Color colorMoneda = new Color(1f, 0.85f, 0.25f);

    private readonly List<Marca> marcas = new List<Marca>();
    private RectTransform flechaJugador;

    private void Start()
    {
        foreach (MonedaRecolectable moneda in FindObjectsByType<MonedaRecolectable>(FindObjectsInactive.Exclude))
        {
            Agregar(moneda.transform, spritePunto, colorMoneda, 6f);
        }

        foreach (ItemRecolectable item in FindObjectsByType<ItemRecolectable>(FindObjectsInactive.Exclude))
        {
            Agregar(item.transform, spritePunto, ColorDeItem(item.tipoItem), 12f);
        }

        Paciente paciente = FindAnyObjectByType<Paciente>();
        if (paciente != null) Agregar(paciente.transform, spriteCruz, Color.white, 20f);

        foreach (EnemigoPatrulla enemigo in FindObjectsByType<EnemigoPatrulla>(FindObjectsInactive.Exclude))
        {
            Agregar(enemigo.transform, spritePunto, colorEnemigo, 14f).enemigo = enemigo;
        }

        flechaJugador = CrearIcono("Jugador", spriteFlecha, colorJugador, 20f);
    }

    private Color ColorDeItem(TipoItemSalud tipo)
    {
        switch (tipo)
        {
            case TipoItemSalud.Jeringa: return colorJeringa;
            case TipoItemSalud.EquipajeSalud: return colorEquipaje;
            default: return colorBotiquin;
        }
    }

    private Marca Agregar(Transform objetivo, Sprite sprite, Color color, float tamano)
    {
        Marca marca = new Marca
        {
            objetivo = objetivo,
            icono = CrearIcono(objetivo.name, sprite, color, tamano)
        };
        marcas.Add(marca);
        return marca;
    }

    private RectTransform CrearIcono(string nombre, Sprite sprite, Color color, float tamano)
    {
        GameObject go = new GameObject("Icono_" + nombre, typeof(RectTransform));
        go.transform.SetParent(area, false);

        Image imagen = go.AddComponent<Image>();
        imagen.sprite = sprite;
        imagen.color = color;
        imagen.raycastTarget = false;

        RectTransform rect = (RectTransform)go.transform;
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.zero;
        rect.sizeDelta = Vector2.one * tamano;
        return rect;
    }

    private void LateUpdate()
    {
        for (int i = marcas.Count - 1; i >= 0; i--)
        {
            Marca marca = marcas[i];

            // Si el objeto ya no existe es porque se recogió o porque cayó el enemigo.
            if (marca.objetivo == null || (marca.enemigo != null && marca.enemigo.EstaDerrotado))
            {
                Destroy(marca.icono.gameObject);
                marcas.RemoveAt(i);
                continue;
            }

            marca.icono.anchoredPosition = APosicionMapa(marca.objetivo.position);

            if (marca.enemigo != null)
            {
                float pulso = marca.enemigo.EstaPersiguiendo ? 1.25f + Mathf.PingPong(Time.time * 3f, 0.5f) : 1f;
                marca.icono.localScale = Vector3.one * pulso;
            }
        }

        if (jugador != null && flechaJugador != null)
        {
            flechaJugador.anchoredPosition = APosicionMapa(jugador.transform.position);

            Transform modelo = jugador.modeloVisualDoctor != null ? jugador.modeloVisualDoctor : jugador.transform;
            flechaJugador.localRotation = Quaternion.Euler(0f, 0f, -modelo.eulerAngles.y);
        }
    }

    private Vector2 APosicionMapa(Vector3 mundo)
    {
        float u = Mathf.InverseLerp(mundoMin.x, mundoMax.x, mundo.x);
        float v = Mathf.InverseLerp(mundoMin.y, mundoMax.y, mundo.z);
        return new Vector2(u * area.rect.width, v * area.rect.height);
    }
}
