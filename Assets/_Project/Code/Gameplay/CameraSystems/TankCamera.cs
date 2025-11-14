using _Project.Code.Gameplay.CameraSystems.Profiles;
using UnityEngine;
using Unity.Cinemachine;
using _Project.Code.Core.Events;
using _Project.Code.Core.ServiceLocator;
using _Project.Code.Gameplay.Player;

namespace _Project.Code.Gameplay.CameraSystems
{
    [RequireComponent(typeof(CinemachineCamera))]
    public class TankCamera : MonoBehaviour
    {
        [SerializeField] private TankCameraProfile _profile;

        private CinemachineCamera _vcam;
        private CameraService _cameraService;
        private PlayerService _playerService;
        private Transform _followTarget;

        private Vector3 _currentPosition;
        private Quaternion _currentRotation;

        private void Awake()
        {
            _vcam = GetComponent<CinemachineCamera>();
        }

        private void Start()
        {
            _cameraService = ServiceLocator.Get<CameraService>();
            _playerService = ServiceLocator.Get<PlayerService>();

            RegisterWithService();

            // Subscribe to PlayerRegisteredEvent for auto-connection
            EventBus.Instance.Subscribe<PlayerRegisteredEvent>(this, OnPlayerRegistered);

            // Auto-connect to player if already registered
            ConnectToPlayer();

            if (_vcam != null && _profile != null)
            {
                // Cinemachine 3.x requires copying and modifying LensSettings
                var lens = _vcam.Lens;
                lens.FieldOfView = _profile.FieldOfView;
                _vcam.Lens = lens;

                _vcam.Priority = _profile.Priority;
            }
        }

        private void RegisterWithService()
        {
            if (_cameraService != null && _vcam != null)
            {
                _cameraService.RegisterCamera(_vcam, setAsActive: true);
            }
        }

        private void OnPlayerRegistered(PlayerRegisteredEvent evt)
        {
            if (evt.Player != null)
            {
                SetupFollowTarget(evt.Player);
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

        private void SetupFollowTarget(Transform player)
        {
            _followTarget = player;

            if (_followTarget != null && _profile != null)
            {
                _currentPosition = _followTarget.position + _profile.FollowOffset;

                if (_profile.UseFixedAngle)
                {
                    _currentRotation = Quaternion.Euler(_profile.FixedRotation);
                }
            }
        }

        private void LateUpdate()
        {
            if (_profile == null || _followTarget == null || _vcam == null) return;

            // Calculate target position
            var targetPosition = _followTarget.position + _profile.FollowOffset;

            // Smooth follow
            _currentPosition = Vector3.Lerp(_currentPosition, targetPosition, _profile.FollowSmoothing * Time.deltaTime);
            _vcam.transform.position = _currentPosition;

            // Apply rotation
            if (_profile.UseFixedAngle)
            {
                _vcam.transform.rotation = Quaternion.Euler(_profile.FixedRotation);
            }
            else if (_profile.FollowRotation)
            {
                var targetRotation = Quaternion.LookRotation(_followTarget.position - _vcam.transform.position);
                _currentRotation = Quaternion.Lerp(_currentRotation, targetRotation, _profile.FollowSmoothing * Time.deltaTime);
                _vcam.transform.rotation = _currentRotation;
            }
        }

        private void OnDestroy()
        {
            EventBus.Instance?.Unsubscribe<PlayerRegisteredEvent>(this);
        }
    }
}