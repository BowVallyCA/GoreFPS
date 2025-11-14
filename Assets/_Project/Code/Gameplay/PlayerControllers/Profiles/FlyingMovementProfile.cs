using UnityEngine;

namespace _Project.Code.Gameplay.PlayerControllers.Profiles
{
    [CreateAssetMenu(fileName = "FlyingMovementProfile", menuName = "ScriptableObjects/Movement/Flying Movement Profile")]
    public class FlyingMovementProfile : MovementProfile
    {
        [field: SerializeField, Header("Flight Settings"), Tooltip("Maximum vertical speed when climbing")]
        public float ClimbRate { get; private set; } = 3f;

        [field: SerializeField, Tooltip("Maximum vertical speed when descending")]
        public float DescendRate { get; private set; } = 2f;

        [field: SerializeField, Tooltip("Horizontal speed when gliding")]
        public float GlideSpeed { get; private set; } = 8f;

        [field: SerializeField, Tooltip("Downward speed when gliding")]
        public float GlideDescendSpeed { get; private set; } = 1f;

        [field: SerializeField, Tooltip("Bank angle when turning (degrees)"), Range(0f, 90f)]
        public float BankAngle { get; private set; } = 30f;

        [field: SerializeField, Tooltip("Speed of banking rotation")]
        public float BankSpeed { get; private set; } = 5f;

        [field: SerializeField, Tooltip("Multiplier for lift forces when gliding")]
        public float LiftMultiplier { get; private set; } = 1.2f;

        [field: SerializeField, Header("Altitude"), Tooltip("Use altitude limits?")]
        public bool UseAltitudeLimits { get; private set; } = false;

        [field: SerializeField, Tooltip("Minimum altitude")]
        public float MinAltitude { get; private set; } = 0f;

        [field: SerializeField, Tooltip("Maximum altitude")]
        public float MaxAltitude { get; private set; } = 100f;

        [field: SerializeField, Header("Stamina (Optional)"), Tooltip("Use stamina system for flight?")]
        public bool UseStamina { get; private set; } = false;

        [field: SerializeField, Tooltip("Maximum stamina")]
        public float MaxStamina { get; private set; } = 100f;

        [field: SerializeField, Tooltip("Stamina drain per second when flying")]
        public float StaminaDrainRate { get; private set; } = 10f;

        [field: SerializeField, Tooltip("Stamina regen per second when grounded")]
        public float StaminaRegenRate { get; private set; } = 20f;
    }
}
