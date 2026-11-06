using System;
using UnityEngine;

/// <summary>Pregunta de opción múltiple sobre una enfermedad.</summary>
[Serializable]
public class PreguntaSalud
{
    [TextArea(2, 3)] public string texto;
    public string[] opciones;
    public int correcta;
    [TextArea(2, 3)] public string explicacion;
}

/// <summary>
/// Enfermedad de un paciente: lo que se aprende al curarla y las preguntas que hace
/// luego el NPC del Hub. Son primeros auxilios básicos, no reemplazan a un profesional de salud.
/// </summary>
[CreateAssetMenu(fileName = "Enfermedad", menuName = "Rescate/Enfermedad")]
public class Enfermedad : ScriptableObject
{
    public string id;
    public string nombre;
    [TextArea(2, 4)] public string sintomas;
    [TextArea(3, 6)] public string tratamiento;
    public PreguntaSalud[] preguntas;
}
