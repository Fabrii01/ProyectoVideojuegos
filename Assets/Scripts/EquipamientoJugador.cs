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

    /// <summary>Objetos que el nivel pone siempre, como el casco espacial.</summary>
    public ObjetoTienda[] objetosFijos;

    private readonly List<GameObject> instancias = new List<GameObject>();
    private PlayerController jugador;
    private AtaqueJugador ataque;

    private void Awake()
    {
        jugador = GetComponent<PlayerController>();
        ataque = GetComponent<AtaqueJugador>();
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
        int danioArma = 0;

        if (catalogo != null)
        {
            foreach (ObjetoTienda objeto in catalogo.objetos)
            {
                if (objeto == null || !DatosJugador.EstaEquipado(objeto.id)) continue;

                vidaExtra += objeto.vidaExtra;
                velocidadExtra += objeto.velocidadExtra;
                danioArma = Mathf.Max(danioArma, objeto.danioArma);
                if (PuntoOcupado(objeto.punto)) continue;

                GameObject instancia = Colocar(jugador.modeloVisualDoctor != null ? jugador.modeloVisualDoctor : transform, objeto);
                if (instancia != null) instancias.Add(instancia);
            }
        }

        if (objetosFijos != null)
        {
            foreach (ObjetoTienda fijo in objetosFijos)
            {
                GameObject instancia = Colocar(jugador.modeloVisualDoctor, fijo);
                if (instancia != null) instancias.Add(instancia);
            }
        }

        jugador.AplicarBonos(vidaExtra, velocidadExtra);
        if (ataque != null) ataque.Danio = danioArma;
    }

    /// <summary>Un objeto fijo del nivel tapa al comprado, pero este conserva sus bonos.</summary>
    private bool PuntoOcupado(PuntoEquipo punto)
    {
        if (objetosFijos == null) return false;

        foreach (ObjetoTienda fijo in objetosFijos)
        {
            if (fijo != null && fijo.punto == punto) return true;
        }
        return false;
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

        // La posición y el giro se guardan relativos al hueso para seguir sus animaciones.
        instancia.transform.rotation = hueso.rotation * Quaternion.Euler(objeto.rotacion);
        instancia.transform.position = hueso.position + hueso.rotation * objeto.posicion;
        instancia.transform.localScale = instancia.transform.localScale * objeto.escala;
        instancia.transform.SetParent(hueso, true);
        return instancia;
    }

    public static string NombreHueso(PuntoEquipo punto)
    {
        switch (punto)
        {
            case PuntoEquipo.Cabeza: return "Head";
            case PuntoEquipo.Pecho: return "Spine2";
            case PuntoEquipo.Espalda: return "Spine2";
            case PuntoEquipo.Cintura: return "Hips";
            case PuntoEquipo.ManoDerecha: return "RightHand";
            default: return "LeftHand";
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
