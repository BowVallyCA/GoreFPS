using _Project.Code.Core.Events;

namespace _Project.Code.Gameplay.GameManagement
{
    public struct GameTimerStartedEvent : IEvent
    {
        public float Duration;
    }

    public struct GameTimerTickEvent : IEvent
    {
        public float TimeRemaining;
        public float TimeElapsed;

        public string GetFormattedTime()
        {
            int totalSeconds = (int)TimeRemaining;
            int centiseconds = (int)((TimeRemaining - totalSeconds) * 100);

            if (totalSeconds >= 60)
            {
                int minutes = totalSeconds / 60;
                int seconds = totalSeconds % 60;
                return $"{minutes:D2}:{seconds:D2}.{centiseconds:D2}";
            }
            else
            {
                return $"{totalSeconds:D2}.{centiseconds:D2}";
            }
        }
    }

    public struct GameTimerFinishedEvent : IEvent
    {
    }
}
