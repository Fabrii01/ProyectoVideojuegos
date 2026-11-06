using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Evaluación del NPC de preguntas. Solo pregunta sobre las enfermedades que el jugador
/// ya curó, en orden aleatorio, y explica la respuesta después de cada pregunta.
/// </summary>
public class PreguntasUI : MonoBehaviour
{
    public BancoSalud banco;
    public int maximoPreguntas = 6;

    [Header("Textos")]
    public TextMeshProUGUI textoProgreso;
    public TextMeshProUGUI textoPregunta;
    public TextMeshProUGUI textoExplicacion;

    [Header("Botones")]
    public Button[] opciones;
    public TextMeshProUGUI[] textosOpciones;
    public Button botonSiguiente;
    public TextMeshProUGUI textoSiguiente;

    [Header("Colores")]
    public Color colorNormal = new Color(0.24f, 0.25f, 0.28f);
    public Color colorCorrecto = new Color(0.1f, 0.62f, 0.44f);
    public Color colorIncorrecto = new Color(0.82f, 0.17f, 0.12f);

    private readonly List<PreguntaSalud> preguntas = new List<PreguntaSalud>();
    private int[] orden;
    private int indice;
    private int aciertos;

    /// <summary>Botón que tiene la respuesta correcta en la pregunta actual.</summary>
    public int BotonCorrecto => System.Array.IndexOf(orden, preguntas[indice].correcta);
    public int TotalPreguntas => preguntas.Count;
    public int Aciertos => aciertos;

    private void Awake()
    {
        for (int i = 0; i < opciones.Length; i++)
        {
            int boton = i;
            opciones[i].onClick.AddListener(() => Responder(boton));
        }
        botonSiguiente.onClick.AddListener(Siguiente);
    }

    private void OnEnable()
    {
        Empezar();
    }

    private void Empezar()
    {
        preguntas.Clear();
        indice = 0;
        aciertos = 0;

        foreach (Enfermedad enfermedad in banco.enfermedades)
        {
            if (DatosJugador.EnfermedadAprendida(enfermedad.id)) preguntas.AddRange(enfermedad.preguntas);
        }

        Mezclar(preguntas);
        if (preguntas.Count > maximoPreguntas) preguntas.RemoveRange(maximoPreguntas, preguntas.Count - maximoPreguntas);

        if (preguntas.Count == 0)
        {
            MostrarMensaje("Todavía no hay nada que repasar", "Completa una misión y cura a tus pacientes. Luego vuelve y te preguntaré sobre lo que aprendiste.");
            return;
        }

        MostrarPregunta();
    }

    private static void Mezclar<T>(IList<T> lista)
    {
        for (int i = lista.Count - 1; i > 0; i--)
        {
            int otro = Random.Range(0, i + 1);
            T temporal = lista[i];
            lista[i] = lista[otro];
            lista[otro] = temporal;
        }
    }

    private void MostrarPregunta()
    {
        PreguntaSalud pregunta = preguntas[indice];

        textoProgreso.text = $"Pregunta {indice + 1} de {preguntas.Count}";
        textoPregunta.text = pregunta.texto;
        textoExplicacion.text = string.Empty;
        botonSiguiente.gameObject.SetActive(false);

        orden = new int[pregunta.opciones.Length];
        for (int i = 0; i < orden.Length; i++) orden[i] = i;
        Mezclar(orden);

        for (int i = 0; i < opciones.Length; i++)
        {
            opciones[i].gameObject.SetActive(true);
            opciones[i].interactable = true;
            Pintar(opciones[i], colorNormal);
            textosOpciones[i].text = pregunta.opciones[orden[i]];
        }
    }

    private void Responder(int boton)
    {
        PreguntaSalud pregunta = preguntas[indice];
        bool acierto = orden[boton] == pregunta.correcta;
        if (acierto) aciertos++;

        for (int i = 0; i < opciones.Length; i++)
        {
            opciones[i].interactable = false;

            if (orden[i] == pregunta.correcta) Pintar(opciones[i], colorCorrecto);
            else if (i == boton) Pintar(opciones[i], colorIncorrecto);
        }

        textoExplicacion.text = (acierto ? "<b>¡Correcto!</b> " : "<b>No exactamente.</b> ") + pregunta.explicacion;
        textoSiguiente.text = indice + 1 < preguntas.Count ? "SIGUIENTE" : "VER RESULTADO";
        botonSiguiente.gameObject.SetActive(true);

        if (AudioManager.Instancia == null) return;
        if (acierto) AudioManager.Instancia.PlayCompra();
        else AudioManager.Instancia.PlayFallo();
    }

    private void Siguiente()
    {
        indice++;

        if (indice < preguntas.Count)
        {
            MostrarPregunta();
            return;
        }

        string consejo = aciertos == preguntas.Count ? "¡Excelente! Estás listo para ayudar." : "Puedes intentarlo otra vez cuando quieras: las preguntas cambian.";
        MostrarMensaje($"Acertaste {aciertos} de {preguntas.Count}", consejo);
    }

    private void MostrarMensaje(string titulo, string mensaje)
    {
        textoProgreso.text = "Resultado";
        textoPregunta.text = titulo;
        textoExplicacion.text = mensaje;
        botonSiguiente.gameObject.SetActive(false);

        foreach (Button opcion in opciones) opcion.gameObject.SetActive(false);
    }

    private static void Pintar(Button boton, Color color)
    {
        ColorBlock colores = boton.colors;
        colores.normalColor = color;
        colores.selectedColor = color;
        colores.disabledColor = color;
        colores.highlightedColor = Color.Lerp(color, Color.white, 0.18f);
        boton.colors = colores;
    }
}
