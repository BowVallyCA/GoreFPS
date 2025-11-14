using UnityEngine;
using _Project.Code.Core.ServiceLocator;
using _Project.Code.Core.Events;

namespace _Project.Code.Gameplay.GameManagement
{
    public class GameTimerService : MonoBehaviourService
    {
        [SerializeField] private float _durationInSeconds = 60f;
        [SerializeField] private float _tickRate = 0.1f;

        private float _timeRemaining;
        private float _timeSinceLastTick;
        private bool _isRunning;
        private bool _isInGameplayState;

        public float TimeRemaining => _timeRemaining;
        public float TimeElapsed => _durationInSeconds - _timeRemaining;
        public bool IsRunning => _isRunning;

        public override void Initialize()
        {
            _timeRemaining = _durationInSeconds;
            _isRunning = false;
            _isInGameplayState = false;

            EventBus.Instance.Subscribe<GameStateChangedEvent>(this, OnGameStateChanged);
            EventBus.Instance.Subscribe<GamePausedEvent>(this, OnGamePaused);
            EventBus.Instance.Subscribe<GameResumedEvent>(this, OnGameResumed);

            // Check if we're already in Gameplay state (since GameManagementService initializes before us)
            var gameManagement = ServiceLocator.Get<GameManagementService>();
            if (gameManagement?.CurrentState is GameplayState)
            {
                _isInGameplayState = true;
                StartTimer();
            }
        }

        private void Update()
        {
            if (!_isRunning || !_isInGameplayState)
                return;

            _timeRemaining -= Time.deltaTime;
            _timeSinceLastTick += Time.deltaTime;

            if (_timeSinceLastTick >= _tickRate)
            {
                _timeSinceLastTick = 0f;
                EventBus.Instance.Publish(new GameTimerTickEvent
                {
                    TimeRemaining = _timeRemaining,
                    TimeElapsed = TimeElapsed
                });
            }

            if (_timeRemaining <= 0f)
            {
                _timeRemaining = 0f;
                _isRunning = false;
                EventBus.Instance.Publish(new GameTimerFinishedEvent());
            }
        }

        private void StartTimer()
        {
            _timeRemaining = _durationInSeconds;
            _timeSinceLastTick = 0f;
            _isRunning = true;
            EventBus.Instance.Publish(new GameTimerStartedEvent { Duration = _durationInSeconds });
        }

        public void StopTimer()
        {
            _isRunning = false;
        }

        public void ResetTimer()
        {
            _timeRemaining = _durationInSeconds;
            _isRunning = false;
        }

        private void OnGameStateChanged(GameStateChangedEvent evt)
        {
            _isInGameplayState = evt.StateName == "Gameplay";

            if (_isInGameplayState)
            {
                StartTimer();
            }
            else
            {
                _isRunning = false;
            }
        }

        private void OnGamePaused(GamePausedEvent evt)
        {
            // Timer automatically stops running when paused since Update checks _isInGameplayState
            // We keep _isRunning true so it can resume when unpaused
        }

        private void OnGameResumed(GameResumedEvent evt)
        {
            // Timer automatically resumes if _isRunning is true and we're in gameplay state
        }

        public override void Dispose()
        {
            EventBus.Instance?.Unsubscribe<GameStateChangedEvent>(this);
            EventBus.Instance?.Unsubscribe<GamePausedEvent>(this);
            EventBus.Instance?.Unsubscribe<GameResumedEvent>(this);
        }
    }
}
