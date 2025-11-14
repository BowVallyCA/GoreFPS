using UnityEngine;
using _Project.Code.Core.Events;
using _Project.Code.Core.ServiceLocator;
using _Project.Code.Gameplay.Input;
using _Project.Code.Gameplay.Animation;

namespace _Project.Code.Gameplay.PlayerControllers.SideScroller.States
{
    public class SideScrollIdleState : SideScrollBaseState
    {
        public SideScrollIdleState(SideScrollerController controller) : base(controller)
        {
        }

        public override void Enter()
        {
            base.Enter();
            _controller.ResetAirMechanics();
            EventBus.Instance.Subscribe<JumpInputEvent>(this, HandleJump);
        }

        public override void Update()
        {
            if (!_controller.IsGrounded)
            {
                _stateMachine.TransitionTo<SideScrollFallingState>();
                return;
            }

            // Check for buffered jump after landing
            if (Time.time - _controller.LastJumpInputTime < _controller.MovementProfile.JumpBufferTime &&
                _controller.CanJump)
            {
                _controller.LastJumpInputTime = 0; // Clear buffer
                _stateMachine.TransitionTo<SideScrollJumpingState>();
                return;
            }

            _controller.AnimationController.SetFloat(AnimationParameter.Speed, 0f);
            _controller.Motor.ApplyGravity(_controller.MovementProfile.Gravity);
        }

        private void HandleJump(JumpInputEvent evt)
        {
            if (evt.IsPressed && _controller.CanJump)
            {
                _stateMachine.TransitionTo<SideScrollJumpingState>();
            }
        }

        protected override void HandleMove(MoveInputEvent evt)
        {
            base.HandleMove(evt);

            if (Mathf.Abs(evt.Input.x) > ServiceLocator.Get<InputService>().Profile.MovementMagnitudeThreshold)
            {
                if (_controller.IsSprinting)
                {
                    _stateMachine.TransitionTo<SideScrollRunningState>();
                }
                else
                {
                    _stateMachine.TransitionTo<SideScrollWalkingState>();
                }
            }
        }

        public override void Exit()
        {
            base.Exit();
            EventBus.Instance?.Unsubscribe<JumpInputEvent>(this);
        }
    }
}
