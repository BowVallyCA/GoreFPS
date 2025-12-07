using UnityEngine;
using _Project.Code.Core.Events;
using _Project.Code.Gameplay.Input;

public class ModularWeapon : MonoBehaviour
{
    public class WeaponOrigin : MonoBehaviour
    {
        public LimbType spawnedFromLimb;
    }

    // --------------------------------------------------------------------------
    //  GENERAL WEAPON SETTINGS
    // --------------------------------------------------------------------------
    [Header("General Settings")]
    public string weaponName = "Flesh Cube Weapon";
    public AudioSource audioSource;

    [Header("Projectile Settings")]
    public GameObject bulletPrefab;
    public Transform firePoint;
    public float bulletSpeed = 20f;
    public float bulletLifetime = 5f;

    [Header("Ammo Settings")]
    public int maxAmmo = 6;
    public int currentAmmo;

    // --------------------------------------------------------------------------
    //  OPTIONAL: SIZE SHRINKING BASED ON AMMO
    // --------------------------------------------------------------------------
    [Header("Size Shrink Settings (Optional)")]
    public bool enableShrink = true;

    public Vector3 minScale = new Vector3(0.2f, 0.2f, 0.2f);
    private Vector3 initialScale;

    // --------------------------------------------------------------------------
    //  OPTIONAL: EXPLOSION MODE
    // --------------------------------------------------------------------------
    [Header("Explosion Settings (Optional)")]
    public bool enableExplosion = false;

    public float explosionRadius = 5f;
    public float explosionForce = 700f;
    public GameObject explosionEffect;
    public string targetTag = "Enemy";

    // --------------------------------------------------------------------------
    //  OPTIONAL: SHOTGUN MODE
    // --------------------------------------------------------------------------
    [Header("Shotgun Settings (Optional)")]
    public bool enableShotgun = false;
    public int pelletCount = 6;
    public float pelletSpreadAngle = 8f;

    // --------------------------------------------------------------------------
    //  DEBUG
    // --------------------------------------------------------------------------
    [Header("Debug")]
    public bool showExplosionDebug = true;

    // --------------------------------------------------------------------------
    //  UNITY
    // --------------------------------------------------------------------------
    void Start()
    {
        currentAmmo = maxAmmo;
        initialScale = transform.localScale;
    }

    private void OnDestroy()
    {
        EventBus.Instance?.Unsubscribe<AttackInputEvent>(this);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (enableExplosion && collision.gameObject.CompareTag("Bullet"))
        {
            Explode();
        }
    }

    // --------------------------------------------------------------------------
    //  SHOOT FUNCTION (Supports: single shot OR shotgun)
    // --------------------------------------------------------------------------
    public void Shoot()
    {
        if (bulletPrefab == null || firePoint == null)
        {
            Debug.LogWarning("Weapon missing bulletPrefab or firePoint!");
            return;
        }

        if (currentAmmo <= 0)
        {
            Debug.Log($"{weaponName} is out of ammo!");
            return;
        }

        currentAmmo--;

        if (enableShrink)
            UpdateSize();

        if (currentAmmo <= 0)
            OnOutOfAmmo();

        // Play sound
        audioSource?.Play();

        // Fire shotgun or single shot:
        if (enableShotgun)
        {
            ShootShotgun();
        }
        else
        {
            ShootSingle();
        }
    }

    // --------------------------------------------------------------------------
    //  SINGLE BULLET FIRE
    // --------------------------------------------------------------------------
    private void ShootSingle()
    {
        GameObject bullet = Instantiate(bulletPrefab, firePoint.position, firePoint.rotation);

        Rigidbody rb = bullet.GetComponent<Rigidbody>();
        if (rb != null)
            rb.linearVelocity = firePoint.forward * bulletSpeed;

        Destroy(bullet, bulletLifetime);
    }

    // --------------------------------------------------------------------------
    //  SHOTGUN FIRE MODE
    // --------------------------------------------------------------------------
    private void ShootShotgun()
    {
        for (int i = 0; i < pelletCount; i++)
        {
            Quaternion randomSpread = firePoint.rotation *
                Quaternion.Euler(
                    Random.Range(-pelletSpreadAngle, pelletSpreadAngle),
                    Random.Range(-pelletSpreadAngle, pelletSpreadAngle),
                    0);

            GameObject pellet = Instantiate(bulletPrefab, firePoint.position, randomSpread);

            Rigidbody rb = pellet.GetComponent<Rigidbody>();
            if (rb != null)
                rb.linearVelocity = randomSpread * Vector3.forward * bulletSpeed;

            Destroy(pellet, bulletLifetime);
        }
    }

    // --------------------------------------------------------------------------
    //  EXPLOSION LOGIC
    // --------------------------------------------------------------------------
    public void Explode()
    {
        if (!enableExplosion)
            return;

        if (explosionEffect != null)
            Instantiate(explosionEffect, transform.position, Quaternion.identity);

        Collider[] hitColliders = Physics.OverlapSphere(transform.position, explosionRadius);

        foreach (Collider hit in hitColliders)
        {
            Rigidbody rb = hit.GetComponent<Rigidbody>();
            if (rb != null)
                rb.AddExplosionForce(explosionForce, transform.position, explosionRadius, 1f, ForceMode.Impulse);
        }

        Destroy(gameObject);
    }

    // --------------------------------------------------------------------------
    //  SIZE SHRINKING BASED ON AMMO
    // --------------------------------------------------------------------------
    private void UpdateSize()
    {
        if (!enableShrink)
            return;

        float ammoRatio = (float)currentAmmo / maxAmmo;
        transform.localScale = Vector3.Lerp(minScale, initialScale, ammoRatio);
    }

    // --------------------------------------------------------------------------
    //  OUT OF AMMO
    // --------------------------------------------------------------------------
    private void OnOutOfAmmo()
    {
        Debug.Log($"{weaponName} is out of ammo and destroyed!");
        Destroy(gameObject);
    }

    // --------------------------------------------------------------------------
    //  DEBUG DRAW
    // --------------------------------------------------------------------------
    private void OnDrawGizmosSelected()
    {
        if (enableExplosion && showExplosionDebug)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, explosionRadius);
        }
    }
}
