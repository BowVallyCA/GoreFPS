using UnityEngine;
using Unity.Cinemachine;
using _Project.Code.Core.ServiceLocator;
using _Project.Code.Core.Events;
using _Project.Code.Gameplay.Player;

namespace _Project.Code.Gameplay.CameraSystems
{
    public class CameraRegisteredEvent : IEvent
    {
        public CinemachineCamera Camera { get; set; }
    }

    public class CameraService : MonoBehaviourService
    {
        private CinemachineCamera _activeVirtualCamera;
        private Camera _mainCamera;
        private Transform _activeCameraTransform;

        public CinemachineCamera ActiveVirtualCamera => _activeVirtualCamera;
        public Camera MainCamera => _mainCamera;
        public Transform ActiveCameraTransform => _activeCameraTransform;

        public override void Initialize()
        {
            // Find the main camera with CinemachineBrain
            _mainCamera = Camera.main;
            if (_mainCamera == null)
            {
                var brain = FindFirstObjectByType<CinemachineBrain>();
                if (brain != null)
                {
                    _mainCamera = brain.GetComponent<Camera>();
                }
            }

            if (_mainCamera != null)
            {
                _activeCameraTransform = _mainCamera.transform;
            }

            // Listen for player registration
            EventBus.Instance.Subscribe<PlayerRegisteredEvent>(this, OnPlayerRegistered);

            Debug.Log("CameraService initialized successfully.");
        }

        private void OnPlayerRegistered(PlayerRegisteredEvent evt)
        {
            // When player registers, update active camera to track them
            if (_activeVirtualCamera != null && evt.Player != null)
            {
                _activeVirtualCamera.Target.TrackingTarget = evt.Player;
                Debug.Log($"Camera now tracking player: {evt.Player.name}");
            }
        }

        public void RegisterCamera(CinemachineCamera vcam, bool setAsActive = true)
        {
            if (vcam == null) return;

            if (setAsActive)
            {
                SetActiveCamera(vcam);
            }

            // Auto-connect to player if available
            var playerService = ServiceLocator.Get<PlayerService>();
            if (playerService?.GetPlayerTransform() != null)
            {
                vcam.Target.TrackingTarget = playerService.GetPlayerTransform();
                Debug.Log($"Camera auto-connected to player: {playerService.GetPlayerTransform().name}");
            }

            // Notify other systems
            EventBus.Instance.Publish(new CameraRegisteredEvent { Camera = vcam });
        }

        public void SetActiveCamera(CinemachineCamera vcam)
        {
            if (vcam == null) return;

            // Lower priority of previous active camera
            if (_activeVirtualCamera != null && _activeVirtualCamera != vcam)
            {
                _activeVirtualCamera.Priority = 0;
            }

            // Set new active camera with higher priority
            _activeVirtualCamera = vcam;
            _activeVirtualCamera.Priority = 10;

            _activeCameraTransform = _mainCamera != null ? _mainCamera.transform : vcam.transform;
        }

        public Transform GetCameraTransform()
        {
            return _activeCameraTransform;
        }

        public Vector3 GetCameraForward()
        {
            return _activeCameraTransform != null ? _activeCameraTransform.forward : Vector3.forward;
        }

        public Vector3 GetCameraRight()
        {
            return _activeCameraTransform != null ? _activeCameraTransform.right : Vector3.right;
        }

        public override void Dispose()
        {
            EventBus.Instance?.Unsubscribe<PlayerRegisteredEvent>(this);
            _activeVirtualCamera = null;
            _mainCamera = null;
            _activeCameraTransform = null;
        }

        private void OnDestroy()
        {
            Dispose();
        }
    }
}
