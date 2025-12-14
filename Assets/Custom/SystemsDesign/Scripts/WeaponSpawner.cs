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
    public Animator firePointAnimator;    // plays animation when weapon is spawned

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

        // -----------------------
        // Drop current weapon
        // -----------------------
        Rigidbody currentlyHeld = holdManager.GetHeldObject();
        if (currentlyHeld != null)
            holdManager.ThrowObject();

        // -----------------------
        // Spawn new weapon
        // -----------------------
        GameObject spawned = Instantiate(
            slot.weaponPrefab,
            weaponHoldPoint.position,
            weaponHoldPoint.rotation
        );

        // Add metadata to refund later
        WeaponOrigin origin = spawned.AddComponent<WeaponOrigin>();
        origin.spawnedFromLimb = slot.limbCost;
        origin.healthCost = slot.healthCost;

        // Apply limb cost
        limbHealth.DamageSpecificLimb(slot.limbCost, slot.healthCost);

        // -----------------------
        // Equip the new weapon
        // -----------------------
        holdManager.EquipWeapon(spawned);

        currentWeapon = spawned;
        cooldownTimer = spawnCooldown;

        // -----------------------
        // FX
        // -----------------------
        if (audioSource != null)
            audioSource.Play();

        CameraShakeManager.Shake();

        if (firePointAnimator != null)
            firePointAnimator.SetTrigger("Spawn");
    }

    private void RefundHealthFromHeldWeapon()
    {
        Rigidbody held = holdManager.GetHeldObject();
        if (held == null) return;

        WeaponOrigin origin = held.GetComponent<WeaponOrigin>();
        if (origin == null) return;

        limbHealth.HealLimb(origin.spawnedFromLimb, origin.healthCost);
    }
}