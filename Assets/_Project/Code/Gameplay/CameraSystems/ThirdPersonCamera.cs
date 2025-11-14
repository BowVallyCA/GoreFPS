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
    public class ThirdPersonCamera : BaseCamera
    {
        [SerializeField] private ThirdPersonCameraProfile _profile;

        private CinemachineCamera _vcam;
        private CinemachineThirdPersonFollow _thirdPersonFollow;
        private CinemachineRotationComposer _rotationComposer;
        private CameraService _cameraService;
        private PlayerService _playerService;
        private Transform _player;
        private Transform _cameraTarget;

        private float _currentYaw;
        private float _currentPitch;
        private float _currentFOV;
        private Vector2 _lookInput;
        private Transform _lockOnTarget;

        protected override void Awake()
        {
            base.Awake();
            _vcam = GetComponent<CinemachineCamera>();
        }

        private void Start()
        {
            StartCoroutine(LateStart());
        }

        private System.Collections.IEnumerator LateStart()
        {
            yield return null; // Wait one frame

            _cameraService = ServiceLocator.Get<CameraService>();
            _playerService = ServiceLocator.Get<PlayerService>();

            RegisterWithService();

            if (_profile != null)
            {
                _currentFOV = _profile.FieldOfView;
            }

            // Subscribe to events
            EventBus.Instance.Subscribe<PlayerRegisteredEvent>(this, OnPlayerRegistered);
            EventBus.Instance.Subscribe<LookInputEvent>(this, HandleLook);

            // Auto-connect to player if already registered
            ConnectToPlayer();
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
            if (_vcam == null || player == null) return;

            _player = player;

            var targetGO = new GameObject("CameraTarget");
            _cameraTarget = targetGO.transform;
            _cameraTarget.position = player.position;
            _cameraTarget.rotation = Quaternion.identity;

            _vcam.Target.TrackingTarget = _cameraTarget;

            var lookTarget = player.Find("LookTarget") ?? player;
            _vcam.Target.LookAtTarget = lookTarget;

            _currentYaw = player.eulerAngles.y;
            _currentPitch = 0f;

            SetupCinemachine();
        }

        private void SetupCinemachine()
        {
            if (_vcam == null || _profile == null) return;

            _thirdPersonFollow = _vcam.GetComponent<CinemachineThirdPersonFollow>();
            if (_thirdPersonFollow == null)
            {
                _thirdPersonFollow = _vcam.gameObject.AddComponent<CinemachineThirdPersonFollow>();
            }

            _rotationComposer = _vcam.GetComponent<CinemachineRotationComposer>();
            if (_rotationComposer == null)
            {
                _rotationComposer = _vcam.gameObject.AddComponent<CinemachineRotationComposer>();
            }

            _thirdPersonFollow.CameraDistance = _profile.FollowDistance;
            _thirdPersonFollow.ShoulderOffset = _profile.ShoulderOffset;
            _thirdPersonFollow.VerticalArmLength = _profile.VerticalArmLength;
            _thirdPersonFollow.CameraSide = 1f;
            _thirdPersonFollow.Damping = Vector3.zero;

            if (_profile.EnableCollision)
            {
                _thirdPersonFollow.AvoidObstacles.Enabled = true;
                _thirdPersonFollow.AvoidObstacles.CameraRadius = _profile.CollisionRadius;
            }

            var lens = _vcam.Lens;
            lens.FieldOfView = _profile.FieldOfView;
            _vcam.Lens = lens;

            _vcam.Priority = _profile.Priority;
        }

        private void HandleLook(LookInputEvent evt)
        {
            _lookInput = evt.Input;
        }

        private void Update()
        {

            ApplyLookInput();
            UpdateFieldOfView();
        }

        private void ApplyLookInput()
        {
            if (!_cameraTarget) return;
            var sensitivityMultiplier = 10f;
            _currentYaw += _lookInput.x * _profile.LookSensitivityX * sensitivityMultiplier * Time.deltaTime;
            _currentPitch -= _lookInput.y * _profile.LookSensitivityY * sensitivityMultiplier * Time.deltaTime;
            _currentPitch = Mathf.Clamp(_currentPitch, _profile.MinPitch, _profile.MaxPitch);

            _cameraTarget.position = _player.position;
            _cameraTarget.rotation = Quaternion.Euler(_currentPitch, _currentYaw, 0f);
        }

        protected override void LateUpdate()
        {
            base.LateUpdate();
        }

        private void UpdateFieldOfView()
        {
            if (!_profile.ModifyFOVOnSprint) return;

            // For now, maintain base FOV (could add sprint detection later)
            var targetFOV = _profile.FieldOfView;
            _currentFOV = Mathf.Lerp(_currentFOV, targetFOV, _profile.FOVTransitionSpeed * Time.deltaTime);

            var lens = _vcam.Lens;
            lens.FieldOfView = _currentFOV;
            _vcam.Lens = lens;
        }

        public void SetLockOnTarget(Transform target)
        {
            _lockOnTarget = target;
        }

        public void ClearLockOnTarget()
        {
            _lockOnTarget = null;
        }

        public Vector3 GetForwardDirection()
        {
            Vector3 forward = Quaternion.Euler(0f, _currentYaw, 0f) * Vector3.forward;
            return forward;
        }

        protected override void OnDisable()
        {
            base.OnDisable();
        }

        private void OnDestroy()
        {
            EventBus.Instance?.Unsubscribe<PlayerRegisteredEvent>(this);
            EventBus.Instance?.Unsubscribe<LookInputEvent>(this);

            if (_cameraTarget != null)
            {
                Destroy(_cameraTarget.gameObject);
            }
        }
    }
}