using UnityEngine;
using _Project.Code.Core.Events;

namespace _Project.Code.Gameplay.Input
{
    public struct MoveInputEvent : IEvent
    {
        public Vector2 Input;
    }

    public struct LookInputEvent : IEvent
    {
        public Vector2 Input;
    }

    public struct JumpInputEvent : IEvent
    {
        public bool IsPressed;
    }

    public struct SprintInputEvent : IEvent
    {
        public bool IsPressed;
    }

    public struct AttackInputEvent : IEvent { }

    public struct InteractInputEvent : IEvent { }

    public struct DodgeInputEvent : IEvent { }

    public struct LockOnInputEvent : IEvent
    {
        public bool IsPressed;
    }

    public struct CameraRotateInputEvent : IEvent
    {
        public float Input;
    }

    public struct PauseInputEvent : IEvent { }
}
