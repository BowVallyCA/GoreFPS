using System;
using Unity.Burst.Intrinsics;
using UnityEngine;

public class LimbHealth : MonoBehaviour
{
    public static event System.Action<LimbType, int, int> OnLimbHealthChanged; // make sure this exists

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

    private void Start()
    {
        head.Reset();
        body.Reset();
        arms.Reset();
        legs.Reset();

        // Fire initial events so UI updates correctly
        //OnLimbHealthChanged?.Invoke(LimbType.Head, head.CurrentHealth, head.MaxHealth);
        //OnLimbHealthChanged?.Invoke(LimbType.Body, body.CurrentHealth, body.MaxHealth);
        //OnLimbHealthChanged?.Invoke(LimbType.Arms, arms.CurrentHealth, arms.MaxHealth);
        //OnLimbHealthChanged?.Invoke(LimbType.Legs, legs.CurrentHealth, legs.MaxHealth);

        if (isPlayer && debuffHandler == null)
            Debug.LogWarning("Player has no debuff handler assigned.");
    }

    // ----------------------------------------------------------------------
    // Damage Functions
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

    public void HealLimb(LimbType limb, int amount)
    {
        LimbData target = GetLimb(limb);
        if (target == null) return;

        target.CurrentHealth = Mathf.Clamp(target.CurrentHealth + amount, 0, target.MaxHealth);
        OnLimbHealthChanged?.Invoke(limb, target.CurrentHealth, target.MaxHealth);

        if (isPlayer)
            debuffHandler?.EvaluateDebuffs(this);
    }

    private void ApplyDamageToLimb(LimbType limb, int amount)
    {
        LimbData target = GetLimb(limb);

        target.CurrentHealth -= amount;
        target.CurrentHealth = Mathf.Max(target.CurrentHealth, 0);

        OnLimbHealthChanged?.Invoke(limb, target.CurrentHealth, target.MaxHealth);

        if (flashAnimator)
            flashAnimator.CrossFade("RedFlash", 0f);

        if (isPlayer)
            debuffHandler?.EvaluateDebuffs(this);

        if (IsDead())
            Die();
    }

    public void DamageSpecificLimb(LimbType limb, int amount)
    {
        LimbData target = GetLimb(limb);
        if (target == null) return;

        target.CurrentHealth -= amount;
        target.CurrentHealth = Mathf.Clamp(target.CurrentHealth, 0, target.MaxHealth);

        // notify UI and other listeners
        OnLimbHealthChanged?.Invoke(limb, target.CurrentHealth, target.MaxHealth);

        // Re-evaluate player debuffs if you have a debuff handler
        if (isPlayer)
            debuffHandler?.EvaluateDebuffs(this);

        if (IsDead())
            Die();
    }

    // ----------------------------------------------------------------------

    private bool IsDead()
    {
        return head.CurrentHealth <= 0 || body.CurrentHealth <= 0;
    }

    protected virtual void Die()
    {
        Debug.Log($"{gameObject.name} has died.");
    }

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
        float sum = head.CurrentHealth +
                    body.CurrentHealth +
                    arms.CurrentHealth +
                    legs.CurrentHealth;

        float max = head.MaxHealth +
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