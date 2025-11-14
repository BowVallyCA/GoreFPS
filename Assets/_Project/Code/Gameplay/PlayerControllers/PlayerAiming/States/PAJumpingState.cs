using UnityEngine;
using _Project.Code.Core.StateMachine;
using _Project.Code.Core.ServiceLocator;
using _Project.Code.Gameplay.Input;
using _Project.Code.Gameplay.PlayerControllers.PlayerAiming;
using _Project.Code.Gameplay.Animation;

namespace _Project.Code.Gameplay.PlayerControllers.PlayerAiming.States
{
    public class PAJumpingState : PABaseState
    {
        public PAJumpingState(PlayerAimingController controller) : base(controller)
        {
        }

        public override void Enter()
        {
            base.Enter();

            _controller.AnimationController?.TriggerAnimation(AnimationTrigger.Jump);

            float jumpForce = _controller.MovementProfile.CalculateJumpForce();
            _controller.Motor.Jump(jumpForce);
        }

        public override void Update()
        {
            // Handle variable jump height
            if (_controller.MovementProfile.VariableJumpHeight)
            {
                if (!_controller.IsJumpHeld && _controller.Motor.Velocity.y > 0)
                {
                    // Calculate minimum velocity based on MinJumpMultiplier
                    float minJumpVelocity = _controller.MovementProfile.CalculateJumpForce() *
                                          _controller.MovementProfile.MinJumpMultiplier;

                    // Only cut jump if we're above minimum height
                    if (_controller.Motor.Velocity.y > minJumpVelocity)
                    {
                        _controller.Motor.CutJumpVelocity(0.4f); // Aggressive cut for responsive feel
                    }
                }
            }

            // Apply gravity
            var fallMultiplier = _controller.Motor.Velocity.y < 0 ? _controller.MovementProfile.FallGravityMultiplier : 1f;
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

            // Check if started falling
            if (_controller.Motor.Velocity.y <= 0)
            {
                _stateMachine.TransitionTo<PAFallingState>();
            }
        }

        public override void Exit()
        {
            base.Exit();
        }
    }
}
