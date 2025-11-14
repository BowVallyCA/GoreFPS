using UnityEngine;
using Unity.Cinemachine;
using _Project.Code.Core.Events;
using _Project.Code.Core.ServiceLocator;
using _Project.Code.Gameplay.CameraSystems.Profiles;
using _Project.Code.Gameplay.Input;
using _Project.Code.Gameplay.Player;

namespace _Project.Code.Gameplay.CameraSystems
{
    [RequireComponent(typeof(CinemachineCamera))]
    public class TopDownCamera : MonoBehaviour
    {
        [SerializeField] private TopDownCameraProfile _profile;

        private CinemachineCamera _vcam;
        private CameraService _cameraService;
        private PlayerService _playerService;
        private Transform _followTarget;

        private Vector3 _currentPosition;
        private float _currentRotationY;

        private void Awake()
        {
            _vcam = GetComponent<CinemachineCamera>();

            if (_profile == null)
                throw new System.Exception("TopDownCamera requires a TopDownCameraProfile! Assign it in the Inspector.");
            if (_vcam == null)
                throw new System.Exception("TopDownCamera requires a CinemachineCamera component!");
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
                var lens = LensSettings.Default;

                if (_profile.IsOrthographic)
                {
                    // For orthographic cameras, set the orthographic size
                    // The actual orthographic mode is set on the Unity Camera component
                    lens.OrthographicSize = _profile.OrthographicSize;
                }
                else
                {
                    lens.FieldOfView = _profile.FieldOfView;
                }

                _vcam.Lens = lens;
                _vcam.Priority = _profile.Priority;
            }

            if (_profile.AllowRotation)
            {
                EventBus.Instance.Subscribe<LookInputEvent>(this, HandleCameraRotation);
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
            SetupInitialPosition();
        }

        private void SetupInitialPosition()
        {
            if (_followTarget == null)
                return;

            var targetPosition = _followTarget.position + _profile.PositionOffset;
            targetPosition.y = _profile.Height;
            _currentPosition = targetPosition;

            _vcam.transform.position = _currentPosition;
            _vcam.transform.rotation = Quaternion.Euler(_profile.LookDownAngle, 0f, 0f);
        }

        private void LateUpdate()
        {
            if (_followTarget == null) return;

            var targetPosition = _followTarget.position + _profile.PositionOffset;
            targetPosition.y = _profile.Height;

            _currentPosition = Vector3.Lerp(_currentPosition, targetPosition, _profile.FollowSmoothing * Time.deltaTime);
            _vcam.transform.position = _currentPosition;

            if (_profile.AllowRotation)
            {
                var rotation = Quaternion.Euler(_profile.LookDownAngle, _currentRotationY, 0f);
                _vcam.transform.rotation = rotation;
            }
            else
            {
                _vcam.transform.rotation = Quaternion.Euler(_profile.LookDownAngle, 0f, 0f);
            }
        }

        private void HandleCameraRotation(LookInputEvent evt)
        {
            if (_profile.AllowRotation)
            {
                _currentRotationY += evt.Input.x * _profile.RotationSpeed * Time.deltaTime;
            }
        }

        private void OnDestroy()
        {
            EventBus.Instance?.Unsubscribe<PlayerRegisteredEvent>(this);

            if (_profile.AllowRotation)
            {
                EventBus.Instance?.Unsubscribe<LookInputEvent>(this);
            }
        }
    }
}