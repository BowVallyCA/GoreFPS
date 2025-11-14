using UnityEngine;
using _Project.Code.Core.StateMachine;
using _Project.Code.Core.ServiceLocator;
using _Project.Code.Core.Events;
using _Project.Code.Gameplay.Input;
using _Project.Code.Gameplay.PlayerControllers.PlayerAiming;
using _Project.Code.Gameplay.Animation;

namespace _Project.Code.Gameplay.PlayerControllers.PlayerAiming.States
{
    public class PAIdleState : PABaseState
    {
        public PAIdleState(PlayerAimingController controller) : base(controller)
        {
        }

        public override void Enter()
        {
            base.Enter();
            _controller.Motor.Stop();
            EventBus.Instance.Subscribe<JumpInputEvent>(this, HandleJump);
        }

        public override void Update()
        {
            if (!_controller.IsGrounded)
            {
                _stateMachine.TransitionTo<PAFallingState>();
                return;
            }

            // Check for buffered jump after landing
            if (Time.time - _controller.LastJumpInputTime < _controller.MovementProfile.JumpBufferTime &&
                _controller.CanJump)
            {
                _controller.LastJumpInputTime = 0; // Clear buffer
                _stateMachine.TransitionTo<PAJumpingState>();
                return;
            }

            _controller.AnimationController?.SetFloat(AnimationParameter.Speed, 0f);

            if (_controller.IsGrounded)
            {
                _controller.Motor.ApplyGravity(_controller.MovementProfile.Gravity);
            }
        }

        private void HandleJump(JumpInputEvent evt)
        {
            if (evt.IsPressed && _controller.CanJump)
            {
                _stateMachine.TransitionTo<PAJumpingState>();
            }
        }

        protected override void HandleMove(MoveInputEvent evt)
        {
            base.HandleMove(evt);

            if (evt.Input.magnitude > ServiceLocator.Get<InputService>().Profile.MovementMagnitudeThreshold)
            {
                Debug.Log($"Movement magnitude of {evt.Input.magnitude}");
                if (_controller.IsSprinting)
                {
                    _stateMachine.TransitionTo<PARunningState>();
                }
                else
                {
                    _stateMachine.TransitionTo<PAWalkingState>();
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
