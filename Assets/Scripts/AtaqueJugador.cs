using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

/// <summary>
/// Golpes del jugador. Solo funciona con un arma equipada; el daño lo indica
/// EquipamientoJugador. Se ataca con clic izquierdo o con la tecla F.
/// </summary>
[RequireComponent(typeof(PlayerController))]
public class AtaqueJugador : MonoBehaviour
{
    public float alcance = 1.3f;
    public float radio = 1.2f;
    public float esperaEntreGolpes = 0.7f;
    public float retrasoImpacto = 0.3f;
    public TextMeshProUGUI textoAyuda;

    private readonly Collider[] golpeados = new Collider[16];
    private PlayerController jugador;
    private float proximoGolpe;
    private int danio;

    public int Danio
    {
        get { return danio; }
        set
        {
            danio = value;
            if (textoAyuda != null) textoAyuda.transform.parent.gameObject.SetActive(danio > 0);
        }
    }

    private void Awake()
    {
        jugador = GetComponent<PlayerController>();
    }

    private void Update()
    {
        if (Time.timeScale <= 0f) return;

        if (Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.F))
        {
            Atacar();
        }
    }

    public void Atacar()
    {
        if (danio <= 0 || Time.time < proximoGolpe) return;

        proximoGolpe = Time.time + esperaEntreGolpes;
        if (jugador.animador != null) jugador.animador.SetTrigger("Golpear");
        StartCoroutine(Impactar());
    }

    private IEnumerator Impactar()
    {
        yield return new WaitForSeconds(retrasoImpacto);

        Transform modelo = jugador.modeloVisualDoctor != null ? jugador.modeloVisualDoctor : transform;
        Vector3 centro = transform.position + modelo.forward * alcance;
        int total = Physics.OverlapSphereNonAlloc(centro, radio, golpeados, ~0, QueryTriggerInteraction.Ignore);

        HashSet<EnemigoPatrulla> enemigos = new HashSet<EnemigoPatrulla>();
        for (int i = 0; i < total; i++)
        {
            EnemigoPatrulla enemigo = golpeados[i].GetComponentInParent<EnemigoPatrulla>();
            if (enemigo != null) enemigos.Add(enemigo);
        }

        foreach (EnemigoPatrulla enemigo in enemigos)
        {
            enemigo.RecibirGolpe(danio, modelo.forward);
        }

        if (AudioManager.Instancia != null)
        {
            if (enemigos.Count > 0) AudioManager.Instancia.PlayGolpe();
            else AudioManager.Instancia.PlayFallo();
        }
    }
}
