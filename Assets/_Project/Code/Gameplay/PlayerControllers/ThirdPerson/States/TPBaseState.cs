using UnityEngine;
using _Project.Code.Core.StateMachine;
using _Project.Code.Core.Events;
using _Project.Code.Gameplay.Input;

namespace _Project.Code.Gameplay.PlayerControllers.ThirdPerson.States
{
    public abstract class TPBaseState : BaseState
    {
        protected readonly ThirdPersonController _controller;
        protected FiniteStateMachine<IState> _stateMachine => _controller.StateMachine;

        public TPBaseState(ThirdPersonController controller)
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