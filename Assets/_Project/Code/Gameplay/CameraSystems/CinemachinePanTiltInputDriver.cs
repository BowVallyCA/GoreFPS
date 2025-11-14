using UnityEngine;
using Unity.Cinemachine;
using _Project.Code.Core.Events;
using _Project.Code.Core.ServiceLocator;
using _Project.Code.Gameplay.Input;

namespace _Project.Code.Gameplay.CameraSystems
{
    [RequireComponent(typeof(CinemachineCamera))]
    public class CinemachinePanTiltInputDriver : MonoBehaviour
    {
        private CinemachinePanTilt _panTilt;
        private Vector2 _lookInput;

        private void Awake()
        {
            var vcam = GetComponent<CinemachineCamera>();
            _panTilt = vcam.GetCinemachineComponent(CinemachineCore.Stage.Aim) as CinemachinePanTilt;

            if (_panTilt == null)
            {
                Debug.LogError("CinemachinePanTiltInputDriver requires CinemachinePanTilt component in Aim stage!");
                enabled = false;
            }
        }

        private void OnEnable()
        {
            EventBus.Instance.Subscribe<LookInputEvent>(this, HandleLookInput);
        }

        private void OnDisable()
        {
            EventBus.Instance?.Unsubscribe<LookInputEvent>(this);
        }

        private void HandleLookInput(LookInputEvent evt)
        {
            _lookInput = evt.Input;
        }

        private void Update()
        {
            if (_panTilt == null) return;

            if (_lookInput.magnitude < ServiceLocator.Get<InputService>().Profile.LookMagnitudeThreshold)
            {
                _lookInput = Vector2.zero;
            }

            _panTilt.PanAxis.Value += _lookInput.x;
            _panTilt.TiltAxis.Value += _lookInput.y;
        }
    }
}
