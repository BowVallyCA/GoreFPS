using _Project.Code.Core.Events;

namespace _Project.Code.Gameplay.Scenes
{
    public struct SceneLoadStartedEvent : IEvent
    {
        public string SceneName;
    }

    public struct SceneLoadedEvent : IEvent
    {
        public string SceneName;
    }
}
