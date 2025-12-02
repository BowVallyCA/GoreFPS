using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class FleshCubeWeapon : MonoBehaviour, IWeapon
{
    [Header("Projectile Settings")]
    public GameObject bulletPrefab;
    public Transform firePoint;
    public float bulletSpeed = 20f;
    public float bulletLifetime = 5f;

    [Header("Gun Settings")]
    public int maxAmmo = 6;
    public int currentAmmo;

    [Header("Shrink Settings")]
    public Vector3 minScale = new Vector3(0.2f, 0.2f, 0.2f);
    private Vector3 initialScale;

    private AudioSource audioSource;
    private FleshCubeExplosion explosionHandler;

    void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        explosionHandler = GetComponent<FleshCubeExplosion>();
        initialScale = transform.localScale;
        currentAmmo = maxAmmo;
    }

    public void Shoot()
    {
        if (bulletPrefab == null || firePoint == null)
        {
            Debug.LogWarning("Missing bulletPrefab or firePoint!");
            return;
        }

        if (currentAmmo <= 0)
        {
            Debug.Log($"{name} is out of ammo!");
            return;
        }

        currentAmmo--;
        Debug.Log($"{name} fired! Ammo left: {currentAmmo}");
        UpdateSize();

        GameObject bullet = Instantiate(bulletPrefab, firePoint.position, firePoint.rotation);

        Rigidbody rb = bullet.GetComponent<Rigidbody>();
        if (rb != null)
            rb.linearVelocity = firePoint.forward * bulletSpeed;

        audioSource.Play();
        Destroy(bullet, bulletLifetime);

        if (currentAmmo <= 0)
            OnOutOfAmmo();
    }

    public void Reload(int ammo)
    {
        currentAmmo = Mathf.Clamp(currentAmmo + ammo, 0, maxAmmo);
        UpdateSize();
    }

    public int GetCurrentAmmo() => currentAmmo;
    public int GetMaxAmmo() => maxAmmo;

    private void UpdateSize()
    {
        float ratio = (float)currentAmmo / maxAmmo;
        transform.localScale = Vector3.Lerp(minScale, initialScale, ratio);
    }

    private void OnOutOfAmmo()
    {
        Debug.Log($"{name} is out of ammo and destroyed!");
        Destroy(gameObject);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("FleshCube"))
        {
            CameraShakeManager.Shake();
            explosionHandler?.Explode();
        }
    }
}
