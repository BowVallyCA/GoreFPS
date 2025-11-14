using UnityEngine;
using _Project.Code.Gameplay.PlayerControllers.Base;
using _Project.Code.Gameplay.PlayerControllers.Profiles;
using _Project.Code.Core.StateMachine;
using _Project.Code.Core.Events;
using _Project.Code.Core.ServiceLocator;
using _Project.Code.Gameplay.Input;
using _Project.Code.Gameplay.CameraSystems;
using _Project.Code.Gameplay.Player;
using _Project.Code.Gameplay.PlayerControllers.Grid.States;
using System.Collections.Generic;
using DG.Tweening;

namespace _Project.Code.Gameplay.PlayerControllers.Grid
{
    [RequireComponent(typeof(KinematicMotor), typeof(GridCamera))]
    public class GridController : BasePlayerController
    {
        [field: SerializeField, Header("Grid Settings")]
        public GridMovementProfile MovementProfile { get; private set; }

        [field: SerializeField]
        public Transform CameraReference { get; private set; }

        private KinematicMotor _motor;
        private GridCamera _camera;
        private PlayerService _playerService;
        private CameraService _cameraService;
        private Vector3 _currentGridPosition;
        private bool _isMoving;
        private Queue<Vector2> _inputQueue = new Queue<Vector2>();

        public KinematicMotor Motor => _motor;
        public GridCamera Camera => _camera;
        public Vector2 MoveInput { get; set; }
        public Vector3 CurrentGridPosition => _currentGridPosition;
        public bool IsMoving => _isMoving;

        protected override void Awake()
        {
            base.Awake();
            _motor = GetComponent<KinematicMotor>();
            _camera = GetComponent<GridCamera>();
        }

        protected override void Start()
        {
            base.Start();

            // Register with PlayerService
            _playerService = ServiceLocator.Get<PlayerService>();
            _playerService.RegisterPlayer(this);

            _cameraService = ServiceLocator.Get<CameraService>();

            // Snap to grid
            _currentGridPosition = SnapToGrid(transform.position);
            transform.position = _currentGridPosition;

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
            var idleState = new GridIdleState(this);
            StateMachine = new FiniteStateMachine<IState>(idleState);

            StateMachine.AddState(new GridMovingState(this));

            EventBus.Instance.Subscribe<MoveInputEvent>(this, HandleMove);
        }

        private void HandleMove(MoveInputEvent evt)
        {
            MoveInput = evt.Input;
        }

        public Vector3 SnapToGrid(Vector3 position)
        {
            var gridSize = MovementProfile.GridSize;
            return new Vector3(
                Mathf.Round(position.x / gridSize) * gridSize,
                position.y,
                Mathf.Round(position.z / gridSize) * gridSize
            );
        }

        public Vector3 GetGridDirection()
        {
            if (CameraReference == null) return Vector3.zero;

            var cameraForward = CameraReference.forward;
            var cameraRight = CameraReference.right;

            cameraForward.y = 0f;
            cameraRight.y = 0f;
            cameraForward.Normalize();
            cameraRight.Normalize();

            var direction = (cameraRight * MoveInput.x + cameraForward * MoveInput.y);

            // Snap to cardinal directions
            if (Mathf.Abs(direction.x) > Mathf.Abs(direction.z))
            {
                return direction.x > 0 ? Vector3.right : Vector3.left;
            }
            else if (Mathf.Abs(direction.z) > 0.01f)
            {
                return direction.z > 0 ? Vector3.forward : Vector3.back;
            }

            return Vector3.zero;
        }

        public bool CanMoveToPosition(Vector3 targetPosition)
        {
            if (!MovementProfile.CheckCollisions) return true;

            var direction = (targetPosition - transform.position).normalized;
            var distance = MovementProfile.GridSize;

            return !Physics.Raycast(transform.position + Vector3.up * 0.5f, direction, distance, MovementProfile.CollisionLayers);
        }

        public void MoveToGridPosition(Vector3 targetPosition)
        {
            _isMoving = true;
            _currentGridPosition = targetPosition;

            var sequence = DOTween.Sequence();

            if (MovementProfile.UseHopAnimation)
            {
                // Hop animation
                var midPoint = (transform.position + targetPosition) * 0.5f;
                midPoint.y += MovementProfile.HopHeight;

                sequence.Append(transform.DOMove(midPoint, MovementProfile.StepDuration * 0.5f).SetEase(Ease.OutQuad));
                sequence.Append(transform.DOMove(targetPosition, MovementProfile.StepDuration * 0.5f).SetEase(Ease.InQuad));
            }
            else
            {
                sequence.Append(transform.DOMove(targetPosition, MovementProfile.StepDuration).SetEase(MovementProfile.MoveEase));
            }

            sequence.OnComplete(() => _isMoving = false);
        }

        public void RotateToDirection(Vector3 direction)
        {
            if (direction.sqrMagnitude < 0.01f) return;

            var targetRotation = Quaternion.LookRotation(direction);
            transform.DORotateQuaternion(targetRotation, MovementProfile.TurnDuration).SetEase(MovementProfile.RotateEase);
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            _playerService?.UnregisterPlayer(this);
            EventBus.Instance?.Unsubscribe<MoveInputEvent>(this);
            DOTween.Kill(transform);
        }
    }
}
