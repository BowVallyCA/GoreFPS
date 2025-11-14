using UnityEngine;
using _Project.Code.Core.StateMachine;
using _Project.Code.Core.ServiceLocator;
using _Project.Code.Core.Events;
using _Project.Code.Gameplay.Input;
using _Project.Code.Gameplay.PlayerControllers.ThirdPerson.Utilities;
using _Project.Code.Gameplay.Animation;

namespace _Project.Code.Gameplay.PlayerControllers.ThirdPerson.States
{
    public class TPIdleState : TPBaseState
    {
        public TPIdleState(ThirdPersonController controller) : base(controller)
        {
        }

        public override void Enter()
        {
            base.Enter();
            _controller.Motor.Stop();
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

            // Check for buffered jump after landing
            if (Time.time - _controller.LastJumpInputTime < _controller.MovementProfile.JumpBufferTime &&
                _controller.CanJump)
            {
                _controller.LastJumpInputTime = 0; // Clear buffer
                _stateMachine.TransitionTo<TPJumpingState>();
                return;
            }

            _controller.AnimationController.SetFloat(AnimationParameter.Speed, 0f);

            if (_controller.IsGrounded)
            {
                _controller.Motor.ApplyGravity(_controller.MovementProfile.Gravity);
            }
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

            if (evt.Input.magnitude > ServiceLocator.Get<InputService>().Profile.MovementMagnitudeThreshold)
            {
                if (_controller.IsSprinting)
                {
                    _stateMachine.TransitionTo<TPSprintingState>();
                }
                else
                {
                    _stateMachine.TransitionTo<TPWalkingState>();
                }
            }
        }

        private void HandleSprint(SprintInputEvent evt)
        {
            _controller.IsSprinting = evt.IsPressed;
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