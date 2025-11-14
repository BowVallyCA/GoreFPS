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
    public class AimCamera : BaseCamera
    {
        [SerializeField] private AimCameraProfile _profile;

        private CinemachineCamera _vcam;

        private CameraService _cameraService;
        private PlayerService _playerService;

        private Vector3 _currentOffset;
        private float _currentFOV;
        private float _currentPitch;
        private float _currentYaw;

        protected override void Awake()
        {
            base.Awake();
            _vcam = GetComponent<CinemachineCamera>();
        }

        private void Start()
        {
            _cameraService = ServiceLocator.Get<CameraService>();
            _playerService = ServiceLocator.Get<PlayerService>();

            SetupCinemachine();
            RegisterWithService();

            // Subscribe to events
            EventBus.Instance.Subscribe<PlayerRegisteredEvent>(this, OnPlayerRegistered);
            EventBus.Instance.Subscribe<LookInputEvent>(this, HandleLook);

            // Auto-connect to player if available
            ConnectToPlayer();

            // Initialize rotation values
            _currentPitch = 0f;
            if (_playerService.GetPlayerTransform() != null)
            {
                _currentYaw = _playerService.GetPlayerTransform().eulerAngles.y;
            }
        }

        private void SetupCinemachine()
        {
            if (_vcam == null || _profile == null) return;

            // We're directly parenting to the player, so we don't need CinemachineFollow
            // Just set up the basic camera settings

            // Set lens settings
            var lens = _vcam.Lens;
            lens.FieldOfView = _profile.FieldOfView;
            _vcam.Lens = lens;

            _vcam.Priority = _profile.Priority;
            _currentFOV = _profile.FieldOfView;
            _currentOffset = _profile.Offset;
        }

        private void RegisterWithService()
        {
            _cameraService.RegisterCamera(_vcam, setAsActive: true);
            Debug.Log($"AimCamera registered with CameraService");
        }

        private void ConnectToPlayer()
        {
            var player = _playerService.GetPlayerTransform();
            if (player != null)
            {
                // Find the attachment point on the player
                var attachPoint = player.Find(_profile.AttachmentPoint);
                if (attachPoint == null)
                {
                    // Fallback to player root if attachment point not found
                    attachPoint = player;
                    Debug.LogWarning($"Attachment point '{_profile.AttachmentPoint}' not found, using player root");
                }

                // Parent camera to attachment point
                transform.SetParent(attachPoint);
                transform.localPosition = _currentOffset;
                transform.localRotation = Quaternion.identity;

                Debug.Log($"AimCamera attached to: {attachPoint.name}");
            }
        }

        private void OnPlayerRegistered(PlayerRegisteredEvent evt)
        {
            if (evt.Player != null)
            {
                // Find the attachment point on the player
                var attachPoint = evt.Player.Find(_profile.AttachmentPoint);
                if (attachPoint == null)
                {
                    // Fallback to player root if attachment point not found
                    attachPoint = evt.Player;
                    Debug.LogWarning($"Attachment point '{_profile.AttachmentPoint}' not found, using player root");
                }

                // Parent camera to attachment point
                transform.SetParent(attachPoint);
                transform.localPosition = _currentOffset;
                transform.localRotation = Quaternion.identity;

                Debug.Log($"AimCamera attached to: {attachPoint.name}");
            }
        }

        private void HandleLook(LookInputEvent evt)
        {
            if (_profile == null) return;

            // Since we're parented to the player's head, we need to:
            // 1. Rotate the player for yaw (horizontal look)
            // 2. Apply pitch locally to the camera

            // Accumulate input
            _currentYaw += evt.Input.x * _profile.LookSensitivityX;
            _currentPitch -= evt.Input.y * _profile.LookSensitivityY;
            _currentPitch = Mathf.Clamp(_currentPitch, _profile.MinPitch, _profile.MaxPitch);
        }

        protected override void LateUpdate()
        {
            if (_profile == null) return;

            // Apply yaw to player with damping
            var player = _playerService.GetPlayerTransform();
            if (player != null)
            {
                var targetRotation = Quaternion.Euler(0, _currentYaw, 0);
                if (_profile.RotationDamping > 0.01f)
                {
                    player.rotation = Quaternion.Slerp(player.rotation, targetRotation,
                        (1f - _profile.RotationDamping) * 10f * Time.deltaTime);
                }
                else
                {
                    player.rotation = targetRotation;
                }
            }

            // Apply pitch to camera with damping
            var targetPitch = Quaternion.Euler(_currentPitch, 0, 0);
            if (_profile.RotationDamping > 0.01f)
            {
                transform.localRotation = Quaternion.Slerp(transform.localRotation, targetPitch,
                    (1f - _profile.RotationDamping) * 10f * Time.deltaTime);
            }
            else
            {
                transform.localRotation = targetPitch;
            }

            base.LateUpdate();
        }

        public void SetOffset(Vector3 offset)
        {
            _currentOffset = offset;
            transform.localPosition = _currentOffset;
        }

        private void Update()
        {
            if (_profile == null || _vcam == null) return;

           // UpdateFieldOfView();
        }

        private void UpdateFieldOfView()
        {
            if (!_profile.DynamicFOV) return;

            // TODO: Check player state for sprint/aim
            var targetFOV = _profile.FieldOfView;

            _currentFOV = Mathf.Lerp(_currentFOV, targetFOV, _profile.FOVTransitionSpeed * Time.deltaTime);

            var lens = _vcam.Lens;
            lens.FieldOfView = _currentFOV;
            _vcam.Lens = lens;
        }



        private void OnDestroy()
        {
            EventBus.Instance?.Unsubscribe<PlayerRegisteredEvent>(this);
            EventBus.Instance?.Unsubscribe<LookInputEvent>(this);
        }
    }
}