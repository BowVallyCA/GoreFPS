using _Project.Code.Core.StateMachine;
using _Project.Code.Core.Events;
using _Project.Code.Gameplay.Input;

namespace _Project.Code.Gameplay.PlayerControllers.Grid.States
{
    public abstract class GridBaseState : BaseState
    {
        protected readonly GridController _controller;
        protected FiniteStateMachine<IState> _stateMachine => _controller.StateMachine;

        public GridBaseState(GridController controller)
        {
            _controller = controller;
        }

        public override void Enter()
        {
            EventBus.Instance.Subscribe<MoveInputEvent>(this, HandleMove);
        }

        protected virtual void HandleMove(MoveInputEvent evt)
        {
            // States can override to handle move input
        }

        public override void Exit()
        {
            EventBus.Instance?.Unsubscribe<MoveInputEvent>(this);
        }
    }
}
