using _Project.Code.Gameplay.CameraSystems.Profiles;
using _Project.Code.Core.ServiceLocator;
using _Project.Code.Core.Events;
using _Project.Code.Gameplay.Player;
using _Project.Code.Gameplay.Input;
using UnityEngine;
using Unity.Cinemachine;

namespace _Project.Code.Gameplay.CameraSystems
{
    [RequireComponent(typeof(CinemachineCamera))]
    public class GridCamera : MonoBehaviour
    {
        [SerializeField] private GridCameraProfile _profile;

        private CinemachineCamera _vcam;
        private CameraService _cameraService;
        private PlayerService _playerService;
        private Transform _followTarget;

        private Vector3 _currentPosition;
        private float _currentYRotation;
        private float _rotateInput;

        private void Awake()
        {
            _vcam = GetComponent<CinemachineCamera>();
        }

        private void Start()
        {
            _cameraService = ServiceLocator.Get<CameraService>();
            _playerService = ServiceLocator.Get<PlayerService>();

            SetupCinemachine();
            RegisterWithService();

            EventBus.Instance.Subscribe<PlayerRegisteredEvent>(this, OnPlayerRegistered);

            if (_profile.AllowRotation)
            {
                EventBus.Instance.Subscribe<CameraRotateInputEvent>(this, OnCameraRotateInput);
            }

            ConnectToPlayer();

            _currentYRotation = _profile.YRotation;
            SetupInitialPosition();
        }

        private void SetupCinemachine()
        {
            if (_vcam == null || _profile == null) return;

            var lens = _vcam.Lens;
            lens.FieldOfView = _profile.FieldOfView;
            _vcam.Lens = lens;
            _vcam.Priority = _profile.Priority;
        }

        private void RegisterWithService()
        {
            if (_cameraService != null && _vcam != null)
            {
                _cameraService.RegisterCamera(_vcam, setAsActive: true);
            }
        }

        private void ConnectToPlayer()
        {
            var player = _playerService.GetPlayerTransform();
            if (player != null)
            {
                _followTarget = player;
            }
        }

        private void OnPlayerRegistered(PlayerRegisteredEvent evt)
        {
            if (evt.Player != null)
            {
                _followTarget = evt.Player;
            }
        }

        private void OnCameraRotateInput(CameraRotateInputEvent evt)
        {
            _rotateInput = evt.Input;
        }

        private void SetupInitialPosition()
        {
            if (_profile == null || _followTarget == null) return;

            _currentPosition = CalculateTargetPosition();

            if (_vcam != null)
            {
                _vcam.transform.position = _currentPosition;
                _vcam.transform.rotation = CalculateRotation();
            }
        }

        private void LateUpdate()
        {
            if (_profile == null || _followTarget == null || _vcam == null) return;

            UpdateRotation();
            UpdatePosition();
        }

        private void UpdateRotation()
        {
            if (!_profile.AllowRotation) return;

            if (Mathf.Abs(_rotateInput) > 0.01f)
            {
                _currentYRotation += _rotateInput * _profile.RotationSpeed * Time.deltaTime;

                if (_profile.SnapRotation)
                {
                    _currentYRotation = Mathf.Round(_currentYRotation / 90f) * 90f;
                }
            }
        }

        private void UpdatePosition()
        {
            var targetPosition = CalculateTargetPosition();

            // Optionally snap to grid
            if (_profile.SnapToGrid)
            {
                targetPosition = SnapToGrid(targetPosition);
            }

            // Smooth follow
            _currentPosition = Vector3.Lerp(_currentPosition, targetPosition, _profile.FollowSmoothing * Time.deltaTime);

            // Apply bounds if enabled
            if (_profile.UseBounds)
            {
                _currentPosition.x = Mathf.Clamp(_currentPosition.x, _profile.MinBounds.x, _profile.MaxBounds.x);
                _currentPosition.y = Mathf.Clamp(_currentPosition.y, _profile.MinBounds.y, _profile.MaxBounds.y);
                _currentPosition.z = Mathf.Clamp(_currentPosition.z, _profile.MinBounds.z, _profile.MaxBounds.z);
            }

            _vcam.transform.position = _currentPosition;
            _vcam.transform.rotation = CalculateRotation();
        }

        private Vector3 CalculateTargetPosition()
        {
            var targetPos = _followTarget.position;

            // Calculate camera offset based on angle and rotation
            var angle = _profile.CameraAngle * Mathf.Deg2Rad;
            var yRotation = _currentYRotation * Mathf.Deg2Rad;

            var horizontalDistance = _profile.Distance * Mathf.Cos(angle);
            var verticalDistance = _profile.Distance * Mathf.Sin(angle);

            var offsetX = horizontalDistance * Mathf.Sin(yRotation);
            var offsetZ = horizontalDistance * Mathf.Cos(yRotation);

            return new Vector3(
                targetPos.x - offsetX,
                targetPos.y + verticalDistance + _profile.HeightOffset,
                targetPos.z - offsetZ
            );
        }

        private Quaternion CalculateRotation()
        {
            return Quaternion.Euler(_profile.CameraAngle, _currentYRotation, 0f);
        }

        private Vector3 SnapToGrid(Vector3 position)
        {
            var gridSize = _profile.GridSize;
            return new Vector3(
                Mathf.Round(position.x / gridSize) * gridSize,
                position.y,
                Mathf.Round(position.z / gridSize) * gridSize
            );
        }

        private void OnDestroy()
        {
            EventBus.Instance?.Unsubscribe<PlayerRegisteredEvent>(this);
            if (_profile != null && _profile.AllowRotation)
            {
                EventBus.Instance?.Unsubscribe<CameraRotateInputEvent>(this);
            }
        }
    }
}
