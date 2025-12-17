using _Project.Code.Gameplay.CameraSystems;
using _Project.Code.Gameplay.PlayerControllers.Base;
using UnityEngine;

public class PlayerDebuffHandler : MonoBehaviour
{
    [Header("Leg Debuff (Movement)")]
    public float legCriticalThreshold = 0.3f;
    public float legSlowMultiplier = 0.5f;

    [Header("Arm Debuff (Drop Weapon)")]
    public float armCriticalThreshold = 0.3f;
    public AudioSource armDebuffAudio;

    [Header("Head Debuff (UI Fade)")]
    [Tooltip("CanvasGroup controlling ALL player UI")]
    public CanvasGroup uiCanvasGroup;
    public float minUIAlpha = 0.25f;

    [Header("Body Debuff (Look Sensitivity)")]
    public float bodyCriticalThreshold = 0.3f;
    public float bodySensitivityMultiplier = 0.5f; // used later with AimCamera

    private LimbHealth limbHealth;
    private CharacterControllerMotor motor;
    private HoldObjectScript holdObject;
    private AimCamera aimCamera;

    // State guards
    private bool armDebuffTriggered = false;

    private void Start()
    {
        limbHealth = GetComponent<LimbHealth>();
        motor = GetComponent<CharacterControllerMotor>();
        holdObject = GetComponent<HoldObjectScript>();
        aimCamera = GetComponent<AimCamera>();

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

    // =========================================================
    // MASTER EVALUATION
    // =========================================================
    public void EvaluateDebuffs(LimbHealth limbs)
    {
        HandleLegDebuff(limbs);
        HandleArmDebuff(limbs);
        HandleHeadDebuff(limbs);
        HandleBodyDebuff(limbs);
    }

    // =========================================================
    // LEG — MOVEMENT SLOW
    // =========================================================
    private void HandleLegDebuff(LimbHealth limbs)
    {
        bool low = limbs.legs.HealthPercent < legCriticalThreshold;

        if (motor != null)
            motor.speedMultiplier = low ? legSlowMultiplier : 1f;
    }

    // =========================================================
    // ARM — FORCE DROP WEAPON + AUDIO
    // =========================================================
    private void HandleArmDebuff(LimbHealth limbs)
    {
        bool low = limbs.arms.HealthPercent < armCriticalThreshold;

        if (low && !armDebuffTriggered)
        {
            armDebuffTriggered = true;

            // Force weapon drop
            if (holdObject != null && holdObject.GetHeldObject() != null)
                holdObject.ThrowObject();

            // Audio feedback
            if (armDebuffAudio != null)
                armDebuffAudio.Play();
        }
        else if (!low)
        {
            armDebuffTriggered = false;
        }
    }

    // =========================================================
    // HEAD — UI TRANSPARENCY BASED ON HEALTH
    // =========================================================
    private void HandleHeadDebuff(LimbHealth limbs)
    {
        if (uiCanvasGroup == null) return;

        float percent = Mathf.Clamp01(limbs.head.HealthPercent);

        // Higher damage → more transparent UI
        float alpha = Mathf.Lerp(minUIAlpha, 1f, percent);
        uiCanvasGroup.alpha = alpha;
    }

    // =========================================================
    // BODY — LOOK SENSITIVITY (HOOK ONLY)
    // =========================================================
    private void HandleBodyDebuff(LimbHealth limbs)
    {
        bool low = limbs.body.HealthPercent < bodyCriticalThreshold;

        //aimCamera.SetSensitivityMultiplier(low ? bodySensitivityMultiplier : 1f);

        /*
         * This is intentionally a hook.
         * When you're ready, AimCamera should expose something like:
         *
         *   SetSensitivityMultiplier(float value)
         *
         * Then you do:
         *
         * aimCamera.SetSensitivityMultiplier(
         *     low ? bodySensitivityMultiplier : 1f
         * );
         *
         * This avoids hard-coding camera internals here.
         */
    }
}
