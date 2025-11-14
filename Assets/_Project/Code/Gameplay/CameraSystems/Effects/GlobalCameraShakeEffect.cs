using UnityEngine;
using _Project.Code.Core.Events;

namespace _Project.Code.Gameplay.CameraSystems.Effects
{
    public struct CameraShakeRequestedEvent : IEvent
    {
        public float Intensity;
        public float Duration;
    }

    [CreateAssetMenu(fileName = "GlobalCameraShake", menuName = "ScriptableObjects/Camera/Effects/Global Camera Shake")]
    public class GlobalCameraShakeEffect : CameraEffectStrategy
    {
        [field: SerializeField, Header("Default Shake Settings")]
        [field: Tooltip("Default shake intensity")]
        [field: Range(0f, 2f)]
        public float DefaultIntensity { get; private set; } = 0.2f;

        [field: SerializeField]
        [field: Tooltip("Default shake duration")]
        [field: Range(0f, 5f)]
        public float DefaultDuration { get; private set; } = 0.3f;

        [field: SerializeField]
        [field: Tooltip("Shake decay speed")]
        [field: Range(0f, 10f)]
        public float DecaySpeed { get; private set; } = 5f;

        [field: SerializeField]
        [field: Tooltip("Shake frequency (roughness)")]
        [field: Range(1f, 50f)]
        public float Frequency { get; private set; } = 25f;

        private float _currentShakeIntensity;
        private float _shakeTimer;
        private MonoBehaviour _camera;

        public override bool IsActive => _currentShakeIntensity > 0.01f;

        public override void Initialize()
        {
            _currentShakeIntensity = 0f;
            _shakeTimer = 0f;
        }

        public override void Reset()
        {
            Initialize();
        }

        public override void OnCameraActivated(MonoBehaviour camera)
        {
            _camera = camera;
            EventBus.Instance.Subscribe<CameraShakeRequestedEvent>(_camera, OnShakeRequested);
        }

        public override void OnCameraDeactivated(MonoBehaviour camera)
        {
            EventBus.Instance?.Unsubscribe<CameraShakeRequestedEvent>(_camera);
            _camera = null;
        }

        private void OnShakeRequested(CameraShakeRequestedEvent evt)
        {
            float intensity = evt.Intensity > 0f ? evt.Intensity : DefaultIntensity;
            float duration = evt.Duration > 0f ? evt.Duration : DefaultDuration;

            _currentShakeIntensity = intensity;
            _shakeTimer = duration;
        }

        public override void Execute(CameraEffectContext context)
        {
            if (_currentShakeIntensity <= 0.01f) return;

            _shakeTimer -= context.DeltaTime;
            if (_shakeTimer <= 0f)
            {
                _currentShakeIntensity = Mathf.Lerp(_currentShakeIntensity, 0f, DecaySpeed * context.DeltaTime);
            }

            float x = (Mathf.PerlinNoise(context.ElapsedTime * Frequency, 0f) - 0.5f) * 2f;
            float y = (Mathf.PerlinNoise(0f, context.ElapsedTime * Frequency) - 0.5f) * 2f;
            float z = (Mathf.PerlinNoise(context.ElapsedTime * Frequency, context.ElapsedTime * Frequency) - 0.5f) * 2f;

            Vector3 shakeOffset = new Vector3(x, y, z) * _currentShakeIntensity;

            context.CameraTransform.localPosition += shakeOffset;
        }
    }
}
