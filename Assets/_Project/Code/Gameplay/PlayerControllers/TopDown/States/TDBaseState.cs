using UnityEngine;
using _Project.Code.Core.StateMachine;
using _Project.Code.Core.Events;
using _Project.Code.Gameplay.Input;

namespace _Project.Code.Gameplay.PlayerControllers.TopDown.States
{
    public abstract class TDBaseState : BaseState
    {
        protected readonly TopDownController _controller;
        protected FiniteStateMachine<IState> _stateMachine => _controller.StateMachine;

        protected TDBaseState(TopDownController controller)
        {
            _controller = controller;
        }

        public override void Enter()
        {
            EventBus.Instance.Subscribe<MoveInputEvent>(this, HandleMove);
        }

        protected virtual void HandleMove(MoveInputEvent evt)
        {
            _controller.MoveInput = evt.Input;
        }

        public override void Exit()
        {
            EventBus.Instance?.Unsubscribe<MoveInputEvent>(this);
        }
    }
}
