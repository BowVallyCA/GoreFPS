using UnityEngine;
using _Project.Code.Core.StateMachine;
using _Project.Code.Gameplay.Animation;

namespace _Project.Code.Gameplay.PlayerControllers.Tank.States
{
    public class TankFallingState : TankBaseState
    {
        public TankFallingState(TankController controller) : base(controller)
        {
        }

        public override void Enter()
        {
            base.Enter();
        }

        public override void Update()
        {
            _controller.Motor.ApplyGravity(_controller.MovementProfile.Gravity);

            if (_controller.IsTurning)
            {
                _controller.ExecuteTurn();
            }

            float normalizedSpeed = 0f;
            _controller.AnimationController.SetFloat(AnimationParameter.Speed, normalizedSpeed);

            if (_controller.IsGrounded)
            {
                TransitionToGroundState();
            }
        }

        private void TransitionToGroundState()
        {
            if (_controller.IsMovingForward || _controller.IsMovingBackward)
            {
                _stateMachine.TransitionTo<TankMovingState>();
            }
            else
            {
                _stateMachine.TransitionTo<TankIdleState>();
            }
        }

        public override void Exit()
        {
            base.Exit();
        }
    }
}
