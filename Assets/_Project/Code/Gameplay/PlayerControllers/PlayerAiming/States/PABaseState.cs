using UnityEngine;
using _Project.Code.Core.StateMachine;
using _Project.Code.Core.Events;
using _Project.Code.Gameplay.Input;

namespace _Project.Code.Gameplay.PlayerControllers.PlayerAiming.States
{
    public abstract class PABaseState : BaseState
    {
        protected readonly PlayerAimingController _controller;
        protected FiniteStateMachine<IState> _stateMachine => _controller.StateMachine;

        public PABaseState(PlayerAimingController controller)
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