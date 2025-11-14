using UnityEngine;

namespace _Project.Code.Gameplay.CameraSystems.Effects
{
    [CreateAssetMenu(fileName = "ShakeEffect", menuName = "ScriptableObjects/Camera/Effects/Shake")]
    public class CameraShakeEffect : CameraEffectStrategy
    {
        [field: SerializeField, Header("Shake Settings")]
        [field: Tooltip("Shake intensity")]
        [field: Range(0f, 2f)]
        public float Intensity { get; private set; } = 0.2f;

        [field: SerializeField]
        [field: Tooltip("Shake duration")]
        [field: Range(0f, 5f)]
        public float Duration { get; private set; } = 0.3f;

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
        private Vector3 _shakeOffset;

        public override bool IsActive => _currentShakeIntensity > 0.01f;

        public override void Initialize()
        {
            _currentShakeIntensity = 0f;
            _shakeTimer = 0f;
            _shakeOffset = Vector3.zero;
        }

        public override void Reset()
        {
            Initialize();
        }

        public void TriggerShake()
        {
            _currentShakeIntensity = Intensity;
            _shakeTimer = Duration;
        }

        public void TriggerShake(float customIntensity, float customDuration)
        {
            _currentShakeIntensity = customIntensity;
            _shakeTimer = customDuration;
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

            _shakeOffset = new Vector3(x, y, z) * _currentShakeIntensity;

            context.CameraTransform.localPosition += _shakeOffset;
        }
    }
}
