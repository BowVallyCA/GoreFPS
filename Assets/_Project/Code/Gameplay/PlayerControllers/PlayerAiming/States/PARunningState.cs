using UnityEngine;
using _Project.Code.Core.StateMachine;
using _Project.Code.Core.ServiceLocator;
using _Project.Code.Core.Events;
using _Project.Code.Gameplay.Input;
using _Project.Code.Gameplay.CameraSystems;
using _Project.Code.Gameplay.Animation;

namespace _Project.Code.Gameplay.PlayerControllers.PlayerAiming.States
{
    public class PARunningState : PABaseState
    {
        private Vector2 _moveInput;
        private float _currentSpeed;

        public PARunningState(PlayerAimingController controller) : base(controller)
        {
        }

        public override void Enter()
        {
            base.Enter();
            EventBus.Instance.Subscribe<JumpInputEvent>(this, HandleJump);
            EventBus.Instance.Subscribe<SprintInputEvent>(this, HandleSprint);
        }

        public override void Update()
        {
            if (!_controller.IsGrounded)
            {
                _stateMachine.TransitionTo<PAFallingState>();
                return;
            }

            ExecuteMovement();
        }

        private void ExecuteMovement()
        {
            float targetSpeed = _controller.MovementProfile.RunSpeed * _controller.MovementProfile.SprintMultiplier;
            _currentSpeed = Mathf.Lerp(_currentSpeed, targetSpeed, _controller.MovementProfile.Acceleration * Time.deltaTime);

            Vector3 moveDirection = _controller.transform.right * _controller.MoveInput.x + _controller.transform.forward * _controller.MoveInput.y;
            _controller.Motor.Move(moveDirection, _currentSpeed);

            float normalizedSpeed = _currentSpeed / _controller.MovementProfile.RunSpeed;
            _controller.AnimationController?.SetFloat(AnimationParameter.Speed, normalizedSpeed);

            _controller.Motor.ApplyGravity(_controller.MovementProfile.Gravity);
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
            _moveInput = evt.Input;

            if (evt.Input.sqrMagnitude == 0)
            {
                _stateMachine.TransitionTo<PAIdleState>();
            }
        }

        private void HandleSprint(SprintInputEvent evt)
        {
            if (!_controller.IsSprinting)
            {
                _stateMachine.TransitionTo<PAWalkingState>();
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
