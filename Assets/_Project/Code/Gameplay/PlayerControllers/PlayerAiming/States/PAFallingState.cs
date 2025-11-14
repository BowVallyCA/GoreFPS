using UnityEngine;
using _Project.Code.Core.StateMachine;
using _Project.Code.Core.ServiceLocator;
using _Project.Code.Core.Events;
using _Project.Code.Gameplay.Input;
using _Project.Code.Gameplay.PlayerControllers.PlayerAiming;
using _Project.Code.Gameplay.Animation;

namespace _Project.Code.Gameplay.PlayerControllers.PlayerAiming.States
{
    public class PAFallingState : PABaseState
    {
        private Vector2 _moveInput;

        public PAFallingState(PlayerAimingController controller) : base(controller)
        {
        }

        public override void Enter()
        {
            base.Enter();
        }

        public override void Update()
        {
            // Apply stronger gravity when falling
            var fallMultiplier = _controller.MovementProfile.FallGravityMultiplier;
            _controller.Motor.ApplyGravity(_controller.MovementProfile.Gravity * fallMultiplier);

            // Allow limited air control
            Vector3 moveDirection = _controller.transform.right * _controller.MoveInput.x + _controller.transform.forward * _controller.MoveInput.y;
            float currentSpeed = 0f;
            if (moveDirection.magnitude > 0)
            {
                var airSpeed = _controller.MovementProfile.WalkSpeed * _controller.MovementProfile.AirControlMultiplier;
                _controller.Motor.Move(moveDirection, airSpeed);
                currentSpeed = airSpeed;
            }

            float normalizedSpeed = currentSpeed / _controller.MovementProfile.RunSpeed;
            _controller.AnimationController?.SetFloat(AnimationParameter.Speed, normalizedSpeed);

            // Check if landed
            if (_controller.IsGrounded)
            {
                TransitionToGroundState();
            }
        }

        private void TransitionToGroundState()
        {
            // Check for buffered jump input
            if (Time.time - _controller.LastJumpInputTime < _controller.MovementProfile.JumpBufferTime)
            {
                _controller.LastJumpInputTime = 0; // Clear buffer
                _stateMachine.TransitionTo<PAJumpingState>();
                return;
            }

            if (_moveInput.magnitude > ServiceLocator.Get<InputService>().Profile.MovementMagnitudeThreshold)
            {
                if (_controller.IsSprinting)
                {
                    _stateMachine.TransitionTo<PARunningState>();
                }
                else
                {
                    _stateMachine.TransitionTo<PAWalkingState>();
                }
            }
            else
            {
                _stateMachine.TransitionTo<PAIdleState>();
            }
        }

        protected override void HandleMove(MoveInputEvent evt)
        {
            base.HandleMove(evt);
            _moveInput = evt.Input;
        }

        public override void Exit()
        {
            base.Exit();
        }
    }
}
