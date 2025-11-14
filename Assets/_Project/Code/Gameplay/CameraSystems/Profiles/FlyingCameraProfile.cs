using UnityEngine;

namespace _Project.Code.Gameplay.CameraSystems.Profiles
{
    [CreateAssetMenu(fileName = "FlyingCameraProfile", menuName = "ScriptableObjects/Camera/Flying Camera Profile")]
    public class FlyingCameraProfile : CameraProfile
    {
        [field: SerializeField, Header("Flying Settings"), Tooltip("Camera offset behind aircraft")]
        public Vector3 FollowOffset { get; private set; } = new Vector3(0f, 2f, -8f);

        [field: SerializeField, Tooltip("Follow smoothing speed"), Range(1f, 20f)]
        public float FollowSmoothing { get; private set; } = 8f;

        [field: SerializeField, Tooltip("Rotation smoothing speed"), Range(1f, 20f)]
        public float RotationSmoothing { get; private set; } = 6f;

        [field: SerializeField, Header("Banking"), Tooltip("Follow aircraft banking?")]
        public bool FollowBanking { get; private set; } = true;

        [field: SerializeField, Tooltip("Bank follow intensity"), Range(0f, 1f)]
        public float BankFollowIntensity { get; private set; } = 0.7f;

        [field: SerializeField, Header("Look Ahead"), Tooltip("Look ahead in flight direction?")]
        public bool EnableLookAhead { get; private set; } = true;

        [field: SerializeField, Tooltip("Look ahead distance")]
        public float LookAheadDistance { get; private set; } = 5f;

        [field: SerializeField, Header("Dynamic FOV"), Tooltip("Adjust FOV based on speed?")]
        public bool DynamicFOV { get; private set; } = true;

        [field: SerializeField, Tooltip("FOV increase per speed unit")]
        public float FOVSpeedMultiplier { get; private set; } = 0.5f;

        [field: SerializeField, Tooltip("Maximum FOV increase")]
        public float MaxFOVIncrease { get; private set; } = 20f;
    }
}
