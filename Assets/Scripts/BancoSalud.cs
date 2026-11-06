using System.Collections.Generic;
using UnityEngine;

/// <summary>Lista de enfermedades del juego. Agrega aquí una Enfermedad para que entre a las preguntas.</summary>
[CreateAssetMenu(fileName = "BancoSalud", menuName = "Rescate/Banco de salud")]
public class BancoSalud : ScriptableObject
{
    public List<Enfermedad> enfermedades = new List<Enfermedad>();
}
