using System;
using UnityEngine;

public class LimbHealth : MonoBehaviour
{
    public static event Action<LimbType, int, int> OnLimbHealthChanged;

    [Header("Limb Health Settings")]
    public LimbData head = new LimbData("Head", 50);
    public LimbData body = new LimbData("Body", 100);
    public LimbData arms = new LimbData("Arms", 60);
    public LimbData legs = new LimbData("Legs", 60);

    [Header("Player Settings")]
    [SerializeField] private bool isPlayer = false;
    [SerializeField] private PlayerDebuffHandler debuffHandler;

    [Header("Optional Damage Flash")]
    [SerializeField] private Animator flashAnimator;

    [Header("Near Death Settings")]
    public float deathCountdownTime = 5f;
    private float deathTimer = 0f;
    private bool isInNearDeath = false;
    private LimbType fatalLimb;

    private UiManager uiManager;

    private void Start()
    {
        head.Reset();
        body.Reset();
        arms.Reset();
        legs.Reset();

        uiManager = FindAnyObjectByType<UiManager>();

        if (isPlayer && debuffHandler == null)
            Debug.LogWarning("Player has no debuff handler assigned.");
    }

    // ----------------------------------------------------------------------
    // DAMAGE
    // ----------------------------------------------------------------------

    public void TakeDamageRandom(int amount)
    {
        LimbType randomLimb = GetRandomLimb();
        ApplyDamageToLimb(randomLimb, amount);
    }

    public void TakeDamageToLimb(LimbType limb, int amount)
    {
        ApplyDamageToLimb(limb, amount);
    }

    public void DamageSpecificLimb(LimbType limb, int amount)
    {
        ApplyDamageToLimb(limb, amount);
    }

    private void ApplyDamageToLimb(LimbType limb, int amount)
    {
        LimbData target = GetLimb(limb);
        target.CurrentHealth = Mathf.Max(target.CurrentHealth - amount, 0);

        OnLimbHealthChanged?.Invoke(limb, target.CurrentHealth, target.MaxHealth);

        if (flashAnimator)
            flashAnimator.CrossFade("RedFlash", 0f);

        if (isPlayer)
            debuffHandler?.EvaluateDebuffs(this);

        // Check for limb death
        if (!isInNearDeath && target.CurrentHealth <= 0)
        {
            EnterNearDeathState(limb);
        }
    }

    // ----------------------------------------------------------------------
    // HEALING
    // ----------------------------------------------------------------------

    public void HealLimb(LimbType limb, int amount)
    {
        LimbData target = GetLimb(limb);
        if (target == null) return;

        bool wasDeadLimb = (target.CurrentHealth <= 0);

        target.CurrentHealth = Mathf.Clamp(target.CurrentHealth + amount, 0, target.MaxHealth);
        OnLimbHealthChanged?.Invoke(limb, target.CurrentHealth, target.MaxHealth);

        if (isPlayer)
            debuffHandler?.EvaluateDebuffs(this);

        // Escape near-death
        if (isInNearDeath && limb == fatalLimb && target.CurrentHealth > 0)
        {
            ExitNearDeathState();
        }
    }

    // ----------------------------------------------------------------------
    // NEAR-DEATH HANDLING
    // ----------------------------------------------------------------------

    private void EnterNearDeathState(LimbType limb)
    {
        isInNearDeath = true;
        fatalLimb = limb;
        deathTimer = deathCountdownTime;

        uiManager?.BeginDeathCountdown(deathTimer);

        Debug.Log($"Player entered near-death due to {limb}!");
    }

    private void ExitNearDeathState()
    {
        isInNearDeath = false;
        uiManager?.CancelDeathCountdown();
        Debug.Log("Player recovered from near-death!");
    }

    private void Update()
    {
        if (!isInNearDeath) return;

        deathTimer -= Time.deltaTime;

        uiManager?.UpdateDeathTimer(deathTimer);

        if (deathTimer <= 0f)
        {
            isInNearDeath = false;
            Die();
        }
    }

    protected virtual void Die()
    {
        Debug.Log($"{gameObject.name} has died.");
        uiManager?.ShowLoseScreen();
    }

    // ----------------------------------------------------------------------

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

    public float GetTotalHealthPercent()
    {
        float sum = head.CurrentHealth + body.CurrentHealth + arms.CurrentHealth + legs.CurrentHealth;
        float max = head.MaxHealth + body.MaxHealth + arms.MaxHealth + legs.MaxHealth;
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