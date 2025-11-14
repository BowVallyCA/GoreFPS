using UnityEngine;

public class FleshCubeExplosion : MonoBehaviour
{
    [Header("Explosion Settings")]
    public float explosionRadius = 5f;
    public float explosionForce = 700f;
    public string targetTag = "Enemy";
    public GameObject explosionEffect;
    public bool showDebugGizmo = true;

    public void Explode()
    {
        if (explosionEffect != null)
            Instantiate(explosionEffect, transform.position, Quaternion.identity);

        Collider[] hits = Physics.OverlapSphere(transform.position, explosionRadius);

        foreach (Collider hit in hits)
        {
            Rigidbody rb = hit.attachedRigidbody;
            if (rb != null)
                rb.AddExplosionForce(explosionForce, transform.position, explosionRadius, 1f, ForceMode.Impulse);
        }

        Destroy(gameObject);
    }

    private void OnDrawGizmosSelected()
    {
        if (!showDebugGizmo) return;

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, explosionRadius);
    }
}
