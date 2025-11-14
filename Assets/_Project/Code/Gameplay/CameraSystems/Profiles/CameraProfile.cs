using UnityEngine;
using Unity.Cinemachine;

namespace _Project.Code.Gameplay.CameraSystems
{
    [CreateAssetMenu(fileName = "CameraProfile", menuName = "ScriptableObjects/Camera/Camera Profile")]
    public class CameraProfile : ScriptableObject
    {
        [field: SerializeField, Header("Cinemachine Settings")]
        public int Priority { get; private set; } = 10;

        [field: SerializeField]
        public float BlendTime { get; private set; } = 1f;

        [field: SerializeField, Header("Field of View"), Range(10f, 120f)]
        public float FieldOfView { get; private set; } = 60f;
    }
}
