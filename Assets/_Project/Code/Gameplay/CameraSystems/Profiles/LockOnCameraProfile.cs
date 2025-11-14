using UnityEngine;

namespace _Project.Code.Gameplay.CameraSystems.Profiles
{
    [CreateAssetMenu(fileName = "LockOnCameraProfile", menuName = "ScriptableObjects/Camera/Lock On Camera Profile")]
    public class LockOnCameraProfile : CameraProfile
    {
        [field: SerializeField, Header("Free Camera Settings"), Tooltip("Camera offset when not locked")]
        public Vector3 FreeOffset { get; private set; } = new Vector3(0f, 5f, -7f);

        [field: SerializeField, Header("Locked Camera Settings"), Tooltip("Camera offset when locked on target")]
        public Vector3 LockedOffset { get; private set; } = new Vector3(0f, 8f, -10f);

        [field: SerializeField, Tooltip("Height offset for look target")]
        public float LookHeightOffset { get; private set; } = 1.5f;

        [field: SerializeField, Tooltip("Maximum distance to consider for FOV adjustment"), Range(5f, 30f)]
        public float MaxTargetDistance { get; private set; } = 15f;

        [field: SerializeField, Tooltip("Maximum FOV multiplier when target is far"), Range(1f, 1.5f)]
        public float MaxFOVMultiplier { get; private set; } = 1.2f;

        [field: SerializeField, Header("Smoothing"), Tooltip("Position follow smoothing"), Range(1f, 20f)]
        public float FollowSmoothing { get; private set; } = 8f;

        [field: SerializeField, Tooltip("Rotation smoothing"), Range(1f, 20f)]
        public float RotationSmoothing { get; private set; } = 10f;
    }
}