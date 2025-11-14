using UnityEngine;
using _Project.Code.Gameplay.PlayerControllers.Base;
using _Project.Code.Gameplay.PlayerControllers.Profiles;
using _Project.Code.Core.StateMachine;
using _Project.Code.Core.Events;
using _Project.Code.Core.ServiceLocator;
using _Project.Code.Gameplay.Input;
using _Project.Code.Gameplay.CameraSystems;
using _Project.Code.Gameplay.Player;
using _Project.Code.Gameplay.PlayerControllers.Flying.States;

namespace _Project.Code.Gameplay.PlayerControllers.Flying
{
    [RequireComponent(typeof(RigidbodyMotor), typeof(FlyingCamera))]
    public class FlyingController : BasePlayerController
    {
        [field: SerializeField, Header("Flying Settings")]
        public FlyingMovementProfile MovementProfile { get; private set; }

        [field: SerializeField]
        public Transform CameraReference { get; private set; }

        private RigidbodyMotor _motor;
        private FlyingCamera _camera;
        private PlayerService _playerService;
        private CameraService _cameraService;
        private bool _isBoostPressed;
        private float _currentPitch;
        private float _currentRoll;

        public RigidbodyMotor Motor => _motor;
        public FlyingCamera Camera => _camera;
        public bool IsBoosting => _isBoostPressed;
        public Vector2 MoveInput { get; set; }
        public Vector2 LookInput { get; set; }
        public float CurrentPitch => _currentPitch;
        public float CurrentRoll => _currentRoll;

        protected override void Awake()
        {
            base.Awake();
            _motor = GetComponent<RigidbodyMotor>();
            _camera = GetComponent<FlyingCamera>();
        }

        protected override void Start()
        {
            base.Start();

            // Register with PlayerService
            _playerService = ServiceLocator.Get<PlayerService>();
            _playerService.RegisterPlayer(this);

            _cameraService = ServiceLocator.Get<CameraService>();

            // Get camera reference from service
            if (CameraReference == null)
            {
                var cameraTransform = _cameraService.GetCameraTransform();
                if (cameraTransform != null)
                {
                    CameraReference = cameraTransform;
                }
            }
        }

        public override void Initialize()
        {
            var idleState = new FlyIdleState(this);
            StateMachine = new FiniteStateMachine<IState>(idleState);

            StateMachine.AddState(new FlyFlyingState(this));
            StateMachine.AddState(new FlyBoostingState(this));

            EventBus.Instance.Subscribe<SprintInputEvent>(this, HandleBoost);
        }

        private void HandleBoost(SprintInputEvent evt)
        {
            _isBoostPressed = evt.IsPressed;
        }

        public void ApplyPitchAndRoll(float pitchInput, float rollInput)
        {
            // Update pitch based on vertical input
            _currentPitch += pitchInput * MovementProfile.RotationSpeed * Time.deltaTime;
            _currentPitch = Mathf.Clamp(_currentPitch, -80f, 80f);

            // Update roll based on horizontal input
            var targetRoll = -rollInput * MovementProfile.BankAngle;
            _currentRoll = Mathf.Lerp(_currentRoll, targetRoll, MovementProfile.BankSpeed * Time.deltaTime);

            // Apply rotation
            var yawRotation = transform.eulerAngles.y + rollInput * MovementProfile.RotationSpeed * Time.deltaTime;
            transform.rotation = Quaternion.Euler(_currentPitch, yawRotation, _currentRoll);
        }

        public void ApplyAltitudeLimits()
        {
            if (!MovementProfile.UseAltitudeLimits) return;

            var pos = transform.position;
            if (pos.y < MovementProfile.MinAltitude)
            {
                pos.y = MovementProfile.MinAltitude;
                transform.position = pos;
                _motor.SetVelocity(new Vector3(_motor.Velocity.x, 0f, _motor.Velocity.z));
            }
            else if (pos.y > MovementProfile.MaxAltitude)
            {
                pos.y = MovementProfile.MaxAltitude;
                transform.position = pos;
                _motor.SetVelocity(new Vector3(_motor.Velocity.x, 0f, _motor.Velocity.z));
            }
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            _playerService?.UnregisterPlayer(this);
            EventBus.Instance?.Unsubscribe<SprintInputEvent>(this);
        }
    }
}
