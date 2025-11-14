using UnityEngine;

namespace _Project.Code.Gameplay.CameraSystems.Profiles
{
    [CreateAssetMenu(fileName = "TankCameraProfile", menuName = "ScriptableObjects/Camera/Tank Camera Profile")]
    public class TankCameraProfile : CameraProfile
    {
        [field: SerializeField, Header("Tank Camera Settings")]
        public Vector3 FollowOffset { get; private set; } = new Vector3(0f, 8f, -8f);

        [field: SerializeField, Range(1f, 10f)]
        public float FollowSmoothing { get; private set; } = 5f;

        [field: SerializeField]
        public bool FollowRotation { get; private set; } = false;

        [field: SerializeField, Header("Fixed Angle")]
        public Vector3 FixedRotation { get; private set; } = new Vector3(45f, 0f, 0f);

        [field: SerializeField]
        public bool UseFixedAngle { get; private set; } = true;
    }
}