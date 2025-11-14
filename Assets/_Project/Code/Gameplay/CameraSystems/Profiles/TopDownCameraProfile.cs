using UnityEngine;

namespace _Project.Code.Gameplay.CameraSystems.Profiles
{
    [CreateAssetMenu(fileName = "TopDownCameraProfile", menuName = "ScriptableObjects/Camera/Top Down Camera Profile")]
    public class TopDownCameraProfile : CameraProfile
    {
        [field: SerializeField, Header("Top Down Settings")]
        public float Height { get; private set; } = 15f;

        [field: SerializeField, Range(30f, 90f)]
        public float LookDownAngle { get; private set; } = 60f;

        [field: SerializeField]
        public bool IsOrthographic { get; private set; } = false;

        [field: SerializeField, Range(5f, 20f)]
        public float OrthographicSize { get; private set; } = 10f;

        [field: SerializeField]
        public Vector3 PositionOffset { get; private set; } = Vector3.zero;

        [field: SerializeField, Range(1f, 10f)]
        public float FollowSmoothing { get; private set; } = 5f;

        [field: SerializeField]
        public bool AllowRotation { get; private set; } = false;

        [field: SerializeField, Range(10f, 90f)]
        public float RotationSpeed { get; private set; } = 45f;
    }
}