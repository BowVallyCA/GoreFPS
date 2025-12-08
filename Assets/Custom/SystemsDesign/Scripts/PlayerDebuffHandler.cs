using UnityEngine;

public class PlayerDebuffHandler : MonoBehaviour
{
    [Header("Movement Debuff (Legs)")]
    public float legCriticalThreshold = 0.3f;
    public float legSlowMultiplier = 0.5f;

    [Header("Aim Debuff (Arms)")]
    public float armCriticalThreshold = 0.3f;
    public float armRecoilMultiplier = 1.5f;

    [Header("Vision Debuff (Head)")]
    public float headCriticalThreshold = 0.3f;
    public CanvasGroup blurOverlay;

    private LimbHealth limbHealth;

    // Optional references (assign if you want these debuffs to actually do something)
    //private PlayerMovement movement;
    //private PlayerGunController gun;

    private void Start()
    {
        limbHealth = GetComponent<LimbHealth>();
        //movement = GetComponent<PlayerMovement>();
        //gun = GetComponent<PlayerGunController>();

        // If LimbHealth supports event callbacks, hook into it
        LimbHealth.OnLimbHealthChanged += OnLimbHealthChanged;

        // Initialize with correct debuffs
        if (limbHealth != null)
            EvaluateDebuffs(limbHealth);
    }

    private void OnDestroy()
    {
        LimbHealth.OnLimbHealthChanged -= OnLimbHealthChanged;
    }

    private void OnLimbHealthChanged(LimbType limb, int current, int max)
    {
        if (limbHealth != null)
            EvaluateDebuffs(limbHealth);
    }

    public void EvaluateDebuffs(LimbHealth limbs)
    {
        HandleLegDebuff(limbs);
        HandleArmDebuff(limbs);
        HandleHeadDebuff(limbs);
    }

    private void HandleLegDebuff(LimbHealth limbs)
    {
        bool low = limbs.legs.HealthPercent < legCriticalThreshold;

        //if (movement != null)
        //{
        //    movement.speedMultiplier = low ? legSlowMultiplier : 1f;
        //}
    }

    private void HandleArmDebuff(LimbHealth limbs)
    {
        bool low = limbs.arms.HealthPercent < armCriticalThreshold;

        //if (gun != null)
        //{
        //    gun.recoilMultiplier = low ? armRecoilMultiplier : 1f;
        //}
    }

    private void HandleHeadDebuff(LimbHealth limbs)
    {
        bool low = limbs.head.HealthPercent < headCriticalThreshold;

        if (blurOverlay != null)
        {
            blurOverlay.alpha = low ? 1f : 0f;
        }
    }
}
