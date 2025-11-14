using UnityEngine;
using _Project.Code.Core.StateMachine;
using _Project.Code.Core.ServiceLocator;
using _Project.Code.Core.Events;
using _Project.Code.Gameplay.Input;

namespace _Project.Code.Gameplay.GameManagement
{
    public class PausedState : BaseState
    {
        private readonly GameManagementService _gameManagement;
        private InputService _inputService;
        private float _previousTimeScale;

        public PausedState(GameManagementService gameManagement)
        {
            _gameManagement = gameManagement;
        }

        public override void Enter()
        {
            _inputService = ServiceLocator.Get<InputService>();
            _previousTimeScale = Time.timeScale;
            Time.timeScale = 0f;
            _inputService?.EnableUIActions();

            EventBus.Instance.Publish(new GameStateChangedEvent { StateName = "Paused" });
            EventBus.Instance.Publish(new GamePausedEvent());
        }

        public override void Exit()
        {
            Time.timeScale = _previousTimeScale;
            EventBus.Instance.Publish(new GameResumedEvent());
        }
    }
}
