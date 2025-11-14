using UnityEngine;
using _Project.Code.Core.Events;
using _Project.Code.Gameplay.Player;
using _Project.Code.Gameplay.CameraSystems.Effects;

namespace _Project.Code.Gameplay.CameraSystems.Mediators
{
    public class ShakeOnLandingMediator : MonoBehaviour
    {
        [SerializeField, Header("Shake Settings")]
        [Tooltip("Base shake intensity")]
        [Range(0f, 2f)]
        private float _baseIntensity = 0.3f;

        [SerializeField]
        [Tooltip("Shake duration")]
        [Range(0f, 1f)]
        private float _duration = 0.2f;

        [SerializeField]
        [Tooltip("Minimum fall speed to trigger shake")]
        private float _minFallSpeed = 5f;

        [SerializeField]
        [Tooltip("Scale intensity based on fall speed")]
        private bool _scaleWithFallSpeed = true;

        private void OnEnable()
        {
            EventBus.Instance.Subscribe<CharacterLandedEvent>(this, OnCharacterLanded);
        }

        private void OnDisable()
        {
            EventBus.Instance?.Unsubscribe<CharacterLandedEvent>(this);
        }

        private void OnCharacterLanded(CharacterLandedEvent evt)
        {
            if (evt.FallSpeed < _minFallSpeed) return;

            float intensity = _baseIntensity;
            if (_scaleWithFallSpeed)
            {
                intensity = Mathf.Clamp(_baseIntensity * (evt.FallSpeed / 20f), 0.1f, 2f);
            }

            EventBus.Instance.Publish(new CameraShakeRequestedEvent
            {
                Intensity = intensity,
                Duration = _duration
            });
        }
    }
}
