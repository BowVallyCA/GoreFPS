using UnityEngine;
using _Project.Code.Gameplay.PlayerControllers.Base;
using _Project.Code.Gameplay.PlayerControllers.Profiles;
using _Project.Code.Core.StateMachine;
using _Project.Code.Core.ServiceLocator;
using _Project.Code.Core.Events;
using _Project.Code.Gameplay.Input;
using _Project.Code.Gameplay.CameraSystems;
using _Project.Code.Gameplay.Player;
using _Project.Code.Gameplay.PlayerControllers.TopDown.States;

namespace _Project.Code.Gameplay.PlayerControllers.TopDown
{
    [RequireComponent(typeof(CharacterControllerMotor), typeof(GroundCheck))]
    public class TopDownController : BasePlayerController
    {
        [field: SerializeField, Header("Top Down Settings")]
        public TopDownMovementProfile MovementProfile { get; private set; }

        private CharacterControllerMotor _motor;
        private PlayerService _playerService;
        private Vector2 _aimInput;

        public CharacterControllerMotor Motor => _motor;
        public bool IsGrounded => GroundCheck != null && GroundCheck.IsGrounded;
        public Vector2 MoveInput { get; set; }
        public Vector2 AimInput => _aimInput;
        public bool IsMoving => MoveInput.magnitude > ServiceLocator.Get<InputService>().Profile.MovementMagnitudeThreshold;

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
        }

        public override void Initialize()
        {
            var idleState = new TDIdleState(this);
            StateMachine = new FiniteStateMachine<IState>(idleState);

            StateMachine.AddState(new TDMovingState(this));
            StateMachine.AddState(new TDFallingState(this));

            EventBus.Instance.Subscribe<LookInputEvent>(this, HandleAim);
        }

        private void HandleAim(LookInputEvent evt)
        {
            _aimInput = evt.Input;
        }

        public Vector3 GetMovementDirection()
        {
            if (MovementProfile.UseWorldSpaceMovement)
            {
                // Direct world-space movement
                return new Vector3(MoveInput.x, 0f, MoveInput.y);
            }
            else
            {
                // Camera-relative movement (if camera can rotate)
                var cameraService = ServiceLocator.Get<CameraService>();
                var cameraTransform = cameraService?.GetCameraTransform();
                if (cameraTransform != null)
                {
                    return GetCameraRelativeDirection(MoveInput, cameraTransform);
                }
                return new Vector3(MoveInput.x, 0f, MoveInput.y);
            }
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

        public void RotateTowardsAim()
        {
            // For twin-stick aiming
            if (_aimInput.magnitude > ServiceLocator.Get<InputService>().Profile.DirectionMagnitudeThreshold)
            {
                var aimDirection = new Vector3(_aimInput.x, 0f, _aimInput.y);
                var targetRotation = Quaternion.LookRotation(aimDirection);
                var rotationSpeed = MovementProfile.RotationSpeed * Time.deltaTime;
                transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, rotationSpeed);
            }
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            _playerService?.UnregisterPlayer(this);

            EventBus.Instance?.Unsubscribe<LookInputEvent>(this);
        }
    }
}