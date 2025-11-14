using UnityEngine;
using _Project.Code.Gameplay.Animation;
using _Project.Code.Core.Events;
using _Project.Code.Gameplay.Input;

namespace _Project.Code.Gameplay.PlayerControllers.SideScroller.States
{
    public class SideScrollJumpingState : SideScrollBaseState
    {
        public SideScrollJumpingState(SideScrollerController controller) : base(controller)
        {
        }

        public override void Enter()
        {
            base.Enter();
            _controller.AnimationController.TriggerAnimation(AnimationTrigger.Jump);
            var jumpVelocity = _controller.MovementProfile.CalculateJumpForce();
            _controller.Motor.Jump(jumpVelocity);

            if (_controller.MovementProfile.EnableDoubleJump)
            {
                EventBus.Instance.Subscribe<JumpInputEvent>(this, HandleDoubleJump);
            }

            if (_controller.MovementProfile.EnableAirDash)
            {
                EventBus.Instance.Subscribe<DodgeInputEvent>(this, HandleDodge);
            }
        }

        private void HandleDoubleJump(JumpInputEvent evt)
        {
            if (evt.IsPressed && _controller.AirJumpsRemaining > 0)
            {
                _controller.AirJumpsRemaining--;
                _controller.AnimationController.TriggerAnimation(AnimationTrigger.Jump);
                var airJumpVelocity = _controller.MovementProfile.CalculateAirJumpForce();
                _controller.Motor.Jump(airJumpVelocity);
            }
        }

        private void HandleDodge(DodgeInputEvent evt)
        {
            if (_controller.CanAirDash)
            {
                _stateMachine.TransitionTo<SideScrollAirDashState>();
            }
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
                        _controller.Motor.CutJumpVelocity(0.4f);
                    }
                }
            }

            // Apply gravity with apex hang time
            float gravityMultiplier = 1f;

            // Apex hang time - reduce gravity near peak of jump
            if (_controller.MovementProfile.UseApexHangTime &&
                Mathf.Abs(_controller.Motor.Velocity.y) < _controller.MovementProfile.ApexThreshold)
            {
                gravityMultiplier = _controller.MovementProfile.ApexGravityMultiplier;
            }
            // Falling gravity multiplier
            else if (_controller.Motor.Velocity.y < 0)
            {
                gravityMultiplier = _controller.MovementProfile.FallGravityMultiplier;
            }

            _controller.Motor.ApplyGravity(_controller.MovementProfile.Gravity * gravityMultiplier);

            // Update animations
            _controller.AnimationController.SetFloat(AnimationParameter.VerticalSpeed, _controller.Motor.Velocity.y);
            _controller.AnimationController.SetBool(AnimationParameter.IsGrounded, false);

            // Allow air control - X axis only
            var moveDirection = new Vector3(_controller.MoveInput.x, 0f, 0f);
            if (Mathf.Abs(_controller.MoveInput.x) > 0.01f)
            {
                // Preserve momentum: use current horizontal speed as base, with air control
                var currentHorizontalSpeed = Mathf.Abs(_controller.Motor.Velocity.x);
                var maxAirSpeed = (_controller.IsSprinting ? _controller.MovementProfile.RunSpeed : _controller.MovementProfile.WalkSpeed);
                var targetAirSpeed = Mathf.Max(currentHorizontalSpeed, maxAirSpeed * _controller.MovementProfile.AirControlMultiplier);

                _controller.Motor.Move(moveDirection, targetAirSpeed);

                // Face the movement direction
                var targetRotation = Quaternion.Euler(0f, _controller.MoveInput.x > 0 ? 90f : -90f, 0f);
                _controller.transform.rotation = Quaternion.RotateTowards(
                    _controller.transform.rotation,
                    targetRotation,
                    _controller.MovementProfile.RotationSpeed * Time.deltaTime
                );
            }

            // Check if started falling
            if (_controller.Motor.Velocity.y <= 0)
            {
                _stateMachine.TransitionTo<SideScrollFallingState>();
            }
        }

        public override void Exit()
        {
            base.Exit();
            if (_controller.MovementProfile.EnableDoubleJump)
            {
                EventBus.Instance?.Unsubscribe<JumpInputEvent>(this);
            }
            if (_controller.MovementProfile.EnableAirDash)
            {
                EventBus.Instance?.Unsubscribe<DodgeInputEvent>(this);
            }
        }
    }
}
