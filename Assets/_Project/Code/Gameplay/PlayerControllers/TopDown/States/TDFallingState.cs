using UnityEngine;
using _Project.Code.Core.StateMachine;
using _Project.Code.Gameplay.Animation;

namespace _Project.Code.Gameplay.PlayerControllers.TopDown.States
{
    public class TDFallingState : TDBaseState
    {
        public TDFallingState(TopDownController controller) : base(controller)
        {
        }

        public override void Enter()
        {
            base.Enter();
        }

        public override void Update()
        {
            _controller.Motor.ApplyGravity(_controller.MovementProfile.Gravity);

            float normalizedSpeed = 0f;
            _controller.AnimationController.SetFloat(AnimationParameter.Speed, normalizedSpeed);

            if (_controller.IsGrounded)
            {
                TransitionToGroundState();
            }
        }

        private void TransitionToGroundState()
        {
            if (_controller.IsMoving)
            {
                _stateMachine.TransitionTo<TDMovingState>();
            }
            else
            {
                _stateMachine.TransitionTo<TDIdleState>();
            }
        }

        public override void Exit()
        {
            base.Exit();
        }
    }
}
