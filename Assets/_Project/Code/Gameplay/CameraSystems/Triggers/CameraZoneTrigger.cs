using UnityEngine;
using Unity.Cinemachine;
using _Project.Code.Core.ServiceLocator;
using _Project.Code.Gameplay.Triggers;

namespace _Project.Code.Gameplay.CameraSystems.Triggers
{
    public class CameraZoneTrigger : TriggerZone
    {
        [Header("Camera Settings")]
        [SerializeField] private BaseCamera _cameraToActivate;

        [Header("Exit Behavior")]
        [SerializeField] private bool _restorePreviousCamera = true;

        private CameraService _cameraService;
        private CinemachineCamera _previousCamera;

        private void Start()
        {
            _cameraService = ServiceLocator.Get<CameraService>();
        }

        protected override void OnZoneEntered(GameObject obj)
        {
            if (_cameraToActivate == null || _cameraService == null) return;

            var vcam = _cameraToActivate.GetComponent<CinemachineCamera>();
            if (vcam == null) return;

            // Store the current camera so we can restore it on exit
            _previousCamera = _cameraService.ActiveVirtualCamera;

            // Switch to the zone camera
            _cameraService.SetActiveCamera(vcam);
        }

        protected override void OnZoneExited(GameObject obj)
        {
            if (_cameraService == null) return;

            // Restore previous camera if configured
            if (_restorePreviousCamera && _previousCamera != null)
            {
                _cameraService.SetActiveCamera(_previousCamera);
            }
        }

        protected override Color GetGizmoColor()
        {
            return new Color(0, 1, 1, 0.3f);
        }
    }
}
