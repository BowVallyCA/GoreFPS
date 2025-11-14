using UnityEngine;
using _Project.Code.Gameplay.Animation;

namespace _Project.Code.Gameplay.PlayerControllers.SideScroller.States
{
    public class SideScrollAirDashState : SideScrollBaseState
    {
        private float _dashStartTime;
        private Vector3 _dashDirection;
        private float _dashSpeed;
        private bool _dashCompleted;

        public SideScrollAirDashState(SideScrollerController controller) : base(controller)
        {
        }

        public override void Enter()
        {
            base.Enter();
            _dashStartTime = Time.time;
            _dashCompleted = false;

            // Determine dash direction based on input or facing direction
            if (Mathf.Abs(_controller.MoveInput.x) > 0.1f)
            {
                _dashDirection = new Vector3(Mathf.Sign(_controller.MoveInput.x), 0f, 0f);
            }
            else
            {
                // Dash in the direction the player is facing
                float facingDirection = Mathf.Sign(_controller.transform.rotation.eulerAngles.y == 90f ? 1f : -1f);
                _dashDirection = new Vector3(facingDirection, 0f, 0f);
            }

            // Calculate dash speed to cover the distance in the duration
            _dashSpeed = _controller.MovementProfile.AirDashDistance / _controller.MovementProfile.AirDashDuration;

            // Consume the dash
            _controller.ConsumeAirDash();

            // Zero out vertical velocity for horizontal dash
            var velocity = _controller.Motor.Velocity;
            velocity.y = 0f;
            _controller.Motor.SetVelocity(velocity);
        }

        public override void Update()
        {
            float dashElapsed = Time.time - _dashStartTime;

            // Check if dash duration completed
            if (dashElapsed >= _controller.MovementProfile.AirDashDuration)
            {
                _dashCompleted = true;

                // Check if grounded during or after dash
                if (_controller.IsGrounded)
                {
                    _controller.AnimationController.TriggerAnimation(AnimationTrigger.Land);
                    _controller.AnimationController.SetBool(AnimationParameter.IsGrounded, true);
                    _stateMachine.TransitionTo<SideScrollIdleState>();
                }
                else
                {
                    _stateMachine.TransitionTo<SideScrollFallingState>();
                }
                return;
            }

            // Execute dash movement
            var dashVelocity = _dashDirection * _dashSpeed;
            dashVelocity.y = 0f; // Keep vertical velocity at zero during dash
            _controller.Motor.SetVelocity(dashVelocity);

            // Update animations
            _controller.AnimationController.SetFloat(AnimationParameter.Speed, 1f);
            _controller.AnimationController.SetFloat(AnimationParameter.VerticalSpeed, 0f);
            _controller.AnimationController.SetBool(AnimationParameter.IsGrounded, false);

            // Face the dash direction
            var targetRotation = Quaternion.Euler(0f, _dashDirection.x > 0 ? 90f : -90f, 0f);
            _controller.transform.rotation = targetRotation;
        }
    }
}
