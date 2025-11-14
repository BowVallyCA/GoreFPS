using _Project.Code.Core.StateMachine;
using _Project.Code.Core.Events;
using _Project.Code.Gameplay.Input;

namespace _Project.Code.Gameplay.PlayerControllers.Flying.States
{
    public abstract class FlyBaseState : BaseState
    {
        protected readonly FlyingController _controller;
        protected FiniteStateMachine<IState> _stateMachine => _controller.StateMachine;

        public FlyBaseState(FlyingController controller)
        {
            _controller = controller;
        }

        public override void Enter()
        {
            EventBus.Instance.Subscribe<MoveInputEvent>(this, HandleMove);
            EventBus.Instance.Subscribe<LookInputEvent>(this, HandleLook);
        }

        protected virtual void HandleMove(MoveInputEvent evt)
        {
            _controller.MoveInput = evt.Input;
        }

        protected virtual void HandleLook(LookInputEvent evt)
        {
            _controller.LookInput = evt.Input;
        }

        public override void Exit()
        {
            EventBus.Instance?.Unsubscribe<MoveInputEvent>(this);
            EventBus.Instance?.Unsubscribe<LookInputEvent>(this);
        }
    }
}
