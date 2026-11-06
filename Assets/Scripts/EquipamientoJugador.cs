using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Coloca sobre los huesos del protagonista los objetos equipados y aplica sus bonificaciones.
/// Reacciona al momento cuando se compra o se quita algo en la tienda.
/// </summary>
[RequireComponent(typeof(PlayerController))]
public class EquipamientoJugador : MonoBehaviour
{
    public CatalogoTienda catalogo;

    private readonly List<GameObject> instancias = new List<GameObject>();
    private PlayerController jugador;

    private void Awake()
    {
        jugador = GetComponent<PlayerController>();
    }

    private void OnEnable()
    {
        DatosJugador.OnCambio += Refrescar;
        Refrescar();
    }

    private void OnDisable()
    {
        DatosJugador.OnCambio -= Refrescar;
    }

    public void Refrescar()
    {
        foreach (GameObject instancia in instancias)
        {
            if (instancia != null) Destroy(instancia);
        }
        instancias.Clear();

        int vidaExtra = 0;
        float velocidadExtra = 0f;

        if (catalogo != null)
        {
            foreach (ObjetoTienda objeto in catalogo.objetos)
            {
                if (objeto == null || !DatosJugador.EstaEquipado(objeto.id)) continue;

                vidaExtra += objeto.vidaExtra;
                velocidadExtra += objeto.velocidadExtra;
                GameObject instancia = Colocar(jugador.modeloVisualDoctor != null ? jugador.modeloVisualDoctor : transform, objeto);
                if (instancia != null) instancias.Add(instancia);
            }
        }

        jugador.AplicarBonos(vidaExtra, velocidadExtra);
    }

    /// <summary>Crea el objeto y lo ata al hueso que le corresponde en el modelo.</summary>
    public static GameObject Colocar(Transform modelo, ObjetoTienda objeto)
    {
        Transform hueso = BuscarHueso(modelo, NombreHueso(objeto.punto));
        if (objeto.prefabVisual == null || hueso == null) return null;

        GameObject instancia = Instantiate(objeto.prefabVisual);
        instancia.name = objeto.nombre;

        foreach (Collider colision in instancia.GetComponentsInChildren<Collider>(true))
        {
            colision.enabled = false;
        }

        // Se ubica en el espacio del personaje y luego se ata al hueso para que lo siga.
        instancia.transform.rotation = modelo.rotation * Quaternion.Euler(objeto.rotacion);
        instancia.transform.position = hueso.position + modelo.rotation * objeto.posicion;
        instancia.transform.localScale = instancia.transform.localScale * objeto.escala;
        instancia.transform.SetParent(hueso, true);
        return instancia;
    }

    private static string NombreHueso(PuntoEquipo punto)
    {
        switch (punto)
        {
            case PuntoEquipo.Cabeza: return "Bip001 Head";
            case PuntoEquipo.Pecho: return "Bip001 Spine1";
            case PuntoEquipo.Espalda: return "Bip001 Spine1";
            case PuntoEquipo.Cintura: return "Bip001 Pelvis";
            case PuntoEquipo.ManoDerecha: return "Bip001 R Hand";
            default: return "Bip001 L Hand";
        }
    }

    private static Transform BuscarHueso(Transform raiz, string nombre)
    {
        foreach (Transform hijo in raiz.GetComponentsInChildren<Transform>(true))
        {
            if (hijo.name == nombre) return hijo;
        }
        return null;
    }
}
