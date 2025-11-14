using UnityEngine;
using _Project.Code.Core.Events;

namespace _Project.Code.Gameplay.CameraSystems
{
    public struct CameraTargetChangedEvent : IEvent
    {
        public Transform FollowTarget;
        public Transform LookTarget;
    }

    public struct CameraModeChangedEvent : IEvent
    {
        public CameraMode Mode;
        public float BlendTime;
    }

    public struct CameraZoneEnteredEvent : IEvent
    {
        public CameraProfile Profile;
        public float BlendTime;
        public int Priority;
    }

    public struct CameraZoneExitedEvent : IEvent
    {
        public CameraProfile Profile;
    }

    public struct CameraShakeEvent : IEvent
    {
        public float Intensity;
        public float Duration;
    }

    public struct CameraFOVChangeEvent : IEvent
    {
        public float TargetFOV;
        public float Duration;
    }

    public struct CameraInputEnabledEvent : IEvent
    {
        public bool Enabled;
    }

    public enum CameraMode
    {
        FirstPerson,
        ThirdPerson,
        Orbit,
        TopDown,
        SideScroller,
        Fixed,
        Spline,
        Custom
    }
}
