using _Project.Code.Gameplay.Animation;

namespace _Project.Code.Gameplay.PlayerControllers.PointAndClick.States
{
    public class PCIdleState : PCBaseState
    {
        public PCIdleState(PointAndClickController controller) : base(controller)
        {
        }

        public override void Enter()
        {
            base.Enter();
            _controller.Motor.Stop();
            _controller.AnimationController?.SetFloat(AnimationParameter.Speed, 0f);
        }

        public override void Update()
        {
            if (_controller.HasDestination)
            {
                _stateMachine.TransitionTo<PCMovingState>();
            }
        }
    }
}
