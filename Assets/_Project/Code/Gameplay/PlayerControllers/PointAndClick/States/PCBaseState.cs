using _Project.Code.Core.StateMachine;

namespace _Project.Code.Gameplay.PlayerControllers.PointAndClick.States
{
    public abstract class PCBaseState : BaseState
    {
        protected readonly PointAndClickController _controller;
        protected FiniteStateMachine<IState> _stateMachine => _controller.StateMachine;

        public PCBaseState(PointAndClickController controller)
        {
            _controller = controller;
        }
    }
}
