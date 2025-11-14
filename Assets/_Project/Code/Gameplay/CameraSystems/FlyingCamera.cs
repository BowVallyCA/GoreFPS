using _Project.Code.Gameplay.CameraSystems.Profiles;
using _Project.Code.Core.ServiceLocator;
using _Project.Code.Core.Events;
using _Project.Code.Gameplay.Player;
using UnityEngine;
using Unity.Cinemachine;

namespace _Project.Code.Gameplay.CameraSystems
{
    [RequireComponent(typeof(CinemachineCamera))]
    public class FlyingCamera : MonoBehaviour
    {
        [SerializeField] private FlyingCameraProfile _profile;

        private CinemachineCamera _vcam;
        private CameraService _cameraService;
        private PlayerService _playerService;
        private Transform _followTarget;

        private Vector3 _currentPosition;
        private Quaternion _currentRotation;
        private float _currentFOV;

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
        }

        private void SetupCinemachine()
        {
            if (_vcam == null || _profile == null) return;

            var lens = _vcam.Lens;
            lens.FieldOfView = _profile.FieldOfView;
            _vcam.Lens = lens;
            _vcam.Priority = _profile.Priority;

            _currentFOV = _profile.FieldOfView;
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

            _currentPosition = _followTarget.position + _followTarget.TransformDirection(_profile.FollowOffset);
            _currentRotation = _followTarget.rotation;

            if (_vcam != null)
            {
                _vcam.transform.position = _currentPosition;
                _vcam.transform.rotation = _currentRotation;
            }
        }

        private void LateUpdate()
        {
            if (_profile == null || _followTarget == null || _vcam == null) return;

            UpdatePosition();
            UpdateRotation();
            UpdateFOV();
        }

        private void UpdatePosition()
        {
            // Calculate position behind the aircraft
            var targetPosition = _followTarget.position + _followTarget.TransformDirection(_profile.FollowOffset);

            // Smooth follow
            _currentPosition = Vector3.Lerp(_currentPosition, targetPosition, _profile.FollowSmoothing * Time.deltaTime);
            _vcam.transform.position = _currentPosition;
        }

        private void UpdateRotation()
        {
            var targetRotation = _followTarget.rotation;

            // Optionally reduce banking effect
            if (_profile.FollowBanking)
            {
                var euler = targetRotation.eulerAngles;
                euler.z *= _profile.BankFollowIntensity;
                targetRotation = Quaternion.Euler(euler);
            }
            else
            {
                var euler = targetRotation.eulerAngles;
                euler.z = 0f;
                targetRotation = Quaternion.Euler(euler);
            }

            // Smooth rotation
            _currentRotation = Quaternion.Slerp(_currentRotation, targetRotation, _profile.RotationSmoothing * Time.deltaTime);
            _vcam.transform.rotation = _currentRotation;
        }

        private void UpdateFOV()
        {
            if (!_profile.DynamicFOV) return;

            // Calculate speed-based FOV
            var rb = _followTarget.GetComponent<Rigidbody>();
            if (rb != null)
            {
                var speed = rb.linearVelocity.magnitude;
                var fovIncrease = Mathf.Clamp(speed * _profile.FOVSpeedMultiplier, 0f, _profile.MaxFOVIncrease);
                _currentFOV = Mathf.Lerp(_currentFOV, _profile.FieldOfView + fovIncrease, Time.deltaTime * 5f);

                var lens = _vcam.Lens;
                lens.FieldOfView = _currentFOV;
                _vcam.Lens = lens;
            }
        }

        private void OnDestroy()
        {
            EventBus.Instance?.Unsubscribe<PlayerRegisteredEvent>(this);
        }
    }
}
