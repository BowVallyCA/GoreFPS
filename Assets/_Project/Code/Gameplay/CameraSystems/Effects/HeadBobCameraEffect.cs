using UnityEngine;
using _Project.Code.Core.ServiceLocator;
using _Project.Code.Gameplay.Player;
using _Project.Code.Gameplay.PlayerControllers.Base;
using _Project.Code.Gameplay.PlayerControllers.Profiles;
using _Project.Code.Gameplay.PlayerControllers.ThirdPerson;

namespace _Project.Code.Gameplay.CameraSystems.Effects
{
    [CreateAssetMenu(fileName = "HeadBobEffect", menuName = "ScriptableObjects/Camera/Effects/Head Bob")]
    public class HeadBobCameraEffect : CameraEffectStrategy
    {
        [field: SerializeField, Header("Bob Settings")]
        [field: Tooltip("Bob cycles per second at walk speed")]
        [field: Range(0.5f, 4f)]
        public float BobCyclesPerSecond { get; private set; } = 2f;

        [field: SerializeField]
        [field: Tooltip("Vertical bob distance in meters")]
        [field: Range(0.01f, 0.2f)]
        public float VerticalBobDistance { get; private set; } = 0.05f;

        [field: SerializeField]
        [field: Tooltip("Horizontal bob distance in meters")]
        [field: Range(0.01f, 0.1f)]
        public float HorizontalBobDistance { get; private set; } = 0.025f;

        [field: SerializeField, Header("Activation")]
        [field: Tooltip("Minimum speed in m/s to activate head bob")]
        public float MinSpeedThreshold { get; private set; } = 0.5f;

        [field: SerializeField]
        [field: Tooltip("Only bob when grounded")]
        public bool OnlyWhenGrounded { get; private set; } = true;

        [field: SerializeField, Header("Smoothing")]
        [field: Tooltip("Time in seconds to fade bob in/out")]
        [field: Range(0.1f, 2f)]
        public float FadeTime { get; private set; } = 0.2f;

        private float _bobPhase;
        private float _currentBobIntensity;
        private bool _isActive;
        private IMotor _motor;
        private GroundCheck _groundCheck;
        private Vector3 _originalLocalPosition;
        private float _walkSpeed;

        public override bool IsActive => _isActive;

        public override void Initialize()
        {
            _bobPhase = 0f;
            _currentBobIntensity = 0f;
            _isActive = false;
        }

        public override void Reset()
        {
            _bobPhase = 0f;
            _currentBobIntensity = 0f;
            _isActive = false;
        }

        public override void OnCameraDeactivated(MonoBehaviour camera)
        {
            _motor = null;
            _groundCheck = null;
        }

        public override void Execute(CameraEffectContext context)
        {
            if (!EnsurePlayerReferences(context)) return;

            float speed = CalculateHorizontalSpeed();
            bool isGrounded = _groundCheck.IsGrounded;

            UpdateBobIntensity(speed, isGrounded, context.DeltaTime);

            if (_currentBobIntensity < 0.01f)
            {
                context.CameraTransform.localPosition = _originalLocalPosition;
                return;
            }

            ApplyBobOffset(context, speed);
        }

        private bool EnsurePlayerReferences(CameraEffectContext context)
        {
            if (_motor != null && _groundCheck != null) return true;

            var playerService = ServiceLocator.Get<PlayerService>();
            var player = playerService.GetPlayerTransform();
            if (player == null) return false;

            _motor = player.GetComponent<IMotor>();
            _groundCheck = player.GetComponent<GroundCheck>();
            _originalLocalPosition = context.CameraTransform.localPosition;

            var controller = player.GetComponent<BasePlayerController>();
            var movementProfile = controller?.GetType().GetProperty("MovementProfile")?.GetValue(controller) as MovementProfile;
            _walkSpeed = movementProfile?.WalkSpeed ?? 1f;

            return true;
        }

        private float CalculateHorizontalSpeed()
        {
            return new Vector3(_motor.Velocity.x, 0f, _motor.Velocity.z).magnitude;
        }

        private void UpdateBobIntensity(float speed, bool isGrounded, float deltaTime)
        {
            bool shouldBob = speed >= MinSpeedThreshold;
            if (OnlyWhenGrounded)
            {
                shouldBob = shouldBob && isGrounded;
            }

            _isActive = shouldBob;

            float targetIntensity = shouldBob ? 1f : 0f;
            float fadeSpeed = 1f / FadeTime;
            _currentBobIntensity = Mathf.Lerp(_currentBobIntensity, targetIntensity, fadeSpeed * deltaTime);
        }

        private void ApplyBobOffset(CameraEffectContext context, float speed)
        {
            if (_currentBobIntensity > 0.01f)
            {
                float speedBasedFrequency = BobCyclesPerSecond * (speed / _walkSpeed);
                _bobPhase += context.DeltaTime * speedBasedFrequency * Mathf.PI * 2f;
            }

            float verticalBob = Mathf.Sin(_bobPhase) * VerticalBobDistance * _currentBobIntensity;
            float horizontalBob = Mathf.Cos(_bobPhase * 0.5f) * HorizontalBobDistance * _currentBobIntensity;

            Vector3 bobOffset = new Vector3(horizontalBob, verticalBob, 0f);
            context.CameraTransform.localPosition = _originalLocalPosition + bobOffset;
        }
    }
}
