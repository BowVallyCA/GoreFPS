using UnityEngine;
using _Project.Code.Core.Events;
using _Project.Code.Gameplay.Input;
using _Project.Code.Gameplay.Animation;

namespace _Project.Code.Gameplay.PlayerControllers.SideScroller.States
{
    public class SideScrollWallSlideState : SideScrollBaseState
    {
        private Vector3 _wallNormal;
        private float _wallJumpInputDelayTime;

        public SideScrollWallSlideState(SideScrollerController controller) : base(controller)
        {
        }

        public override void Enter()
        {
            base.Enter();
            _wallJumpInputDelayTime = 0f;
            EventBus.Instance.Subscribe<JumpInputEvent>(this, HandleWallJump);
        }

        private void HandleWallJump(JumpInputEvent evt)
        {
            if (evt.IsPressed)
            {
                // Wall jump applies both horizontal and vertical force
                var wallJumpDirection = new Vector3(_wallNormal.x, 0f, 0f);
                var horizontalForce = wallJumpDirection * _controller.MovementProfile.WallJumpForce;
                var verticalForce = _controller.MovementProfile.WallJumpUpForce;

                // Set velocity for wall jump
                var wallJumpVelocity = new Vector3(horizontalForce.x, verticalForce, 0f);
                _controller.Motor.SetVelocity(wallJumpVelocity);

                _wallJumpInputDelayTime = Time.time;
                _controller.AnimationController.TriggerAnimation(AnimationTrigger.Jump);
                _stateMachine.TransitionTo<SideScrollJumpingState>();
            }
        }

        public override void Update()
        {
            // Check if still touching wall
            if (!_controller.CheckWall(out _wallNormal))
            {
                _stateMachine.TransitionTo<SideScrollFallingState>();
                return;
            }

            // Check if landed
            if (_controller.IsGrounded)
            {
                _controller.AnimationController.TriggerAnimation(AnimationTrigger.Land);
                _controller.AnimationController.SetBool(AnimationParameter.IsGrounded, true);
                _stateMachine.TransitionTo<SideScrollIdleState>();
                return;
            }

            // Apply wall slide speed
            var currentVelocity = _controller.Motor.Velocity;
            if (currentVelocity.y < -_controller.MovementProfile.WallSlideSpeed)
            {
                currentVelocity.y = -_controller.MovementProfile.WallSlideSpeed;
                _controller.Motor.SetVelocity(currentVelocity);
            }
            else
            {
                // Still apply some gravity
                _controller.Motor.ApplyGravity(_controller.MovementProfile.Gravity * 0.3f);
            }

            // Update animations
            _controller.AnimationController.SetFloat(AnimationParameter.VerticalSpeed, _controller.Motor.Velocity.y);
            _controller.AnimationController.SetBool(AnimationParameter.IsGrounded, false);

            // Limited horizontal input during wall slide (player can push away from wall)
            var moveDirection = new Vector3(_controller.MoveInput.x, 0f, 0f);
            if (Mathf.Abs(_controller.MoveInput.x) > 0.01f)
            {
                // Check if player is pushing away from wall
                float inputDirection = Mathf.Sign(_controller.MoveInput.x);
                float wallDirection = Mathf.Sign(_wallNormal.x);

                if (inputDirection == wallDirection)
                {
                    // Pushing away from wall - fall off
                    _stateMachine.TransitionTo<SideScrollFallingState>();
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
