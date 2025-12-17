using System;
using UnityEngine;

public class LimbHealth : MonoBehaviour
{
    public static event Action<LimbType, int, int> OnLimbHealthChanged;

    [Header("Limb Health")]
    public LimbData head = new LimbData("Head", 50);
    public LimbData body = new LimbData("Body", 100);
    public LimbData arms = new LimbData("Arms", 60);
    public LimbData legs = new LimbData("Legs", 60);

    [Header("Type")]
    [SerializeField] public bool isPlayer = false;

    [Header("Player Only")]
    [SerializeField] private PlayerDebuffHandler debuffHandler;
    [SerializeField] private Animator flashAnimator;

    [Header("Weapon Drops")]
    [SerializeField] private WeaponSpawner weaponSpawner;
    [SerializeField] private Transform weaponDropPoint;

    [SerializeField] private int randomDmgThreshold = 20;
    private int randomDmgValue = 0;

    private UiManager uiManager;

    // Near-death (player only)
    public float deathCountdownTime = 5f;
    private float deathTimer;
    private bool isInNearDeath;
    private LimbType fatalLimb;

    private void Start()
    {
        head.Reset();
        body.Reset();
        arms.Reset();
        legs.Reset();

        if (isPlayer)
            uiManager = FindAnyObjectByType<UiManager>();

        if (weaponDropPoint == null)
            weaponDropPoint = transform;
    }

    // ----------------------------------------------------
    // DAMAGE
    // ----------------------------------------------------

    public void TakeDamageRandom(int amount)
    {
        ApplyDamageToLimbEnemyDMG(GetRandomLimb(), amount);
    }

    public void DamageSpecificLimb(LimbType limb, int amount)
    {
        ApplyDamageToLimb(limb, amount);
    }

    private void ApplyDamageToLimb(LimbType limb, int amount)
    {
        LimbData target = GetLimb(limb);
        if (target == null) return;

        target.CurrentHealth = Mathf.Max(target.CurrentHealth - amount, 0);

        // Player UI ONLY
        if (isPlayer)
        {
            OnLimbHealthChanged?.Invoke(limb, target.CurrentHealth, target.MaxHealth);
            debuffHandler?.EvaluateDebuffs(this);

            if (flashAnimator)
                flashAnimator.CrossFade("RedFlash", 0f);
        }

        // Limb death handling
        if (target.CurrentHealth <= 0)
        {
            if (isPlayer)
            {
                EnterNearDeathState(limb);
            }
            else
            {
                Die(); // enemies die immediately
            }
        }
    }

    private void ApplyDamageToLimbEnemyDMG(LimbType limb, int amount)
    {
        LimbData target = GetLimb(limb);
        if (target == null) return;

        // ---------------------------------
        // 1️⃣ ALWAYS APPLY DAMAGE
        // ---------------------------------
        target.CurrentHealth = Mathf.Max(target.CurrentHealth - amount, 0);

        // ---------------------------------
        // 2️⃣ ALWAYS UPDATE PLAYER UI / DEBUFFS
        // ---------------------------------
        if (isPlayer)
        {
            OnLimbHealthChanged?.Invoke(limb, target.CurrentHealth, target.MaxHealth);
            debuffHandler?.EvaluateDebuffs(this);

            if (flashAnimator)
                flashAnimator.CrossFade("RedFlash", 0f);
        }

        // ---------------------------------
        // 3️⃣ ACCUMULATE DAMAGE FOR DROP LOGIC
        // ---------------------------------
        randomDmgValue += amount;

        if (weaponSpawner != null && randomDmgValue >= randomDmgThreshold)
        {
            weaponSpawner.SpawnLimbWeaponToGround(limb);
            randomDmgValue = 0; // reset threshold
        }

        // ---------------------------------
        // 4️⃣ LIMB DEATH HANDLING
        // ---------------------------------
        if (target.CurrentHealth <= 0)
        {
            if (isPlayer)
            {
                EnterNearDeathState(limb);
            }
            else
            {
                Die();
            }
        }
    }

    // ----------------------------------------------------
    // HEALING (PLAYER ONLY)
    // ----------------------------------------------------

    public void HealLimb(LimbType limb, int amount)
    {
        LimbData target = GetLimb(limb);
        if (target == null) return;

        target.CurrentHealth = Mathf.Clamp(
            target.CurrentHealth + amount,
            0,
            target.MaxHealth
        );

        if (isPlayer)
        {
            OnLimbHealthChanged?.Invoke(limb, target.CurrentHealth, target.MaxHealth);
            debuffHandler?.EvaluateDebuffs(this);

            if (isInNearDeath && limb == fatalLimb && target.CurrentHealth > 0)
                ExitNearDeathState();
        }
    }

    // ----------------------------------------------------
    // PLAYER NEAR DEATH
    // ----------------------------------------------------

    private void EnterNearDeathState(LimbType limb)
    {
        if (isInNearDeath) return;

        isInNearDeath = true;
        fatalLimb = limb;
        deathTimer = deathCountdownTime;

        uiManager?.BeginDeathCountdown(deathTimer);
    }

    private void ExitNearDeathState()
    {
        isInNearDeath = false;
        uiManager?.CancelDeathCountdown();
    }

    private void Update()
    {
        if (!isPlayer || !isInNearDeath) return;

        deathTimer -= Time.deltaTime;
        uiManager?.UpdateDeathTimer(deathTimer);

        if (deathTimer <= 0f)
            Die();
    }

    protected virtual void Die()
    {
        if (isPlayer)
            uiManager?.ShowLoseScreen();

        Destroy(gameObject);
    }

    // ----------------------------------------------------
    // HELPERS
    // ----------------------------------------------------

    private LimbType GetRandomLimb()
    {
        LimbType[] limbs =
        {
            LimbType.Head,
            LimbType.Body,
            LimbType.Arms,
            LimbType.Legs
        };

        return limbs[UnityEngine.Random.Range(0, limbs.Length)];
    }

    private LimbData GetLimb(LimbType limb)
    {
        return limb switch
        {
            LimbType.Head => head,
            LimbType.Body => body,
            LimbType.Arms => arms,
            LimbType.Legs => legs,
            _ => body
        };
    }

    // ⚠️ REQUIRED — DO NOT REMOVE
    public float GetTotalHealthPercent()
    {
        float sum =
            head.CurrentHealth +
            body.CurrentHealth +
            arms.CurrentHealth +
            legs.CurrentHealth;

        float max =
            head.MaxHealth +
            body.MaxHealth +
            arms.MaxHealth +
            legs.MaxHealth;

        return sum / max;
    }
}






// ----------------------------------------------------------------------

[System.Serializable]
public class LimbData
{
    public string LimbName;
    public int MaxHealth;
    public int CurrentHealth;

    public LimbData(string name, int maxHealth)
    {
        LimbName = name;
        MaxHealth = maxHealth;
        CurrentHealth = maxHealth;
    }

    public void Reset()
    {
        CurrentHealth = MaxHealth;
    }

    public float HealthPercent => (float)CurrentHealth / MaxHealth;
}

// ----------------------------------------------------------------------

public enum LimbType
{
    Head,
    Body,
    Arms,
    Legs
}