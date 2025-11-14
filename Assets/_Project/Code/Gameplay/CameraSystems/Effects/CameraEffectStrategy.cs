using UnityEngine;
using _Project.Code.Core.Strategy;

namespace _Project.Code.Gameplay.CameraSystems.Effects
{
    public abstract class CameraEffectStrategy : ScriptableStrategy<CameraEffectContext>
    {
        public abstract void Initialize();
        public abstract void Reset();
        public abstract bool IsActive { get; }

        public virtual void OnCameraActivated(MonoBehaviour camera) { }
        public virtual void OnCameraDeactivated(MonoBehaviour camera) { }
    }
}
