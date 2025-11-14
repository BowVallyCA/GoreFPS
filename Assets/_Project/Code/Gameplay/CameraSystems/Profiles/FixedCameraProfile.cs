using UnityEngine;

namespace _Project.Code.Gameplay.CameraSystems.Profiles
{
    [CreateAssetMenu(fileName = "FixedCameraProfile", menuName = "ScriptableObjects/Camera/Fixed Camera Profile")]
    public class FixedCameraProfile : ScriptableObject
    {
        [field: SerializeField, Header("Camera Settings")]
        [field: Tooltip("Camera priority (higher priority cameras take precedence)")]
        public int Priority { get; private set; } = 10;

        [field: SerializeField]
        [field: Tooltip("Field of view in degrees")]
        [field: Range(30f, 120f)]
        public float FieldOfView { get; private set; } = 60f;

        [field: SerializeField, Header("Tracking")]
        [field: Tooltip("Should the camera rotate to look at the player?")]
        public bool TrackPlayer { get; private set; } = true;

        [field: SerializeField]
        [field: Tooltip("Speed at which camera rotates to track player")]
        [field: Range(1f, 20f)]
        public float TrackingSpeed { get; private set; } = 5f;

        [field: SerializeField]
        [field: Tooltip("Vertical offset from player position when tracking (0 = look at feet)")]
        public float TrackingHeightOffset { get; private set; } = 1.5f;
    }
}
