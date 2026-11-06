using System.Collections.Generic;
using TMPro;
using UnityEngine;

/// <summary>
/// Panel de la tienda del Hub. Crea una fila por cada objeto del catálogo
/// y permite comprar, equipar y quitar.
/// </summary>
public class TiendaUI : MonoBehaviour
{
    public CatalogoTienda catalogo;
    public FilaTienda plantillaFila;
    public Transform contenedorFilas;
    public TextMeshProUGUI textoMensaje;

    [Header("Colores del botón")]
    public Color colorComprar = new Color(0.95f, 0.62f, 0.12f);
    public Color colorEquipar = new Color(0.13f, 0.55f, 0.42f);
    public Color colorQuitar = new Color(0.32f, 0.37f, 0.45f);
    public Color colorBloqueado = new Color(0.2f, 0.23f, 0.28f);

    private readonly Dictionary<ObjetoTienda, FilaTienda> filas = new Dictionary<ObjetoTienda, FilaTienda>();

    private void OnEnable()
    {
        if (plantillaFila != null) plantillaFila.gameObject.SetActive(false);
        if (filas.Count == 0) CrearFilas();

        Avisar("Lo que compres aparece al momento en tu personaje.");
        DatosJugador.OnCambio += Refrescar;
        Refrescar();
    }

    private void OnDisable()
    {
        DatosJugador.OnCambio -= Refrescar;
    }

    private void CrearFilas()
    {
        if (catalogo == null || plantillaFila == null) return;

        foreach (ObjetoTienda objeto in catalogo.objetos)
        {
            if (objeto == null) continue;

            FilaTienda fila = Instantiate(plantillaFila, contenedorFilas);
            fila.name = "Fila_" + objeto.id;
            fila.gameObject.SetActive(true);

            fila.textoNombre.text = objeto.nombre;
            fila.textoDescripcion.text = objeto.descripcion;
            fila.icono.color = objeto.colorIcono;
            fila.textoIcono.text = objeto.nombre.Substring(0, 1);

            ObjetoTienda capturado = objeto;
            fila.boton.onClick.AddListener(() => AlPulsar(capturado));
            filas[objeto] = fila;
        }
    }

    private void Refrescar()
    {
        foreach (KeyValuePair<ObjetoTienda, FilaTienda> par in filas)
        {
            PintarFila(par.Value, par.Key);
        }
    }

    private void PintarFila(FilaTienda fila, ObjetoTienda objeto)
    {
        bool comprado = DatosJugador.EstaComprado(objeto.id);
        bool equipado = DatosJugador.EstaEquipado(objeto.id);
        bool alcanza = DatosJugador.Monedas >= objeto.precio;

        fila.textoPrecio.text = comprado ? "Comprado" : objeto.precio + " monedas";
        fila.boton.interactable = comprado || alcanza;

        if (!comprado)
        {
            fila.textoBoton.text = alcanza ? "COMPRAR" : "FALTAN " + (objeto.precio - DatosJugador.Monedas);
            fila.fondoBoton.color = alcanza ? colorComprar : colorBloqueado;
        }
        else
        {
            fila.textoBoton.text = equipado ? "QUITAR" : "EQUIPAR";
            fila.fondoBoton.color = equipado ? colorQuitar : colorEquipar;
        }
    }

    private void AlPulsar(ObjetoTienda objeto)
    {
        if (DatosJugador.EstaComprado(objeto.id))
        {
            DatosJugador.AlternarEquipado(objeto.id);
            bool equipado = DatosJugador.EstaEquipado(objeto.id);
            Avisar(objeto.nombre + (equipado ? ": equipado." : ": guardado."));
            if (AudioManager.Instancia != null) AudioManager.Instancia.PlayClick();
            return;
        }

        if (DatosJugador.Comprar(objeto.id, objeto.precio))
        {
            Avisar("Compraste: " + objeto.nombre + ".");
            if (AudioManager.Instancia != null) AudioManager.Instancia.PlayCompra();
        }
    }

    private void Avisar(string mensaje)
    {
        if (textoMensaje != null) textoMensaje.text = mensaje;
    }
}
