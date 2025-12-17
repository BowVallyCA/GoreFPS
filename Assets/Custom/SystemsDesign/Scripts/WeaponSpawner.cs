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

    [Tooltip("Used only for player-held weapons")]
    public Transform weaponHoldPoint;

    [Header("References")]
    public HoldObjectScript holdManager;   // Player-only
    public LimbHealth limbHealth;
    public AudioSource audioSource;
    public Animator firePointAnimator;

    [Header("Spawn (Player Only)")]
    public float spawnCooldown = 0.7f;
    private float cooldownTimer;

    private void Start()
    {
        if (limbHealth == null)
            limbHealth = GetComponent<LimbHealth>();
    }

    private void Update()
    {
        if (cooldownTimer > 0f)
            cooldownTimer -= Time.deltaTime;

        // Manual spawning is PLAYER ONLY
        if (!limbHealth || !limbHealth.isPlayer) return;

        if (Input.GetKeyDown(KeyCode.Alpha1)) SpawnWeaponHeld(0);
        if (Input.GetKeyDown(KeyCode.Alpha2)) SpawnWeaponHeld(1);
        if (Input.GetKeyDown(KeyCode.Alpha3)) SpawnWeaponHeld(2);
        if (Input.GetKeyDown(KeyCode.Alpha4)) SpawnWeaponHeld(3);
    }

    // =========================================================
    // PLAYER: SPAWN + EQUIP WEAPON
    // =========================================================
    private void SpawnWeaponHeld(int index)
    {
        if (index < 0 || index >= weaponSlots.Length) return;
        if (cooldownTimer > 0f) return;

        WeaponSlot slot = weaponSlots[index];
        if (slot == null || slot.weaponPrefab == null) return;

        // Drop currently held weapon
        if (holdManager != null && holdManager.GetHeldObject() != null)
            holdManager.ThrowObject();

        GameObject spawned = Instantiate(
            slot.weaponPrefab,
            weaponHoldPoint.position,
            weaponHoldPoint.rotation
        );

        // Metadata
        WeaponOrigin origin = spawned.AddComponent<WeaponOrigin>();
        origin.spawnedFromLimb = slot.limbCost;
        origin.healthCost = slot.healthCost;

        // Apply limb cost
        limbHealth.DamageSpecificLimb(slot.limbCost, slot.healthCost);

        // Equip
        holdManager?.EquipWeapon(spawned);

        cooldownTimer = spawnCooldown;

        audioSource?.Play();
        CameraShakeManager.Shake();
        firePointAnimator?.SetTrigger("Spawn");
    }

    // =========================================================
    // 🔥 SHARED: DROP WEAPON TO GROUND (PLAYER + ENEMY)
    // CALLED BY LimbHealth WHEN DAMAGED
    // =========================================================
    public void SpawnLimbWeaponToGround(LimbType limb)
    {
        WeaponSlot slot = GetSlotForLimb(limb);
        if (slot == null || slot.weaponPrefab == null) return;

        Vector3 spawnPos = transform.position + Vector3.up * 0.4f;

        GameObject spawned = Instantiate(
            slot.weaponPrefab,
            spawnPos,
            Quaternion.identity
        );

        WeaponOrigin origin = spawned.AddComponent<WeaponOrigin>();
        origin.spawnedFromLimb = limb;
        origin.healthCost = slot.healthCost;

        if (!spawned.TryGetComponent<Rigidbody>(out _))
            spawned.AddComponent<Rigidbody>();
    }


    // =========================================================
    // INTERNAL
    // =========================================================
    private WeaponSlot GetSlotForLimb(LimbType limb)
    {
        for (int i = 0; i < weaponSlots.Length; i++)
        {
            if (weaponSlots[i].limbCost == limb)
                return weaponSlots[i];
        }
        return null;
    }
}

