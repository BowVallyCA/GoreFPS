using _Project.Code.Core.Events;

namespace _Project.Code.Gameplay.GameManagement
{
    public struct GameStateChangedEvent : IEvent
    {
        public string StateName;
    }

    public struct GamePausedEvent : IEvent { }

    public struct GameResumedEvent : IEvent { }
}
