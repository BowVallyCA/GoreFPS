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
    //  GENERAL
    // --------------------------------------------------------------------------
    [Header("General Settings")]
    public string weaponName = "Flesh Cube Weapon";
    public AudioSource audioSource;

    // --------------------------------------------------------------------------
    //  PROJECTILES
    // --------------------------------------------------------------------------
    [Header("Projectile Settings")]
    public GameObject bulletPrefab;
    public Transform firePoint;
    public float bulletSpeed = 20f;
    public float bulletLifetime = 5f;

    // --------------------------------------------------------------------------
    //  AMMO
    // --------------------------------------------------------------------------
    [Header("Ammo Settings")]
    public int maxAmmo = 6;
    public int currentAmmo;

    // --------------------------------------------------------------------------
    //  SIZE SHRINK
    // --------------------------------------------------------------------------
    [Header("Size Shrink Settings")]
    public bool enableShrink = true;
    public Vector3 minScale = new Vector3(0.2f, 0.2f, 0.2f);
    private Vector3 initialScale;

    // --------------------------------------------------------------------------
    //  EXPLOSION MODE
    // --------------------------------------------------------------------------
    [Header("Explosion Settings")]
    public bool enableExplosion = false;
    public float explosionRadius = 5f;
    public float explosionForce = 700f;
    public GameObject explosionEffect;

    // --------------------------------------------------------------------------
    //  SHOTGUN MODE
    // --------------------------------------------------------------------------
    [Header("Shotgun Settings")]
    public bool enableShotgun = false;
    public int pelletCount = 6;
    public float pelletSpreadAngle = 8f;

    // --------------------------------------------------------------------------
    //  WALL SPAWN MODE (SHOT BY OTHER WEAPONS)
    // --------------------------------------------------------------------------
    [Header("Wall Spawn Settings (Shot Reaction)")]
    public bool enableWallSpawn = false;
    public GameObject wallPrefab;
    public float wallLifetime = 10f;

    // --------------------------------------------------------------------------
    //  DEBUG
    // --------------------------------------------------------------------------
    [Header("Debug")]
    public bool showExplosionDebug = true;

    // --------------------------------------------------------------------------
    //  UNITY
    // --------------------------------------------------------------------------
    private void Start()
    {
        currentAmmo = maxAmmo;
        initialScale = transform.localScale;
    }

    private void OnDestroy()
    {
        EventBus.Instance?.Unsubscribe<AttackInputEvent>(this);
    }

    // --------------------------------------------------------------------------
    //  COLLISION — SHOT BY ANOTHER WEAPON
    // --------------------------------------------------------------------------
    private void OnCollisionEnter(Collision collision)
    {
        if (!collision.gameObject.CompareTag("Bullet"))
            return;

        // Explosion behavior
        if (enableExplosion)
        {
            Explode();
            return;
        }

        // Wall spawn behavior
        if (enableWallSpawn)
        {
            SpawnWallAndDestroy();
            return;
        }
    }

    // --------------------------------------------------------------------------
    //  SHOOT (PLAYER USE ONLY)
    // --------------------------------------------------------------------------
    public void Shoot()
    {
        if (bulletPrefab == null || firePoint == null)
            return;

        if (currentAmmo <= 0)
        {
            Debug.Log($"{weaponName} is out of ammo!");
            return;
        }

        currentAmmo--;

        if (enableShrink)
            UpdateSize();

        audioSource?.Play();

        if (enableShotgun)
            ShootShotgun();
        else
            ShootSingle();

        if (currentAmmo <= 0)
            OnOutOfAmmo();
    }

    // --------------------------------------------------------------------------
    //  SINGLE SHOT
    // --------------------------------------------------------------------------
    private void ShootSingle()
    {
        GameObject bullet = Instantiate(
            bulletPrefab,
            firePoint.position,
            firePoint.rotation
        );

        Rigidbody rb = bullet.GetComponent<Rigidbody>();
        if (rb != null)
            rb.linearVelocity = firePoint.forward * bulletSpeed;

        Destroy(bullet, bulletLifetime);
    }

    // --------------------------------------------------------------------------
    //  SHOTGUN
    // --------------------------------------------------------------------------
    private void ShootShotgun()
    {
        for (int i = 0; i < pelletCount; i++)
        {
            Quaternion spread =
                firePoint.rotation *
                Quaternion.Euler(
                    Random.Range(-pelletSpreadAngle, pelletSpreadAngle),
                    Random.Range(-pelletSpreadAngle, pelletSpreadAngle),
                    0f
                );

            GameObject pellet = Instantiate(
                bulletPrefab,
                firePoint.position,
                spread
            );

            Rigidbody rb = pellet.GetComponent<Rigidbody>();
            if (rb != null)
                rb.linearVelocity = spread * Vector3.forward * bulletSpeed;

            Destroy(pellet, bulletLifetime);
        }
    }

    // --------------------------------------------------------------------------
    //  WALL SPAWN (REACTION TO BEING SHOT)
    // --------------------------------------------------------------------------
    private void SpawnWallAndDestroy()
    {
        if (explosionEffect != null)
            Instantiate(explosionEffect, transform.position, Quaternion.identity);

        if (wallPrefab != null)
        {
            GameObject wall = Instantiate(
                wallPrefab,
                transform.position,
                transform.rotation
            );

            if (wallLifetime > 0f)
                Destroy(wall, wallLifetime);
        }

        Destroy(gameObject);
    }

    // --------------------------------------------------------------------------
    //  EXPLOSION
    // --------------------------------------------------------------------------
    private void Explode()
    {
        if (explosionEffect != null)
            Instantiate(explosionEffect, transform.position, Quaternion.identity);

        Collider[] hits = Physics.OverlapSphere(transform.position, explosionRadius);

        foreach (Collider hit in hits)
        {
            Rigidbody rb = hit.GetComponent<Rigidbody>();
            if (rb != null)
                rb.AddExplosionForce(
                    explosionForce,
                    transform.position,
                    explosionRadius,
                    1f,
                    ForceMode.Impulse
                );
            LimbHealth lh = hit.GetComponent<LimbHealth>();
            if(lh != null)
            {
                lh.TakeDamageRandom(50);
            }
        }

        Destroy(gameObject);
    }

    // --------------------------------------------------------------------------
    //  SIZE SHRINK
    // --------------------------------------------------------------------------
    private void UpdateSize()
    {
        float ratio = (float)currentAmmo / maxAmmo;
        transform.localScale = Vector3.Lerp(minScale, initialScale, ratio);
    }

    // --------------------------------------------------------------------------
    //  OUT OF AMMO
    // --------------------------------------------------------------------------
    private void OnOutOfAmmo()
    {
        Destroy(gameObject);
    }

    // --------------------------------------------------------------------------
    //  DEBUG
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