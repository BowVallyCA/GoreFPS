using UnityEngine;
using _Project.Code.Core.StateMachine;
using _Project.Code.Core.ServiceLocator;
using _Project.Code.Core.Events;
using _Project.Code.Gameplay.Input;
using _Project.Code.Gameplay.PlayerControllers.ThirdPerson.Utilities;
using _Project.Code.Gameplay.Animation;

namespace _Project.Code.Gameplay.PlayerControllers.ThirdPerson.States
{
    public class TPSprintingState : TPBaseState
    {
        private float _currentSpeed;

        public TPSprintingState(ThirdPersonController controller) : base(controller)
        {
        }

        public override void Enter()
        {
            base.Enter();
            EventBus.Instance.Subscribe<JumpInputEvent>(this, HandleJump);
            EventBus.Instance.Subscribe<SprintInputEvent>(this, HandleSprint);
            EventBus.Instance.Subscribe<LockOnInputEvent>(this, HandleLockOn);
        }

        public override void Update()
        {
            if (!_controller.IsGrounded)
            {
                _stateMachine.TransitionTo<TPFallingState>();
                return;
            }

            ExecuteMovement();
        }

        private void ExecuteMovement()
        {
            var targetSpeed = _controller.MovementProfile.RunSpeed * _controller.MovementProfile.SprintMultiplier;
            _currentSpeed = Mathf.Lerp(_currentSpeed, targetSpeed, _controller.MovementProfile.Acceleration * Time.deltaTime);

            var moveDirection = _controller.GetCameraRelativeMovement();

            if (moveDirection.magnitude > 0)
            {
                _controller.RotateTowardsMovement(moveDirection);
                _controller.Motor.Move(moveDirection, _currentSpeed);
            }

            float normalizedSpeed = _currentSpeed / _controller.MovementProfile.RunSpeed;
            _controller.AnimationController.SetFloat(AnimationParameter.Speed, normalizedSpeed);

            _controller.Motor.ApplyGravity(_controller.MovementProfile.Gravity);
        }

        private void HandleJump(JumpInputEvent evt)
        {
            if (evt.IsPressed && _controller.CanJump)
            {
                _stateMachine.TransitionTo<TPJumpingState>();
            }
        }

        protected override void HandleMove(MoveInputEvent evt)
        {
            base.HandleMove(evt);

            if (evt.Input.magnitude == 0)
            {
                _stateMachine.TransitionTo<TPIdleState>();
            }
        }

        private void HandleSprint(SprintInputEvent evt)
        {
            _controller.IsSprinting = evt.IsPressed;
            if (!_controller.IsSprinting)
            {
                _stateMachine.TransitionTo<TPRunningState>();
            }
        }

        private void HandleLockOn(LockOnInputEvent evt)
        {
            if (evt.IsPressed)
            {
                var target = TargetFinder.FindNearestTarget(
                    _controller.transform,
                    _controller.MovementProfile.TargetSearchRadius,
                    _controller.MovementProfile.TargetAngle,
                    _controller.MovementProfile.TargetLayers
                );

                if (target != null)
                {
                    var lockOnState = _stateMachine.GetState<TPLockOnState>();
                    lockOnState.SetTarget(target);
                    _stateMachine.TransitionTo<TPLockOnState>();
                }
            }
        }

        public override void Exit()
        {
            base.Exit();
            EventBus.Instance?.Unsubscribe<JumpInputEvent>(this);
            EventBus.Instance?.Unsubscribe<SprintInputEvent>(this);
            EventBus.Instance?.Unsubscribe<LockOnInputEvent>(this);
        }
    }
}