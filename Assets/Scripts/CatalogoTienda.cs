using System.Collections.Generic;
using UnityEngine;

/// <summary>Lista de objetos a la venta. Agrega aquí un ObjetoTienda para que aparezca en la tienda.</summary>
[CreateAssetMenu(fileName = "CatalogoTienda", menuName = "Rescate/Catálogo de tienda")]
public class CatalogoTienda : ScriptableObject
{
    public List<ObjetoTienda> objetos = new List<ObjetoTienda>();
}
