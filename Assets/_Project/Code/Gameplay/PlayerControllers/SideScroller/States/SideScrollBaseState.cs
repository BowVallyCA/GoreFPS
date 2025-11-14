using _Project.Code.Core.StateMachine;
using _Project.Code.Core.Events;
using _Project.Code.Gameplay.Input;

namespace _Project.Code.Gameplay.PlayerControllers.SideScroller.States
{
    public abstract class SideScrollBaseState : BaseState
    {
        protected readonly SideScrollerController _controller;
        protected FiniteStateMachine<IState> _stateMachine => _controller.StateMachine;

        public SideScrollBaseState(SideScrollerController controller)
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
