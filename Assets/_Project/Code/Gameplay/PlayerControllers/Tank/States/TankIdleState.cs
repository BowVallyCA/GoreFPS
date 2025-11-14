using UnityEngine;
using _Project.Code.Core.StateMachine;
using _Project.Code.Core.Events;
using _Project.Code.Gameplay.Input;
using _Project.Code.Gameplay.Animation;

namespace _Project.Code.Gameplay.PlayerControllers.Tank.States
{
    public class TankIdleState : TankBaseState
    {
        public TankIdleState(TankController controller) : base(controller)
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
                _stateMachine.TransitionTo<TankFallingState>();
                return;
            }

            _controller.AnimationController.SetFloat(AnimationParameter.Speed, 0f);
            _controller.Motor.ApplyGravity(_controller.MovementProfile.Gravity);
        }

        protected override void HandleMove(MoveInputEvent evt)
        {
            base.HandleMove(evt);

            if (_controller.IsMovingForward || _controller.IsMovingBackward || _controller.IsTurning)
            {
                _stateMachine.TransitionTo<TankMovingState>();
            }
        }

        public override void Exit()
        {
            base.Exit();
        }
    }
}