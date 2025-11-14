using UnityEngine;

namespace _Project.Code.Gameplay.Input
{
    [CreateAssetMenu(fileName = "InputProfile", menuName = "ScriptableObjects/Input/Input Profile")]
    public class InputProfile : ScriptableObject
    {
        [field: SerializeField, Header("Sensitivity")]
        public float MouseSensitivity { get; private set; } = 1f;

        [field: SerializeField]
        public float GamepadSensitivity { get; private set; } = 3f;

        [field: SerializeField, Header("Dead Zones"), Range(0f, 0.5f)]
        public float MoveDeadZone { get; private set; } = 0.1f;

        [field: SerializeField, Range(0f, 0.5f)]
        public float LookDeadZone { get; private set; } = 0.1f;

        [field: SerializeField, Header("Inversion")]
        public bool InvertY { get; private set; } = false;

        [field: SerializeField]
        public bool InvertX { get; private set; } = false;

        [field: SerializeField, Header("Input Thresholds")]
        public float MovementMagnitudeThreshold { get; private set; } = 0.1f;

        [field: SerializeField]
        public float DirectionMagnitudeThreshold { get; private set; } = 0.1f;

        [field: SerializeField]
        public float LookMagnitudeThreshold { get; private set; } = 0.01f;

        [field: SerializeField, Header("Rotation Settings")]
        public float RotationThreshold { get; private set; } = 0.1f;

        [field: SerializeField, Header("Velocity Thresholds")]
        public float VelocityEpsilon { get; private set; } = 0.01f;

        [field: SerializeField]
        public float GroundedVelocityThreshold { get; private set; } = 2f;
    }
}
