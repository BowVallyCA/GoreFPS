using UnityEngine;
using _Project.Code.Gameplay.PlayerControllers.Base;
using _Project.Code.Gameplay.PlayerControllers.Profiles;
using _Project.Code.Core.StateMachine;
using _Project.Code.Core.ServiceLocator;
using _Project.Code.Core.Events;
using _Project.Code.Gameplay.Input;
using _Project.Code.Gameplay.CameraSystems;
using _Project.Code.Gameplay.Player;
using _Project.Code.Gameplay.PlayerControllers.ThirdPerson.States;

namespace _Project.Code.Gameplay.PlayerControllers.ThirdPerson
{
    [RequireComponent(typeof(CharacterControllerMotor), typeof(GroundCheck))]
    public class ThirdPersonController : BasePlayerController
    {
        [field: SerializeField, Header("Third Person Settings")]
        public ThirdPersonMovementProfile MovementProfile { get; private set; }

        private CharacterControllerMotor _motor;
        private PlayerService _playerService;
        private CameraService _cameraService;

        public CharacterControllerMotor Motor => _motor;
        public bool IsGrounded => GroundCheck != null && GroundCheck.IsGrounded;
        public bool CanJump => GroundCheck != null && GroundCheck.TimeSinceGrounded < MovementProfile.CoyoteTime;

        public bool IsSprinting { get; set; }
        public Vector2 MoveInput { get; set; }
        public bool IsJumpHeld { get; set; }
        public float LastJumpInputTime { get; set; }

        protected override void Awake()
        {
            base.Awake();
            _motor = GetComponent<CharacterControllerMotor>();
        }

        protected override void Start()
        {
            base.Start();

            // Register with PlayerService - CRITICAL for service architecture
            _playerService = ServiceLocator.Get<PlayerService>();
            _playerService.RegisterPlayer(this);

            _cameraService = ServiceLocator.Get<CameraService>();
        }

        public override void Initialize()
        {
            var idleState = new TPIdleState(this);
            StateMachine = new FiniteStateMachine<IState>(idleState);

            StateMachine.AddState(new TPWalkingState(this));
            StateMachine.AddState(new TPRunningState(this));
            StateMachine.AddState(new TPSprintingState(this));
            StateMachine.AddState(new TPJumpingState(this));
            StateMachine.AddState(new TPFallingState(this));
            StateMachine.AddState(new TPLockOnState(this));
            StateMachine.AddState(new TPLockOnJumpingState(this));

            EventBus.Instance.Subscribe<JumpInputEvent>(this, HandleJumpInput);
        }

        private void HandleJumpInput(JumpInputEvent evt)
        {
            IsJumpHeld = evt.IsPressed;
            if (evt.IsPressed)
            {
                LastJumpInputTime = Time.time;
            }
        }

        public Vector3 GetCameraRelativeMovement()
        {
            if (_cameraService == null) return Vector3.zero;
            var cameraTransform = _cameraService.GetCameraTransform();
            if (cameraTransform == null) return Vector3.zero;
            return GetCameraRelativeDirection(MoveInput, cameraTransform);
        }

        public void RotateTowardsMovement(Vector3 direction)
        {
            if (direction.magnitude > ServiceLocator.Get<InputService>().Profile.DirectionMagnitudeThreshold)
            {
                var targetRotation = Quaternion.LookRotation(direction);
                var rotationSpeed = MovementProfile.RotationSpeed * Time.deltaTime;
                transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, rotationSpeed);
            }
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            _playerService?.UnregisterPlayer(this);
            EventBus.Instance?.Unsubscribe<JumpInputEvent>(this);
        }
    }
}