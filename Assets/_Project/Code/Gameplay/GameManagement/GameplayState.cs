using UnityEngine;
using _Project.Code.Core.StateMachine;
using _Project.Code.Core.ServiceLocator;
using _Project.Code.Core.Events;
using _Project.Code.Gameplay.Input;

namespace _Project.Code.Gameplay.GameManagement
{
    public class GameplayState : BaseState
    {
        private readonly GameManagementService _gameManagement;
        private InputService _inputService;

        public GameplayState(GameManagementService gameManagement)
        {
            _gameManagement = gameManagement;
        }

        public override void Enter()
        {
            _inputService = ServiceLocator.Get<InputService>();
            Time.timeScale = 1f;
            _inputService?.EnableGameplayActions();

            EventBus.Instance.Publish(new GameStateChangedEvent { StateName = "Gameplay" });
        }
    }
}
