using UnityEngine;
using System.Collections.Generic;
using _Project.Code.Gameplay.CameraSystems.Effects;

namespace _Project.Code.Gameplay.CameraSystems
{
    public abstract class BaseCamera : MonoBehaviour
    {
        [SerializeField] private List<CameraEffectStrategy> _effectStrategies = new List<CameraEffectStrategy>();

        private CameraEffectContext _effectContext;
        private float _elapsedTime;

        protected virtual void Awake()
        {
            _effectContext = new CameraEffectContext();
            InitializeEffectStrategies();
        }

        private void InitializeEffectStrategies()
        {
            foreach (var strategy in _effectStrategies)
            {
                strategy.Initialize();
            }
        }

        protected virtual void OnDisable()
        {
            DeactivateEffects();
        }

        protected virtual void LateUpdate()
        {
            ExecuteEffectStrategies();
        }

        private void ExecuteEffectStrategies()
        {
            _elapsedTime += Time.deltaTime;

            _effectContext.CameraTransform = transform;
            _effectContext.DeltaTime = Time.deltaTime;
            _effectContext.ElapsedTime = _elapsedTime;

            foreach (var strategy in _effectStrategies)
            {
                if (strategy != null)
                {
                    strategy.Execute(_effectContext);
                }
            }
        }

        private void DeactivateEffects()
        {
            foreach (var strategy in _effectStrategies)
            {
                strategy?.OnCameraDeactivated(this);
            }
        }
    }
}
