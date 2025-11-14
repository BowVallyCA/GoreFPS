using UnityEngine;

namespace _Project.Code.Gameplay.CameraSystems.Profiles
{
    [CreateAssetMenu(fileName = "AimCameraProfile", menuName = "ScriptableObjects/Camera/Aim Camera Profile")]
    public class AimCameraProfile : CameraProfile
    {
        [field: SerializeField, Header("Position")]
        [field: Tooltip("Position offset from the target")]
        public Vector3 Offset { get; private set; } = new Vector3(0f, 1.8f, -4f);

        [field: SerializeField]
        [field: Tooltip("Name of the child transform to attach to (e.g., 'HeadTarget', 'CameraMount')")]
        public string AttachmentPoint { get; private set; } = "HeadTarget";

        [field: SerializeField, Range(0f, 0.99f)]
        [field: Tooltip("Damping for rotation following (lower = more responsive)")]
        public float RotationDamping { get; private set; } = 0.1f;

        [field: SerializeField, Header("Look Settings"), Range(0f, 1f)]
        [field: Tooltip("Horizontal look sensitivity")]
        public float LookSensitivityX { get; private set; } = 0.3f;

        [field: SerializeField, Range(0f, 1f)]
        [field: Tooltip("Vertical look sensitivity")]
        public float LookSensitivityY { get; private set; } = 0.3f;

        [field: SerializeField, Range(-90f, 0f)]
        [field: Tooltip("Vertical look limits")]
        public float MinPitch { get; private set; } = -60f;

        [field: SerializeField, Range(0f, 90f)]
        public float MaxPitch { get; private set; } = 60f;

        [field: SerializeField, Header("Collision")]
        [field: Tooltip("Enable camera collision detection")]
        public bool EnableCollision { get; private set; } = true;

        [field: SerializeField, Range(0.1f, 1f)]
        [field: Tooltip("Camera collision sphere radius")]
        public float CollisionRadius { get; private set; } = 0.2f;

        [field: SerializeField, Range(0.5f, 3f)]
        [field: Tooltip("Minimum distance from player when occluded")]
        public float MinDistance { get; private set; } = 1f;

        [field: SerializeField]
        [field: Tooltip("Layers that camera will collide with")]
        public LayerMask CollisionLayers { get; private set; } = -1;

        [field: SerializeField, Header("Field of View")]
        [field: Tooltip("Adjust FOV based on movement state")]
        public bool DynamicFOV { get; private set; } = false;

        [field: SerializeField, Range(60f, 90f)]
        [field: Tooltip("FOV when sprinting")]
        public float SprintFOV { get; private set; } = 70f;

        [field: SerializeField, Range(30f, 60f)]
        [field: Tooltip("FOV when aiming")]
        public float AimFOV { get; private set; } = 40f;

        [field: SerializeField, Range(1f, 10f)]
        [field: Tooltip("FOV transition speed")]
        public float FOVTransitionSpeed { get; private set; } = 5f;

    }
}