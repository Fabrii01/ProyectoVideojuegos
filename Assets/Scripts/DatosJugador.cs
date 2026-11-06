using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Progreso persistente del jugador: monedas, objetos comprados y objetos equipados.
/// Se guarda con PlayerPrefs, así que se conserva entre escenas y entre partidas.
/// </summary>
public static class DatosJugador
{
    private const string CLAVE_MONEDAS = "Rescate_Monedas";
    private const string CLAVE_COMPRADOS = "Rescate_Comprados";
    private const string CLAVE_EQUIPADOS = "Rescate_Equipados";

    /// <summary>Se dispara cuando cambian las monedas, las compras o el equipo.</summary>
    public static event Action OnCambio;

    private static bool cargado;
    private static int monedas;
    private static readonly HashSet<string> comprados = new HashSet<string>();
    private static readonly HashSet<string> equipados = new HashSet<string>();

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ReiniciarEstadoEstatico()
    {
        cargado = false;
        OnCambio = null;
    }

    public static int Monedas
    {
        get { Cargar(); return monedas; }
    }

    public static void AgregarMonedas(int cantidad)
    {
        Cargar();
        monedas = Mathf.Max(0, monedas + cantidad);
        Guardar();
    }

    public static bool EstaComprado(string id)
    {
        Cargar();
        return comprados.Contains(id);
    }

    public static bool EstaEquipado(string id)
    {
        Cargar();
        return equipados.Contains(id);
    }

    /// <summary>Compra y equipa el objeto. Devuelve false si no alcanza el dinero o ya se tenía.</summary>
    public static bool Comprar(string id, int precio)
    {
        Cargar();
        if (comprados.Contains(id) || monedas < precio) return false;

        monedas -= precio;
        comprados.Add(id);
        equipados.Add(id);
        Guardar();
        return true;
    }

    public static void AlternarEquipado(string id)
    {
        Cargar();
        if (!comprados.Contains(id)) return;

        if (!equipados.Remove(id))
        {
            equipados.Add(id);
        }
        Guardar();
    }

    public static void BorrarProgreso()
    {
        PlayerPrefs.DeleteKey(CLAVE_MONEDAS);
        PlayerPrefs.DeleteKey(CLAVE_COMPRADOS);
        PlayerPrefs.DeleteKey(CLAVE_EQUIPADOS);
        PlayerPrefs.Save();
        cargado = false;
        Cargar();
        OnCambio?.Invoke();
    }

    private static void Cargar()
    {
        if (cargado) return;
        cargado = true;

        monedas = PlayerPrefs.GetInt(CLAVE_MONEDAS, 0);
        LeerLista(CLAVE_COMPRADOS, comprados);
        LeerLista(CLAVE_EQUIPADOS, equipados);
    }

    private static void LeerLista(string clave, HashSet<string> destino)
    {
        destino.Clear();
        string texto = PlayerPrefs.GetString(clave, string.Empty);
        foreach (string id in texto.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries))
        {
            destino.Add(id);
        }
    }

    private static void Guardar()
    {
        PlayerPrefs.SetInt(CLAVE_MONEDAS, monedas);
        PlayerPrefs.SetString(CLAVE_COMPRADOS, string.Join(";", comprados));
        PlayerPrefs.SetString(CLAVE_EQUIPADOS, string.Join(";", equipados));
        PlayerPrefs.Save();
        OnCambio?.Invoke();
    }
}
