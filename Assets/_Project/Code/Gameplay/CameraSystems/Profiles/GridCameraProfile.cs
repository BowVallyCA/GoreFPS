using UnityEngine;

namespace _Project.Code.Gameplay.CameraSystems.Profiles
{
    [CreateAssetMenu(fileName = "GridCameraProfile", menuName = "ScriptableObjects/Camera/Grid Camera Profile")]
    public class GridCameraProfile : CameraProfile
    {
        [field: SerializeField, Header("Grid Settings"), Tooltip("Fixed camera angle (degrees from horizontal)")]
        public float CameraAngle { get; private set; } = 45f;

        [field: SerializeField, Tooltip("Rotation around Y axis (degrees)")]
        public float YRotation { get; private set; } = 45f;

        [field: SerializeField, Tooltip("Distance from target")]
        public float Distance { get; private set; } = 12f;

        [field: SerializeField, Tooltip("Height offset above target")]
        public float HeightOffset { get; private set; } = 0f;

        [field: SerializeField, Tooltip("Follow smoothing speed"), Range(1f, 20f)]
        public float FollowSmoothing { get; private set; } = 8f;

        [field: SerializeField, Header("Snapping"), Tooltip("Snap camera to grid cells?")]
        public bool SnapToGrid { get; private set; } = false;

        [field: SerializeField, Tooltip("Grid size for camera snapping")]
        public float GridSize { get; private set; } = 1f;

        [field: SerializeField, Header("Bounds"), Tooltip("Use camera bounds?")]
        public bool UseBounds { get; private set; } = false;

        [field: SerializeField, Tooltip("Minimum position bounds")]
        public Vector3 MinBounds { get; private set; } = new Vector3(-50f, 0f, -50f);

        [field: SerializeField, Tooltip("Maximum position bounds")]
        public Vector3 MaxBounds { get; private set; } = new Vector3(50f, 100f, 50f);

        [field: SerializeField, Header("Rotation"), Tooltip("Allow camera rotation?")]
        public bool AllowRotation { get; private set; } = false;

        [field: SerializeField, Tooltip("Rotation speed (degrees per second)")]
        public float RotationSpeed { get; private set; } = 90f;

        [field: SerializeField, Tooltip("Snap rotation to 90-degree angles?")]
        public bool SnapRotation { get; private set; } = true;
    }
}
