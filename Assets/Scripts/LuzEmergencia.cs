using UnityEngine;

/// <summary>Sirena: alterna la intensidad de dos luces.</summary>
public class LuzEmergencia : MonoBehaviour
{
    public Light luzA;
    public Light luzB;
    public float intensidad = 6f;
    public float frecuencia = 2.2f;

    private void Update()
    {
        float fase = Mathf.PingPong(Time.time * frecuencia, 1f);
        if (luzA != null) luzA.intensity = intensidad * fase;
        if (luzB != null) luzB.intensity = intensidad * (1f - fase);
    }
}
