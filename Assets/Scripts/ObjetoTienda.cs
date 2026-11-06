using UnityEngine;

public enum PuntoEquipo
{
    Cabeza,
    Pecho,
    Espalda,
    Cintura,
    ManoDerecha,
    ManoIzquierda
}

/// <summary>
/// Un objeto que se vende en la tienda del Hub y se coloca sobre el modelo del protagonista.
/// Posición y rotación se miden respecto al personaje: +Z es su frente y +Y es arriba.
/// </summary>
[CreateAssetMenu(fileName = "ObjetoTienda", menuName = "Rescate/Objeto de tienda")]
public class ObjetoTienda : ScriptableObject
{
    [Header("Datos de venta")]
    public string id;
    public string nombre;
    [TextArea(2, 3)] public string descripcion;
    public int precio = 30;
    public Color colorIcono = Color.white;

    [Header("Modelo sobre el personaje")]
    public GameObject prefabVisual;
    public PuntoEquipo punto = PuntoEquipo.Espalda;
    public Vector3 posicion;
    public Vector3 rotacion;
    public float escala = 1f;

    [Header("Bonificaciones")]
    public int vidaExtra;
    public float velocidadExtra;
}
