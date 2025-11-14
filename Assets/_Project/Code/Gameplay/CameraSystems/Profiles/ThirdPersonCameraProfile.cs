using UnityEngine;

namespace _Project.Code.Gameplay.CameraSystems.Profiles
{
    [CreateAssetMenu(fileName = "ThirdPersonCameraProfile", menuName = "ScriptableObjects/Camera/Third Person Camera Profile")]
    public class ThirdPersonCameraProfile : CameraProfile
    {
        [field: SerializeField, Header("Camera Distance")]
        [field: Range(2f, 20f)]
        public float FollowDistance { get; private set; } = 5f;

        [field: SerializeField, Header("Camera Offset")]
        public Vector3 ShoulderOffset { get; private set; } = new Vector3(0.0f, 0f, 0f);

        [field: SerializeField, Range(0f, 5f)]
        public float VerticalArmLength { get; private set; } = 1.5f;

        [field: SerializeField, Tooltip("Camera side (1 = right, -1 = left)")]
        [field: Range(-1f, 1f)]
        public float CameraSide { get; private set; } = 1f;

        [field: SerializeField, Header("Look Sensitivity")]
        [field: Range(0.1f, 1f)]
        public float LookSensitivityX { get; private set; } = 1f;

        [field: SerializeField, Range(0.1f, 1f)]
        public float LookSensitivityY { get; private set; } = 1f;

        [field: SerializeField, Header("Pitch Constraints"), Range(-89f, 0f)]
        public float MinPitch { get; private set; } = -30f;

        [field: SerializeField, Range(0f, 89f)]
        public float MaxPitch { get; private set; } = 60f;

        [field: SerializeField, Header("Collision")]
        public bool EnableCollision { get; private set; } = true;

        [field: SerializeField, Tooltip("Camera collision sphere radius")]
        public float CollisionRadius { get; private set; } = 0.2f;

        [field: SerializeField, Tooltip("Minimum distance from target when occluded")]
        public float MinDistance { get; private set; } = 1f;

        [field: SerializeField, Tooltip("How much to dampen the camera when occluded")]
        [field: Range(0f, 1f)]
        public float OcclusionDamping { get; private set; } = 0.5f;

        [field: SerializeField]
        public LayerMask CollisionLayers { get; private set; } = -1;

        [field: SerializeField, Header("Sprint Settings")]
        public bool ModifyFOVOnSprint { get; private set; } = true;

        [field: SerializeField, Range(60f, 90f)]
        public float SprintFieldOfView { get; private set; } = 70f;

        [field: SerializeField, Range(1f, 10f)]
        public float FOVTransitionSpeed { get; private set; } = 5f;
    }
}