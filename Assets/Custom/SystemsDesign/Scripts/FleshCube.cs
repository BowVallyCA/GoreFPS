using _Project.Code.Gameplay.Input;
using _Project.Code.Gameplay.Player;
using _Project.Code.Core.Events;
using UnityEngine;
using _Project.Code.Core.ServiceLocator;


public class FleshCube : MonoBehaviour
{
    [Header("Projectile Settings")]
    public GameObject bulletPrefab;  // The bullet prefab to spawn
    public Transform firePoint;      // The position & direction to shoot from
    public float bulletSpeed = 20f;  // Speed of the bullet
    public float bulletLifetime = 5f; // How long before the bullet is destroyed

    [Header("Gun Settings")]
    public int maxAmmo = 6;
    public int currentAmmo;

    [Header("Shrink Settings")]
    public Vector3 minScale = new Vector3(0.2f, 0.2f, 0.2f);  // Size when empty
    private Vector3 initialScale;

    [Header("Explosion Settings")]
    public float explosionRadius = 5f;
    public float explosionForce = 700f;
    public string targetTag = "Enemy"; // The tag to check for
    public GameObject explosionEffect; // Optional particle system

    [Header("Debug")]
    public bool showDebugGizmo = true;

    [SerializeField] private AudioSource audioSource;
    void Start()
    {
        currentAmmo = maxAmmo;
        initialScale = transform.localScale;
    }

    public void Initialize()
    {
        if (EventBus.Instance == null)
        {
            Debug.LogError("EventBus instance not found — FleshCube cannot subscribe!");
            return;
        }

        //EventBus.Instance.Subscribe<AttackInputEvent>(this, HandleAttackInput);
    }

    protected void OnDestroy()
    {
        // Use shawns input events systems

        EventBus.Instance?.Unsubscribe<AttackInputEvent>(this);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if(collision.gameObject.tag == "FleshCube")
        {
            CameraShakeManager.Shake();

            Explode();
        }
    }

    // Call this to trigger the explosion
    public void Explode()
    {
        // Optional: spawn explosion visual effect
        if (explosionEffect != null)
        {
            Instantiate(explosionEffect, transform.position, Quaternion.identity);
        }



        // Detect all colliders in a sphere around the explosion
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, explosionRadius);

        foreach (Collider hit in hitColliders)
        {
            Rigidbody rb = hit.GetComponent<Rigidbody>();

            // Apply explosion force to rigidbodies
            if (rb != null)
            {
                rb.AddExplosionForce(explosionForce, transform.position, explosionRadius, 1f, ForceMode.Impulse);
            }
        }

        // Optionally destroy the object after explosion
        Destroy(gameObject);
    }

    public void Shoot()
    {
        if (bulletPrefab == null || firePoint == null)
        {
            Debug.LogWarning("Missing bulletPrefab or firePoint!");
            return;
        }

        if (currentAmmo > 0)
        {
            currentAmmo--;
            Debug.Log($"{name} fired! Ammo left: {currentAmmo}");
            UpdateSize();

            if (currentAmmo <= 0)
            {
                OnOutOfAmmo();
            }
        }
        else
        {
            Debug.Log($"{name} is out of ammo!");
        }

        Debug.Log("Attack input pressed");

        // Spawn bullet
        GameObject bullet = Instantiate(bulletPrefab, firePoint.position, firePoint.rotation);

        // Add forward velocity
        Rigidbody rb = bullet.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.linearVelocity = firePoint.forward * bulletSpeed;
        }

        audioSource.Play();  

        // Destroy after lifetime
        Destroy(bullet, bulletLifetime);
    }

    /// Scales the gun down based on remaining ammo.
    private void UpdateSize()
    {
        float ammoRatio = (float)currentAmmo / maxAmmo;
        transform.localScale = Vector3.Lerp(minScale, initialScale, ammoRatio);
    }

    /// Called when ammo reaches zero. Destroys the gun.
    private void OnOutOfAmmo()
    {
        Debug.Log($"{name} is out of ammo and destroyed!");
        Destroy(gameObject);
    }

    // Visualize the explosion radius in the editor
    private void OnDrawGizmosSelected()
    {
        if (showDebugGizmo)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, explosionRadius);
        }
    }
}
