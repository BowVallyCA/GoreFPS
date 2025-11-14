using UnityEngine;
using _Project.Code.Core.ServiceLocator;
using _Project.Code.Core.StateMachine;
using _Project.Code.Core.Events;
using _Project.Code.Gameplay.Input;

namespace _Project.Code.Gameplay.GameManagement
{
    public class GameManagementService : MonoBehaviourService
    {
        private FiniteStateMachine<IState> _stateMachine;

        public IState CurrentState => _stateMachine?.CurrentState;

        public override void Initialize()
        {
            var gameplayState = new GameplayState(this);
            _stateMachine = new FiniteStateMachine<IState>(gameplayState);

            _stateMachine.AddState(new PausedState(this));
            _stateMachine.AddState(new MenuState(this));

            EventBus.Instance.Subscribe<PauseInputEvent>(this, HandlePauseInput);
        }

        private void HandlePauseInput(PauseInputEvent evt)
        {
            if (_stateMachine.CurrentState is GameplayState)
            {
                TransitionToPaused();
            }
            else if (_stateMachine.CurrentState is PausedState)
            {
                TransitionToGameplay();
            }
        }

        public void TransitionToGameplay() => _stateMachine.TransitionTo<GameplayState>();
        public void TransitionToPaused() => _stateMachine.TransitionTo<PausedState>();
        public void TransitionToMenu() => _stateMachine.TransitionTo<MenuState>();

        private void Update()
        {
            _stateMachine?.Update();
        }

        public override void Dispose()
        {
            EventBus.Instance?.Unsubscribe<PauseInputEvent>(this);
        }
    }
}
