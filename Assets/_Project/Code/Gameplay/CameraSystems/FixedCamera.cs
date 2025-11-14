using UnityEngine;
using Unity.Cinemachine;
using _Project.Code.Core.ServiceLocator;
using _Project.Code.Core.Events;
using _Project.Code.Gameplay.CameraSystems.Profiles;
using _Project.Code.Gameplay.Player;

namespace _Project.Code.Gameplay.CameraSystems
{
    [RequireComponent(typeof(CinemachineCamera))]
    public class FixedCamera : BaseCamera
    {
        [SerializeField] private FixedCameraProfile _profile;

        private CinemachineCamera _vcam;
        private CameraService _cameraService;
        private PlayerService _playerService;
        private Transform _player;
        private Vector3 _fixedPosition;
        private Quaternion _fixedRotation;

        protected override void Awake()
        {
            base.Awake();
            _vcam = GetComponent<CinemachineCamera>();

            // Store the initial position and rotation from the scene
            _fixedPosition = transform.position;
            _fixedRotation = transform.rotation;
        }

        private void Start()
        {
            _cameraService = ServiceLocator.Get<CameraService>();
            _playerService = ServiceLocator.Get<PlayerService>();

            SetupCinemachine();
            RegisterWithService();

            EventBus.Instance.Subscribe<PlayerRegisteredEvent>(this, OnPlayerRegistered);

            // Auto-connect to player if already registered
            ConnectToPlayer();
        }

        private void SetupCinemachine()
        {
            if (_vcam == null || _profile == null) return;

            var lens = _vcam.Lens;
            lens.FieldOfView = _profile.FieldOfView;
            _vcam.Lens = lens;
            _vcam.Priority = _profile.Priority;

            // Ensure camera stays at fixed position
            transform.position = _fixedPosition;
            transform.rotation = _fixedRotation;
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
                _player = evt.Player;
            }
        }

        private void ConnectToPlayer()
        {
            var player = _playerService.GetPlayerTransform();
            if (player != null)
            {
                _player = player;
            }
        }

        protected override void LateUpdate()
        {
            base.LateUpdate();

            if (_profile == null || _vcam == null) return;

            // Always maintain fixed position
            transform.position = _fixedPosition;

            // Optionally track player with rotation
            if (_profile.TrackPlayer && _player != null)
            {
                Vector3 targetPosition = _player.position + Vector3.up * _profile.TrackingHeightOffset;
                Vector3 directionToTarget = (targetPosition - transform.position).normalized;
                Quaternion targetRotation = Quaternion.LookRotation(directionToTarget);

                transform.rotation = Quaternion.Slerp(
                    transform.rotation,
                    targetRotation,
                    _profile.TrackingSpeed * Time.deltaTime
                );
            }
            else
            {
                // Keep fixed rotation if not tracking
                transform.rotation = _fixedRotation;
            }
        }

        protected override void OnDisable()
        {
            base.OnDisable();
        }

        private void OnDestroy()
        {
            EventBus.Instance?.Unsubscribe<PlayerRegisteredEvent>(this);
        }
    }
}
