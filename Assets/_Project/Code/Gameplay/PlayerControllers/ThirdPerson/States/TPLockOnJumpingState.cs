using UnityEngine;
using _Project.Code.Core.StateMachine;
using _Project.Code.Core.ServiceLocator;
using _Project.Code.Core.Events;
using _Project.Code.Gameplay.Input;
using _Project.Code.Gameplay.CameraSystems;
using _Project.Code.Gameplay.Animation;

namespace _Project.Code.Gameplay.PlayerControllers.ThirdPerson.States
{
    public class TPLockOnJumpingState : TPBaseState
    {
        private Transform _target;
        private float _jumpTimeRemaining;

        public TPLockOnJumpingState(ThirdPersonController controller) : base(controller)
        {
        }

        public void SetTarget(Transform target)
        {
            _target = target;
        }

        public override void Enter()
        {
            base.Enter();

            _controller.AnimationController?.TriggerAnimation(AnimationTrigger.Jump);
            _controller.AnimationController?.SetLayerWeight(1, 1f);

            var jumpVelocity = _controller.MovementProfile.CalculateJumpForce();
            _controller.Motor.Jump(jumpVelocity);
            _jumpTimeRemaining = 0.5f;

            EventBus.Instance.Subscribe<LockOnInputEvent>(this, HandleLockOn);
        }

        public override void Update()
        {
            _jumpTimeRemaining -= Time.deltaTime;

            // Handle variable jump height
            if (_controller.MovementProfile.VariableJumpHeight)
            {
                if (!_controller.IsJumpHeld && _controller.Motor.Velocity.y > 0)
                {
                    float minJumpVelocity = _controller.MovementProfile.CalculateJumpForce() *
                                          _controller.MovementProfile.MinJumpMultiplier;

                    if (_controller.Motor.Velocity.y > minJumpVelocity)
                    {
                        _controller.Motor.CutJumpVelocity(0.4f);
                    }
                }
            }

            var inputMagnitude = _controller.MoveInput.magnitude;
            var threshold = ServiceLocator.Get<InputService>().Profile.MovementMagnitudeThreshold;

            if (inputMagnitude > threshold)
            {
                HandleAirMovement();
            }

            FaceTarget();

            _controller.AnimationController?.SetFloat(AnimationParameter.StrafeX, _controller.MoveInput.x);
            _controller.AnimationController?.SetFloat(AnimationParameter.StrafeY, _controller.MoveInput.y);

            _controller.Motor.ApplyGravity(_controller.MovementProfile.Gravity);

            if (_controller.IsGrounded && _jumpTimeRemaining <= 0)
            {
                var lockOnState = _stateMachine.GetState<TPLockOnState>();
                if (lockOnState != null)
                {
                    lockOnState.SetTarget(_target);
                    _stateMachine.TransitionTo<TPLockOnState>();
                }
            }
        }

        private void FaceTarget()
        {
            Vector3 directionToTarget = _target.position - _controller.transform.position;
            directionToTarget.y = 0;

            if (directionToTarget.magnitude > 0.01f)
            {
                Quaternion targetRotation = Quaternion.LookRotation(directionToTarget);
                _controller.transform.rotation = Quaternion.RotateTowards(
                    _controller.transform.rotation,
                    targetRotation,
                    _controller.MovementProfile.LockOnRotationSpeed * Time.deltaTime
                );
            }
        }

        private void HandleAirMovement()
        {
            var cameraTransform = ServiceLocator.Get<CameraService>().GetCameraTransform();

            Vector3 forward = cameraTransform.forward;
            Vector3 right = cameraTransform.right;
            forward.y = 0;
            right.y = 0;
            forward.Normalize();
            right.Normalize();

            Vector3 movement = forward * _controller.MoveInput.y + right * _controller.MoveInput.x;
            movement.Normalize();

            float speed = _controller.MovementProfile.StrafeAirSpeed;
            _controller.Motor.Move(movement, speed);
        }

        private void HandleLockOn(LockOnInputEvent evt)
        {
            if (!evt.IsPressed)
            {
                _stateMachine.TransitionTo<TPFallingState>();
            }
        }

        public override void Exit()
        {
            base.Exit();

            _controller.AnimationController?.SetLayerWeight(1, 0f);

            EventBus.Instance?.Unsubscribe<LockOnInputEvent>(this);

            _target = null;
        }
    }
}