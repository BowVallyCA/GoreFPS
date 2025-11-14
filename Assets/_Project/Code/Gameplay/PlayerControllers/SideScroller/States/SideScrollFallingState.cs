using UnityEngine;
using _Project.Code.Core.ServiceLocator;
using _Project.Code.Core.Events;
using _Project.Code.Gameplay.Input;
using _Project.Code.Gameplay.Animation;

namespace _Project.Code.Gameplay.PlayerControllers.SideScroller.States
{
    public class SideScrollFallingState : SideScrollBaseState
    {
        public SideScrollFallingState(SideScrollerController controller) : base(controller)
        {
        }

        public override void Enter()
        {
            base.Enter();

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
                _stateMachine.TransitionTo<SideScrollJumpingState>();
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
            // Check for wall slide
            if (_controller.MovementProfile.EnableWallMechanics && _controller.CheckWall(out _))
            {
                _stateMachine.TransitionTo<SideScrollWallSlideState>();
                return;
            }

            // Apply gravity with fast fall option
            float fallMultiplier = _controller.MovementProfile.FallGravityMultiplier;

            // Fast fall when holding down
            if (_controller.MovementProfile.UseFastFall && _controller.MoveInput.y < -0.5f)
            {
                fallMultiplier *= _controller.MovementProfile.FastFallMultiplier;
            }

            _controller.Motor.ApplyGravity(_controller.MovementProfile.Gravity * fallMultiplier);

            // Cap max fall speed
            if (_controller.Motor.Velocity.y < -_controller.MovementProfile.MaxFallSpeed)
            {
                var cappedVelocity = _controller.Motor.Velocity;
                cappedVelocity.y = -_controller.MovementProfile.MaxFallSpeed;
                _controller.Motor.SetVelocity(cappedVelocity);
            }

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

            // Check if landed
            if (_controller.IsGrounded)
            {
                TransitionToGroundState();
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

        private void TransitionToGroundState()
        {
            // Trigger landing animation
            _controller.AnimationController.TriggerAnimation(AnimationTrigger.Land);
            _controller.AnimationController.SetBool(AnimationParameter.IsGrounded, true);

            // Check for buffered jump input
            if (Time.time - _controller.LastJumpInputTime < _controller.MovementProfile.JumpBufferTime)
            {
                _controller.LastJumpInputTime = 0; // Clear buffer
                _stateMachine.TransitionTo<SideScrollJumpingState>();
                return;
            }

            if (Mathf.Abs(_controller.MoveInput.x) > ServiceLocator.Get<InputService>().Profile.MovementMagnitudeThreshold)
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
            else
            {
                _stateMachine.TransitionTo<SideScrollIdleState>();
            }
        }
    }
}
