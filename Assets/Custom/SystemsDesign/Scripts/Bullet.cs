using UnityEngine;

public class Bullet : MonoBehaviour
{
    [Header("Bullet Settings")]
    [SerializeField] private float speed = 50f;
    [SerializeField] private float lifetime = 5f;
    [SerializeField] private float impactForce = 10f;
    [SerializeField] private int damage = 10;

    [Header("Collision Filtering")]
    [Tooltip("If true, the bullet will only damage objects on the target layer.")]
    [SerializeField] private bool useTargetLayer = false;
    [SerializeField] private LayerMask targetLayer;

    [Tooltip("If true, the bullet will only damage objects with these tags.")]
    [SerializeField] private bool useTargetTags = false;
    [SerializeField] private string[] targetTags;

    private float timer;
    private GameObject shooter; // prevents self-hit

    // ----------------------------------------------------
    // INITIALIZATION
    // ----------------------------------------------------
    public void Initialize(GameObject shooter)
    {
        this.shooter = shooter;
    }

    private void Awake()
    {
        timer = lifetime;
    }

    private void Update()
    {
        transform.position += transform.forward * speed * Time.deltaTime;

        timer -= Time.deltaTime;
        if (timer <= 0f)
        {
            Destroy(gameObject);
        }
    }

    // ----------------------------------------------------
    // COLLISION
    // ----------------------------------------------------
    private void OnCollisionEnter(Collision collision)
    {
        GameObject hitObject = collision.gameObject;

        // 1. Prevent self-hit
        if (shooter != null && hitObject == shooter)
            return;

        if (hitObject.CompareTag("Bullet"))
        {
            return;
        }

            // 2. Layer filtering
            if (useTargetLayer && (targetLayer.value & (1 << hitObject.layer)) == 0)
        {
            Destroy(gameObject);
            return;
        }

        // 3. Tag filtering
        if (useTargetTags && !TagMatch(hitObject.tag))
        {
            Destroy(gameObject);
            return;
        }

        // 4. Apply physics force
        if (collision.rigidbody != null)
        {
            collision.rigidbody.AddForce(
                transform.forward * impactForce,
                ForceMode.Impulse
            );
        }

        // 5. Limb-based damage (random limb)
        LimbHealth limbHealth = hitObject.GetComponentInParent<LimbHealth>();
        if (limbHealth != null)
        {
            limbHealth.TakeDamageRandom(damage);
            Destroy(gameObject);
        }

        // 6. Destroy bullet
        Destroy(gameObject);
    }

    // ----------------------------------------------------
    // UTIL
    // ----------------------------------------------------
    private bool TagMatch(string tag)
    {
        for (int i = 0; i < targetTags.Length; i++)
        {
            if (tag == targetTags[i])
                return true;
        }
        return false;
    }
}

