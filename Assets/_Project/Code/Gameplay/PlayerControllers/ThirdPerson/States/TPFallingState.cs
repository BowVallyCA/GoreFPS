using UnityEngine;
using _Project.Code.Core.StateMachine;
using _Project.Code.Core.ServiceLocator;
using _Project.Code.Core.Events;
using _Project.Code.Gameplay.Input;
using _Project.Code.Gameplay.Animation;

namespace _Project.Code.Gameplay.PlayerControllers.ThirdPerson.States
{
    public class TPFallingState : TPBaseState
    {
        public TPFallingState(ThirdPersonController controller) : base(controller)
        {
        }

        public override void Update()
        {
            // Apply stronger gravity when falling
            var fallMultiplier = _controller.MovementProfile.FallGravityMultiplier;
            _controller.Motor.ApplyGravity(_controller.MovementProfile.Gravity * fallMultiplier);

            // Allow limited air control
            var moveDirection = _controller.GetCameraRelativeMovement();
            float currentSpeed = 0f;
            if (moveDirection.magnitude > 0)
            {
                var airSpeed = _controller.MovementProfile.WalkSpeed * _controller.MovementProfile.AirControlMultiplier;
                _controller.Motor.Move(moveDirection, airSpeed);
                currentSpeed = airSpeed;
            }

            float normalizedSpeed = currentSpeed / _controller.MovementProfile.RunSpeed;
            _controller.AnimationController.SetFloat(AnimationParameter.Speed, normalizedSpeed);

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
                _stateMachine.TransitionTo<TPJumpingState>();
                return;
            }

            if (_controller.MoveInput.magnitude == 0)
            {
                _stateMachine.TransitionTo<TPIdleState>();
            }
            else if (_controller.IsSprinting)
            {
                _stateMachine.TransitionTo<TPSprintingState>();
            }
            else
            {
                _stateMachine.TransitionTo<TPWalkingState>();
            }
        }

    }
}