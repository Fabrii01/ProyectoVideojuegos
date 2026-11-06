using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// Mueve las piernas del esqueleto al caminar. El modelo no trae animación de caminata,
/// así que el paso se calcula a partir de la velocidad del personaje.
/// </summary>
public class AndarProcedural : MonoBehaviour
{
    public Transform modelo;
    public float amplitudMuslo = 30f;
    public float amplitudRodilla = 45f;
    public float pasosPorMetro = 1.5f;

    private Transform musloI, musloD, rodillaI, rodillaD;
    private Quaternion baseMusloI, baseMusloD, baseRodillaI, baseRodillaD;
    private Rigidbody cuerpo;
    private NavMeshAgent agente;
    private float fase;
    private float peso;

    private void Awake()
    {
        cuerpo = GetComponent<Rigidbody>();
        agente = GetComponent<NavMeshAgent>();
        if (modelo == null) modelo = transform;

        foreach (Transform hueso in modelo.GetComponentsInChildren<Transform>(true))
        {
            switch (hueso.name)
            {
                case "Bip001 L Thigh": musloI = hueso; baseMusloI = hueso.localRotation; break;
                case "Bip001 R Thigh": musloD = hueso; baseMusloD = hueso.localRotation; break;
                case "Bip001 L Calf": rodillaI = hueso; baseRodillaI = hueso.localRotation; break;
                case "Bip001 R Calf": rodillaD = hueso; baseRodillaD = hueso.localRotation; break;
            }
        }
    }

    private void LateUpdate()
    {
        if (musloI == null || musloD == null || rodillaI == null || rodillaD == null) return;

        float velocidad = VelocidadHorizontal();
        peso = Mathf.MoveTowards(peso, velocidad > 0.3f ? 1f : 0f, Time.deltaTime * 6f);
        fase += velocidad * pasosPorMetro * Time.deltaTime * Mathf.PI;

        float paso = Mathf.Sin(fase);
        Vector3 eje = modelo.right;

        Girar(musloI, baseMusloI, eje, paso * amplitudMuslo * peso);
        Girar(musloD, baseMusloD, eje, -paso * amplitudMuslo * peso);
        Girar(rodillaI, baseRodillaI, eje, Mathf.Max(0f, paso) * amplitudRodilla * peso);
        Girar(rodillaD, baseRodillaD, eje, Mathf.Max(0f, -paso) * amplitudRodilla * peso);
    }

    private float VelocidadHorizontal()
    {
        Vector3 velocidad = Vector3.zero;
        if (agente != null) velocidad = agente.velocity;
        else if (cuerpo != null) velocidad = cuerpo.linearVelocity;

        velocidad.y = 0f;
        return velocidad.magnitude;
    }

    private static void Girar(Transform hueso, Quaternion rotacionBase, Vector3 eje, float angulo)
    {
        hueso.localRotation = rotacionBase;
        hueso.rotation = Quaternion.AngleAxis(angulo, eje) * hueso.rotation;
    }
}
