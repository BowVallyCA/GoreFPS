using UnityEngine;
using UnityEngine.InputSystem;
using _Project.Code.Gameplay.PlayerControllers.Base;
using _Project.Code.Gameplay.PlayerControllers.Profiles;
using _Project.Code.Core.StateMachine;
using _Project.Code.Core.ServiceLocator;
using _Project.Code.Gameplay.CameraSystems;
using _Project.Code.Gameplay.Player;
using _Project.Code.Gameplay.PlayerControllers.PointAndClick.States;

namespace _Project.Code.Gameplay.PlayerControllers.PointAndClick
{
    [RequireComponent(typeof(NavMeshMotor))]
    public class PointAndClickController : BasePlayerController
    {
        [field: SerializeField, Header("Point & Click Settings")]
        public PointClickMovementProfile MovementProfile { get; private set; }

        private NavMeshMotor _motor;
        private PlayerService _playerService;
        private CameraService _cameraService;
        private Vector3 _targetPosition;
        private bool _hasDestination;
        private Mouse _mouse;

        public NavMeshMotor Motor => _motor;
        public Vector3 TargetPosition => _targetPosition;
        public bool HasDestination => _hasDestination;
        public bool HasReachedDestination => _motor.HasReachedDestination;

        protected override void Awake()
        {
            base.Awake();
            _motor = GetComponent<NavMeshMotor>();
        }

        protected override void Start()
        {
            base.Start();

            // Register with PlayerService
            _playerService = ServiceLocator.Get<PlayerService>();
            _playerService.RegisterPlayer(this);

            _cameraService = ServiceLocator.Get<CameraService>();

            _motor.SetStoppingDistance(MovementProfile.StoppingDistance);

            // Get mouse from new Input System
            _mouse = Mouse.current;
        }

        public override void Initialize()
        {
            var idleState = new PCIdleState(this);
            StateMachine = new FiniteStateMachine<IState>(idleState);

            StateMachine.AddState(new PCMovingState(this));
        }

        protected override void Update()
        {
            base.Update();
            HandleMouseInput();
        }

        private void HandleMouseInput()
        {
            if (_mouse == null) return;

            if (_mouse.leftButton.wasPressedThisFrame)
            {
                var cameraTransform = _cameraService.GetCameraTransform();
                if (cameraTransform == null) return;

                var camera = cameraTransform.GetComponent<UnityEngine.Camera>();
                if (camera == null) return;

                var ray = camera.ScreenPointToRay(_mouse.position.ReadValue());

                if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, MovementProfile.ClickableLayers))
                {
                    SetDestination(hit.point);
                }
            }
        }

        public void SetDestination(Vector3 destination)
        {
            _targetPosition = destination;
            _hasDestination = true;
            _motor.SetDestination(destination);
        }

        public void ClearDestination()
        {
            _hasDestination = false;
            _motor.Stop();
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            _playerService?.UnregisterPlayer(this);
        }
    }
}
