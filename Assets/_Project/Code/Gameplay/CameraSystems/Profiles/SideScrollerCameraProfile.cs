using UnityEngine;

namespace _Project.Code.Gameplay.CameraSystems.Profiles
{
    [CreateAssetMenu(fileName = "SideScrollerCameraProfile", menuName = "ScriptableObjects/Camera/Side Scroller Camera Profile")]
    public class SideScrollerCameraProfile : CameraProfile
    {
        [field: SerializeField, Header("Projection"), Tooltip("Use orthographic projection (typical for 2D platformers)")]
        public bool IsOrthographic { get; private set; } = true;

        [field: SerializeField, Tooltip("Orthographic camera size (height in units)"), Range(2f, 20f)]
        public float OrthographicSize { get; private set; } = 5f;

        [field: SerializeField, Header("Side Scroller Settings"), Tooltip("Fixed rotation for side view camera")]
        public Vector3 FixedRotation { get; private set; } = new Vector3(0f, 90f, 0f);

        [field: SerializeField, Tooltip("Distance from player on Z axis")]
        public float CameraDistance { get; private set; } = -10f;

        [field: SerializeField, Tooltip("Lock camera Z position")]
        public bool LockZPosition { get; private set; } = true;

        [field: SerializeField, Tooltip("Fixed Z position when locked")]
        public float FixedZPosition { get; private set; } = 0f;

        [field: SerializeField, Tooltip("Camera follow smoothing"), Range(1f, 20f)]
        public float FollowSmoothing { get; private set; } = 10f;

        [field: SerializeField, Header("Deadzone"), Tooltip("Use deadzone (prevents camera jitter on small movements)")]
        public bool UseDeadzone { get; private set; } = true;

        [field: SerializeField, Tooltip("Horizontal deadzone width (left/right)"), Range(0.1f, 5f)]
        public float DeadzoneWidth { get; private set; } = 1.5f;

        [field: SerializeField, Tooltip("Vertical deadzone height (up/down)"), Range(0.1f, 5f)]
        public float DeadzoneHeight { get; private set; } = 1f;

        [field: SerializeField, Header("Look-Ahead"), Tooltip("Camera shifts in direction player is facing")]
        public bool UseLookAhead { get; private set; } = true;

        [field: SerializeField, Tooltip("How far ahead to look in facing direction"), Range(0.5f, 5f)]
        public float LookAheadDistance { get; private set; } = 2f;

        [field: SerializeField, Tooltip("Speed of look-ahead transitions"), Range(1f, 10f)]
        public float LookAheadSmoothing { get; private set; } = 3f;

        [field: SerializeField, Tooltip("Minimum speed before look-ahead activates"), Range(0.1f, 2f)]
        public float LookAheadThreshold { get; private set; } = 0.5f;

        [field: SerializeField, Header("Bounds"), Tooltip("Constrain camera within bounds")]
        public bool UseBounds { get; private set; } = false;

        [field: SerializeField, Tooltip("Minimum camera bounds")]
        public Vector2 MinBounds { get; private set; } = new Vector2(-50f, -10f);

        [field: SerializeField, Tooltip("Maximum camera bounds")]
        public Vector2 MaxBounds { get; private set; } = new Vector2(50f, 20f);
    }
}