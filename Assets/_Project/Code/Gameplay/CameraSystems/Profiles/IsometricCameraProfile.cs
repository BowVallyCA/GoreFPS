using UnityEngine;

namespace _Project.Code.Gameplay.CameraSystems.Profiles
{
    [CreateAssetMenu(fileName = "IsometricCameraProfile", menuName = "ScriptableObjects/Camera/Isometric Camera Profile")]
    public class IsometricCameraProfile : CameraProfile
    {
        [field: SerializeField, Header("Isometric Settings"), Tooltip("Fixed angle for isometric view (degrees)")]
        public float IsometricAngle { get; private set; } = 45f;

        [field: SerializeField, Tooltip("Rotation around Y axis (degrees)")]
        public float YRotation { get; private set; } = 45f;

        [field: SerializeField, Tooltip("Distance from target")]
        public float Distance { get; private set; } = 15f;

        [field: SerializeField, Tooltip("Height offset above target")]
        public float HeightOffset { get; private set; } = 10f;

        [field: SerializeField, Tooltip("Follow smoothing speed"), Range(1f, 20f)]
        public float FollowSmoothing { get; private set; } = 8f;

        [field: SerializeField, Header("Bounds"), Tooltip("Use camera bounds?")]
        public bool UseBounds { get; private set; } = false;

        [field: SerializeField, Tooltip("Minimum position bounds")]
        public Vector3 MinBounds { get; private set; } = new Vector3(-50f, 0f, -50f);

        [field: SerializeField, Tooltip("Maximum position bounds")]
        public Vector3 MaxBounds { get; private set; } = new Vector3(50f, 100f, 50f);

        [field: SerializeField, Header("Zoom"), Tooltip("Allow zoom?")]
        public bool AllowZoom { get; private set; } = true;

        [field: SerializeField, Tooltip("Minimum zoom distance")]
        public float MinZoomDistance { get; private set; } = 8f;

        [field: SerializeField, Tooltip("Maximum zoom distance")]
        public float MaxZoomDistance { get; private set; } = 25f;

        [field: SerializeField, Tooltip("Zoom speed")]
        public float ZoomSpeed { get; private set; } = 2f;
    }
}
