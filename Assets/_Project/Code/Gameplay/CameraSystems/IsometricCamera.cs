using _Project.Code.Gameplay.CameraSystems.Profiles;
using _Project.Code.Core.ServiceLocator;
using _Project.Code.Core.Events;
using _Project.Code.Gameplay.Player;
using UnityEngine;
using UnityEngine.InputSystem;
using Unity.Cinemachine;

namespace _Project.Code.Gameplay.CameraSystems
{
    [RequireComponent(typeof(CinemachineCamera))]
    public class IsometricCamera : MonoBehaviour
    {
        [SerializeField] private IsometricCameraProfile _profile;

        private CinemachineCamera _vcam;
        private CameraService _cameraService;
        private PlayerService _playerService;
        private Transform _followTarget;
        private Mouse _mouse;

        private float _currentDistance;
        private Vector3 _currentPosition;

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

            ConnectToPlayer();
            SetupInitialPosition();

            _mouse = Mouse.current;
        }

        private void SetupCinemachine()
        {
            if (_vcam == null || _profile == null) return;

            var lens = _vcam.Lens;
            lens.FieldOfView = _profile.FieldOfView;
            lens.OrthographicSize = _profile.Distance * 0.5f; // For orthographic projection
            _vcam.Lens = lens;
            _vcam.Priority = _profile.Priority;

            _currentDistance = _profile.Distance;
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

            UpdateZoom();
            UpdatePosition();
            UpdateRotation();
        }

        private void UpdateZoom()
        {
            if (!_profile.AllowZoom || _mouse == null) return;

            // Handle zoom input (mouse scroll) using new Input System
            var scrollDelta = _mouse.scroll.ReadValue().y;
            if (Mathf.Abs(scrollDelta) > 0.01f)
            {
                var normalizedScroll = scrollDelta / 120f; // Normalize scroll value
                _currentDistance -= normalizedScroll * _profile.ZoomSpeed;
                _currentDistance = Mathf.Clamp(_currentDistance, _profile.MinZoomDistance, _profile.MaxZoomDistance);
            }
        }

        private void UpdatePosition()
        {
            var targetPosition = CalculateTargetPosition();

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
        }

        private void UpdateRotation()
        {
            _vcam.transform.rotation = CalculateRotation();
        }

        private Vector3 CalculateTargetPosition()
        {
            var targetPos = _followTarget.position;

            // Calculate isometric offset
            var angle = _profile.IsometricAngle * Mathf.Deg2Rad;
            var yRotation = _profile.YRotation * Mathf.Deg2Rad;

            var horizontalDistance = _currentDistance * Mathf.Cos(angle);
            var verticalDistance = _currentDistance * Mathf.Sin(angle);

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
            return Quaternion.Euler(_profile.IsometricAngle, _profile.YRotation, 0f);
        }

        private void OnDestroy()
        {
            EventBus.Instance?.Unsubscribe<PlayerRegisteredEvent>(this);
        }
    }
}
