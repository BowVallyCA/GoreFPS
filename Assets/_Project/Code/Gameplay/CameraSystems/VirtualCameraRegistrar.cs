using UnityEngine;
using Unity.Cinemachine;
using _Project.Code.Core.ServiceLocator;

namespace _Project.Code.Gameplay.CameraSystems
{
    [RequireComponent(typeof(CinemachineCamera))]
    public class VirtualCameraRegistrar : MonoBehaviour
    {
        private CinemachineCamera _vcam;

        private void Awake()
        {
            _vcam = GetComponent<CinemachineCamera>();
        }

        private void Start()
        {
            var cameraService = ServiceLocator.Get<CameraService>();
            if (cameraService != null && _vcam != null)
            {
                cameraService.RegisterCamera(_vcam);
            }
        }
    }
}
