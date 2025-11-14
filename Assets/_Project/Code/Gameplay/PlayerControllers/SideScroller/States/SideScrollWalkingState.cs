using UnityEngine;
using _Project.Code.Core.Events;
using _Project.Code.Core.ServiceLocator;
using _Project.Code.Gameplay.Input;
using _Project.Code.Gameplay.Animation;

namespace _Project.Code.Gameplay.PlayerControllers.SideScroller.States
{
    public class SideScrollWalkingState : SideScrollBaseState
    {
        private float _currentSpeed;

        public SideScrollWalkingState(SideScrollerController controller) : base(controller)
        {
        }

        public override void Enter()
        {
            base.Enter();
            _currentSpeed = Mathf.Abs(_controller.Motor.Velocity.x);
            _controller.ResetAirMechanics();
            EventBus.Instance.Subscribe<JumpInputEvent>(this, HandleJump);
            EventBus.Instance.Subscribe<SprintInputEvent>(this, HandleSprint);
        }

        public override void Update()
        {
            if (!_controller.IsGrounded)
            {
                _stateMachine.TransitionTo<SideScrollFallingState>();
                return;
            }

            _controller.Motor.ApplyGravity(_controller.MovementProfile.Gravity);

            var targetSpeed = Mathf.Abs(_controller.MoveInput.x) > 0.01f ? _controller.MovementProfile.WalkSpeed : 0f;
            var acceleration = targetSpeed > 0 ? _controller.MovementProfile.Acceleration : _controller.MovementProfile.Deceleration;
            _currentSpeed = Mathf.MoveTowards(_currentSpeed, targetSpeed, acceleration * Time.deltaTime);

            var moveDirection = new Vector3(_controller.MoveInput.x, 0f, 0f);
            _controller.Motor.Move(moveDirection, _currentSpeed);

            float normalizedSpeed = _currentSpeed / _controller.MovementProfile.RunSpeed;
            _controller.AnimationController.SetFloat(AnimationParameter.Speed, normalizedSpeed);

            // Face the movement direction
            if (Mathf.Abs(_controller.MoveInput.x) > 0.01f)
            {
                var targetRotation = Quaternion.Euler(0f, _controller.MoveInput.x > 0 ? 90f : -90f, 0f);
                _controller.transform.rotation = Quaternion.RotateTowards(
                    _controller.transform.rotation,
                    targetRotation,
                    _controller.MovementProfile.RotationSpeed * Time.deltaTime
                );
            }
        }

        private void HandleJump(JumpInputEvent evt)
        {
            if (evt.IsPressed && _controller.CanJump)
            {
                _stateMachine.TransitionTo<SideScrollJumpingState>();
            }
        }

        private void HandleSprint(SprintInputEvent evt)
        {
            if (evt.IsPressed)
            {
                _stateMachine.TransitionTo<SideScrollRunningState>();
            }
        }

        protected override void HandleMove(MoveInputEvent evt)
        {
            base.HandleMove(evt);

            if (Mathf.Abs(evt.Input.x) < ServiceLocator.Get<InputService>().Profile.MovementMagnitudeThreshold)
            {
                _stateMachine.TransitionTo<SideScrollIdleState>();
            }
        }

        public override void Exit()
        {
            base.Exit();
            EventBus.Instance?.Unsubscribe<JumpInputEvent>(this);
            EventBus.Instance?.Unsubscribe<SprintInputEvent>(this);
        }
    }
}
