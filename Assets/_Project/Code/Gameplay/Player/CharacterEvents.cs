using UnityEngine;
using _Project.Code.Core.Events;

namespace _Project.Code.Gameplay.Player
{
    public struct CharacterJumpedEvent : IEvent
    {
        public Transform Source;
        public float JumpForce;
    }

    public struct CharacterLandedEvent : IEvent
    {
        public Transform Source;
        public float FallSpeed;
    }

    public struct CharacterFootstepEvent : IEvent
    {
        public Transform Source;
    }

    public struct CharacterSprintStartedEvent : IEvent
    {
        public Transform Source;
    }

    public struct CharacterSprintStoppedEvent : IEvent
    {
        public Transform Source;
    }

    public struct CharacterDodgedEvent : IEvent
    {
        public Transform Source;
        public Vector3 Direction;
    }
}
