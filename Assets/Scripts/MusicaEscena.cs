using UnityEngine;

/// <summary>
/// Música de la escena. Si tiene tema de combate, cambia a él mientras
/// un enemigo persigue al jugador y vuelve al tema principal al calmarse.
/// </summary>
public class MusicaEscena : MonoBehaviour
{
    public AudioClip temaPrincipal;
    public AudioClip temaCombate;
    public float esperaAlCalmarse = 3f;

    private EnemigoPatrulla[] enemigos;
    private float ultimoPeligro = float.NegativeInfinity;
    private bool enCombate;

    private void Start()
    {
        enemigos = FindObjectsByType<EnemigoPatrulla>(FindObjectsInactive.Exclude);
        if (AudioManager.Instancia != null) AudioManager.Instancia.CambiarMusica(temaPrincipal);
    }

    private void Update()
    {
        if (temaCombate == null || AudioManager.Instancia == null || Time.timeScale <= 0f) return;

        foreach (EnemigoPatrulla enemigo in enemigos)
        {
            if (enemigo != null && enemigo.EstaPersiguiendo) ultimoPeligro = Time.time;
        }

        bool peligro = Time.time - ultimoPeligro < esperaAlCalmarse;
        if (peligro == enCombate) return;

        enCombate = peligro;
        AudioManager.Instancia.CambiarMusica(peligro ? temaCombate : temaPrincipal);
    }
}
