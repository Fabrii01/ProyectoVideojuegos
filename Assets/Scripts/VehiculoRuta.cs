using UnityEngine;

/// <summary>Vehículo que da vueltas por una ruta y frena si tiene a alguien delante.</summary>
public class VehiculoRuta : MonoBehaviour
{
    public Transform[] puntos;
    public int indice;
    public float velocidad = 8f;
    public float velocidadGiro = 110f;
    public float distanciaFreno = 6f;

    private void Update()
    {
        if (puntos == null || puntos.Length == 0 || HayAlguienDelante()) return;

        Vector3 destino = puntos[indice].position;
        Vector3 direccion = destino - transform.position;
        direccion.y = 0f;

        if (direccion.magnitude < 0.6f)
        {
            indice = (indice + 1) % puntos.Length;
            return;
        }

        Quaternion rotacion = Quaternion.LookRotation(direccion);
        transform.rotation = Quaternion.RotateTowards(transform.rotation, rotacion, velocidadGiro * Time.deltaTime);
        transform.position += transform.forward * (velocidad * Time.deltaTime);
    }

    private bool HayAlguienDelante()
    {
        Vector3 centro = transform.position + Vector3.up;
        RaycastHit[] golpes = Physics.BoxCastAll(centro, new Vector3(1f, 0.8f, 1f), transform.forward, transform.rotation, distanciaFreno, ~0, QueryTriggerInteraction.Ignore);

        foreach (RaycastHit golpe in golpes)
        {
            if (golpe.transform.IsChildOf(transform)) continue;

            bool esPersona = golpe.collider.CompareTag("Player") || golpe.collider.GetComponentInParent<NPCResidente>() != null;
            if (esPersona || golpe.collider.GetComponentInParent<VehiculoRuta>() != null) return true;
        }
        return false;
    }
}
