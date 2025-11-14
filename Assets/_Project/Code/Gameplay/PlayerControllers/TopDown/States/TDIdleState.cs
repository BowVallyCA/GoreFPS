using UnityEngine;
using _Project.Code.Core.StateMachine;
using _Project.Code.Gameplay.Input;
using _Project.Code.Gameplay.Animation;

namespace _Project.Code.Gameplay.PlayerControllers.TopDown.States
{
    public class TDIdleState : TDBaseState
    {
        public TDIdleState(TopDownController controller) : base(controller)
        {
        }

        public override void Enter()
        {
            base.Enter();
            _controller.Motor.Stop();
        }

        public override void Update()
        {
            if (!_controller.IsGrounded)
            {
                _stateMachine.TransitionTo<TDFallingState>();
                return;
            }

            _controller.AnimationController.SetFloat(AnimationParameter.Speed, 0f);
            _controller.Motor.ApplyGravity(_controller.MovementProfile.Gravity);

            if (_controller.MovementProfile.UseTwinStickAiming && _controller.AimInput.magnitude > 0)
            {
                _controller.RotateTowardsAim();
            }
        }

        protected override void HandleMove(MoveInputEvent evt)
        {
            base.HandleMove(evt);

            if (_controller.IsMoving)
            {
                _stateMachine.TransitionTo<TDMovingState>();
            }
        }
    }
}