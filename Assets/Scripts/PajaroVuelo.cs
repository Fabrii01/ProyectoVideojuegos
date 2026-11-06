using UnityEngine;

/// <summary>Pájaro decorativo que vuela en círculos aleteando.</summary>
public class PajaroVuelo : MonoBehaviour
{
    public Vector3 centro;
    public float radio = 18f;
    public float velocidadAngular = 22f;
    public Transform alaIzquierda;
    public Transform alaDerecha;

    private float angulo;
    private float fase;

    private void Start()
    {
        angulo = Random.Range(0f, 360f);
        fase = Random.Range(0f, 6f);
    }

    private void Update()
    {
        angulo += velocidadAngular * Time.deltaTime;
        float radianes = angulo * Mathf.Deg2Rad;

        Vector3 posicion = centro + new Vector3(Mathf.Cos(radianes), 0f, Mathf.Sin(radianes)) * radio;
        posicion.y += Mathf.Sin(Time.time * 0.6f + fase) * 1.5f;

        Vector3 avance = posicion - transform.position;
        if (avance.sqrMagnitude > 0.0001f) transform.rotation = Quaternion.LookRotation(avance);
        transform.position = posicion;

        float aleteo = Mathf.Sin(Time.time * 11f + fase) * 35f;
        if (alaIzquierda != null) alaIzquierda.localRotation = Quaternion.Euler(0f, 0f, aleteo);
        if (alaDerecha != null) alaDerecha.localRotation = Quaternion.Euler(0f, 0f, -aleteo);
    }
}
