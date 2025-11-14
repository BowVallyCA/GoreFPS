using UnityEngine;

namespace _Project.Code.Gameplay.CameraSystems.Effects
{
    public class CameraEffectContext
    {
        public Transform CameraTransform { get; set; }
        public float DeltaTime { get; set; }
        public float ElapsedTime { get; set; }
    }
}
