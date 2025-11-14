using _Project.Code.Core.ServiceLocator;
using _Project.Code.Gameplay.Input;

namespace _Project.Code.Gameplay.PlayerControllers.Flying.States
{
    public class FlyIdleState : FlyBaseState
    {
        public FlyIdleState(FlyingController controller) : base(controller)
        {
        }

        public override void Enter()
        {
            base.Enter();
            _controller.Motor.Stop();
        }

        public override void Update()
        {
            // Idle hovering - gentle descent
            _controller.Motor.ApplyGravity(-2f);
        }

        protected override void HandleMove(MoveInputEvent evt)
        {
            base.HandleMove(evt);

            if (evt.Input.magnitude > ServiceLocator.Get<InputService>().Profile.MovementMagnitudeThreshold)
            {
                if (_controller.IsBoosting)
                {
                    _stateMachine.TransitionTo<FlyBoostingState>();
                }
                else
                {
                    _stateMachine.TransitionTo<FlyFlyingState>();
                }
            }
        }
    }
}
