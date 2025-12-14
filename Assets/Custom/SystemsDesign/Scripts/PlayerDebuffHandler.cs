using _Project.Code.Gameplay.PlayerControllers.Base;
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

    // The movement script that controls speed for CharacterController movement
    private CharacterControllerMotor motor;

    private void Start()
    {
        limbHealth = GetComponent<LimbHealth>();
        motor = GetComponent<CharacterControllerMotor>();

        LimbHealth.OnLimbHealthChanged += OnLimbHealthChanged;

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

    // ---------------------------------------------------------
    // LEG DEBUFF — APPLIES SPEED MULTIPLIER TO CHARACTERCONTROLLER MOVEMENT
    // ---------------------------------------------------------
    private void HandleLegDebuff(LimbHealth limbs)
    {
        bool low = limbs.legs.HealthPercent < legCriticalThreshold;

        if (motor != null)
        {
            motor.speedMultiplier = low ? legSlowMultiplier : 1f;
        }
    }

    // ---------------------------------------------------------
    // ARM DEBUFF — APPLIES RECOIL MULTIPLIER
    // ---------------------------------------------------------
    private void HandleArmDebuff(LimbHealth limbs)
    {
        bool low = limbs.arms.HealthPercent < armCriticalThreshold;

        //if (movement != null)
        //{
        //    movement.recoilMultiplier = low ? armRecoilMultiplier : 1f;
        //}
    }

    // ---------------------------------------------------------
    // HEAD DEBUFF — BLUR / SCREEN EFFECT
    // ---------------------------------------------------------
    private void HandleHeadDebuff(LimbHealth limbs)
    {
        bool low = limbs.head.HealthPercent < headCriticalThreshold;

        if (blurOverlay != null)
        {
            blurOverlay.alpha = low ? 1f : 0f;
        }
    }
}
