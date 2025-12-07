using UnityEngine;

public class WeaponSpawner : MonoBehaviour
{
    [System.Serializable]
    public class WeaponSlot
    {
        public string name;
        public GameObject weaponPrefab;
        public LimbType limbCost;
        public int healthCost = 0;
    }

    [Header("Weapon Settings")]
    public WeaponSlot[] weaponSlots = new WeaponSlot[4];
    public Transform weaponHoldPoint;

    [Header("References")]
    public HoldObjectScript holdManager;
    public LimbHealth limbHealth;
    public AudioSource audioSource;

    [Header("Spawn")]
    public float spawnCooldown = 0.7f;
    private float cooldownTimer = 0f;

    private GameObject currentWeapon;

    void Start()
    {
        if (holdManager == null)
            holdManager = GetComponent<HoldObjectScript>();

        if (limbHealth == null)
            limbHealth = GetComponent<LimbHealth>();
    }

    void Update()
    {
        if (cooldownTimer > 0f)
            cooldownTimer -= Time.deltaTime;

        if (Input.GetKeyDown(KeyCode.Alpha1)) SpawnWeaponAtIndex(0);
        if (Input.GetKeyDown(KeyCode.Alpha2)) SpawnWeaponAtIndex(1);
        if (Input.GetKeyDown(KeyCode.Alpha3)) SpawnWeaponAtIndex(2);
        if (Input.GetKeyDown(KeyCode.Alpha4)) SpawnWeaponAtIndex(3);
    }

    private void SpawnWeaponAtIndex(int index)
    {
        if (index < 0 || index >= weaponSlots.Length) return;
        if (cooldownTimer > 0f) return;
        if (limbHealth.body.CurrentHealth <= 0) return;

        WeaponSlot slot = weaponSlots[index];
        if (slot == null || slot.weaponPrefab == null) return;

        // If holding a weapon -> destroy & refund
        if (holdManager.GetHeldObject() != null)
        {
            RefundHealthFromHeldWeapon();
            Destroy(holdManager.GetHeldObject().gameObject);
            holdManager.ClearHeldObject();
            currentWeapon = null;
        }

        // Spawn weapon into world (not parented)
        GameObject spawned = Instantiate(
            slot.weaponPrefab,
            weaponHoldPoint.position,
            weaponHoldPoint.rotation
        );

        // Store metadata for refund later
        WeaponOrigin origin = spawned.AddComponent<WeaponOrigin>();
        origin.spawnedFromLimb = slot.limbCost;
        origin.healthCost = slot.healthCost;

        // Apply damage cost to the chosen limb
        limbHealth.DamageSpecificLimb(slot.limbCost, slot.healthCost);

        // Equip weapon
        Rigidbody rb = spawned.GetComponent<Rigidbody>();
        if (rb != null)
            //holdManager.ForceHold(rb);

        currentWeapon = spawned;
        cooldownTimer = spawnCooldown;

        // FX
        if (audioSource != null)
            audioSource.Play();

        CameraShakeManager.Shake();
    }

    private void RefundHealthFromHeldWeapon()
    {
        Rigidbody held = holdManager.GetHeldObject();
        if (held == null) return;

        WeaponOrigin origin = held.GetComponent<WeaponOrigin>();
        if (origin == null) return;

        // DEBUG — see what cost is detected
        Debug.Log($"[REFUND] Limb: {origin.spawnedFromLimb}, Refund: {origin.healthCost}");

        limbHealth.HealLimb(origin.spawnedFromLimb, origin.healthCost);
    }
}