using _Project.Code.Core.ServiceLocator;
using _Project.Code.Gameplay.Input;

namespace _Project.Code.Gameplay.PlayerControllers.Grid.States
{
    public class GridIdleState : GridBaseState
    {
        public GridIdleState(GridController controller) : base(controller)
        {
        }

        public override void Enter()
        {
            base.Enter();
            _controller.Motor.Stop();
        }

        public override void Update()
        {
            // Wait for input
        }

        protected override void HandleMove(MoveInputEvent evt)
        {
            if (evt.Input.magnitude > ServiceLocator.Get<InputService>().Profile.MovementMagnitudeThreshold)
            {
                _stateMachine.TransitionTo<GridMovingState>();
            }
        }
    }
}
