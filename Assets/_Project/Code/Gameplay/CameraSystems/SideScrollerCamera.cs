using _Project.Code.Gameplay.CameraSystems.Profiles;
using _Project.Code.Core.ServiceLocator;
using _Project.Code.Core.Events;
using _Project.Code.Gameplay.Player;
using UnityEngine;
using Unity.Cinemachine;

namespace _Project.Code.Gameplay.CameraSystems
{
    [RequireComponent(typeof(CinemachineCamera))]
    public class SideScrollerCamera : MonoBehaviour
    {
        [SerializeField] private SideScrollerCameraProfile _profile;

        private CinemachineCamera _vcam;
        private CameraService _cameraService;
        private PlayerService _playerService;
        private Transform _followTarget;
        private Rigidbody _targetRigidbody;

        private Vector3 _currentPosition;
        private Vector3 _deadzoneCenter;
        private float _currentLookAheadOffset;
        private float _lastFacingDirection;

        private void Awake()
        {
            _vcam = GetComponent<CinemachineCamera>();

            if (_profile == null)
                throw new System.Exception("SideScrollerCamera requires a SideScrollerCameraProfile! Assign it in the Inspector.");
            if (_vcam == null)
                throw new System.Exception("SideScrollerCamera requires a CinemachineCamera component!");
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
        }

        private void SetupCinemachine()
        {
            var lens = _vcam.Lens;

            if (_profile.IsOrthographic)
            {
                lens.OrthographicSize = _profile.OrthographicSize;
            }
            else
            {
                lens.FieldOfView = _profile.FieldOfView;
            }

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
                SetupFollowTarget(player);
            }
        }

        private void OnPlayerRegistered(PlayerRegisteredEvent evt)
        {
            if (evt.Player != null)
            {
                SetupFollowTarget(evt.Player);
            }
        }

        private void SetupFollowTarget(Transform player)
        {
            _followTarget = player;
            _targetRigidbody = player.GetComponent<Rigidbody>();
            _lastFacingDirection = 1f;
        }

        private void SetupInitialPosition()
        {
            if (_followTarget == null) return;

            var targetPosition = _followTarget.position;

            _currentPosition = new Vector3(
                targetPosition.x,
                targetPosition.y,
                _profile.LockZPosition ? _profile.FixedZPosition : targetPosition.z + _profile.CameraDistance
            );

            _deadzoneCenter = _currentPosition;
            _currentLookAheadOffset = 0f;

            _vcam.transform.position = _currentPosition;
            _vcam.transform.rotation = Quaternion.Euler(_profile.FixedRotation);
        }

        private void LateUpdate()
        {
            if (_followTarget == null) return;

            var targetPosition = _followTarget.position;

            // Calculate look-ahead offset
            float lookAheadOffset = 0f;
            if (_profile.UseLookAhead && _targetRigidbody != null)
            {
                var velocity = _targetRigidbody.linearVelocity;
                var horizontalSpeed = Mathf.Abs(velocity.x);

                if (horizontalSpeed > _profile.LookAheadThreshold)
                {
                    var facingDirection = Mathf.Sign(velocity.x);
                    if (Mathf.Abs(facingDirection - _lastFacingDirection) > 0.1f)
                    {
                        _lastFacingDirection = facingDirection;
                    }
                    lookAheadOffset = _lastFacingDirection * _profile.LookAheadDistance;
                }
            }

            _currentLookAheadOffset = Mathf.Lerp(_currentLookAheadOffset, lookAheadOffset,
                _profile.LookAheadSmoothing * Time.deltaTime);

            // Calculate desired position with look-ahead
            var desiredX = targetPosition.x + _currentLookAheadOffset;
            var desiredY = targetPosition.y;
            var desiredZ = _profile.LockZPosition ? _profile.FixedZPosition : targetPosition.z + _profile.CameraDistance;

            // Apply deadzone (centered on player)
            if (_profile.UseDeadzone)
            {
                // Horizontal deadzone
                var deltaX = desiredX - _deadzoneCenter.x;
                var halfWidth = _profile.DeadzoneWidth * 0.5f;
                if (deltaX > halfWidth)
                {
                    _deadzoneCenter.x = desiredX - halfWidth;
                }
                else if (deltaX < -halfWidth)
                {
                    _deadzoneCenter.x = desiredX + halfWidth;
                }

                // Vertical deadzone
                var deltaY = desiredY - _deadzoneCenter.y;
                var halfHeight = _profile.DeadzoneHeight * 0.5f;
                if (deltaY > halfHeight)
                {
                    _deadzoneCenter.y = desiredY - halfHeight;
                }
                else if (deltaY < -halfHeight)
                {
                    _deadzoneCenter.y = desiredY + halfHeight;
                }

                desiredX = _deadzoneCenter.x;
                desiredY = _deadzoneCenter.y;
            }

            var targetCameraPosition = new Vector3(desiredX, desiredY, desiredZ);

            // Smooth follow
            _currentPosition = Vector3.Lerp(_currentPosition, targetCameraPosition,
                _profile.FollowSmoothing * Time.deltaTime);

            // Apply bounds
            if (_profile.UseBounds)
            {
                _currentPosition.x = Mathf.Clamp(_currentPosition.x, _profile.MinBounds.x, _profile.MaxBounds.x);
                _currentPosition.y = Mathf.Clamp(_currentPosition.y, _profile.MinBounds.y, _profile.MaxBounds.y);
            }

            _vcam.transform.position = _currentPosition;
            _vcam.transform.rotation = Quaternion.Euler(_profile.FixedRotation);
        }

        private void OnDestroy()
        {
            EventBus.Instance?.Unsubscribe<PlayerRegisteredEvent>(this);
        }
    }
}