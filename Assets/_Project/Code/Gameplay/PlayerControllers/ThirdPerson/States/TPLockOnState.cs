using UnityEngine;
using _Project.Code.Core.StateMachine;
using _Project.Code.Core.ServiceLocator;
using _Project.Code.Core.Events;
using _Project.Code.Gameplay.Input;
using _Project.Code.Gameplay.CameraSystems;
using _Project.Code.Gameplay.Animation;

namespace _Project.Code.Gameplay.PlayerControllers.ThirdPerson.States
{
    public class TPLockOnState : TPBaseState
    {
        private CameraService _cameraService;
        private Transform _target;

        public TPLockOnState(ThirdPersonController controller) : base(controller)
        {
            _cameraService = ServiceLocator.Get<CameraService>();
        }

        public void SetTarget(Transform target)
        {
            _target = target;
        }

        public override void Enter()
        {
            base.Enter();
            UpdateCamera();

            _controller.AnimationController?.SetLayerWeight(1, 1f);

            EventBus.Instance.Subscribe<JumpInputEvent>(this, HandleJump);
            EventBus.Instance.Subscribe<SprintInputEvent>(this, HandleSprint);
            EventBus.Instance.Subscribe<LockOnInputEvent>(this, HandleLockOn);
        }

        public override void Update()
        {
            if (!_controller.IsGrounded)
            {
                _controller.Motor.ApplyGravity(_controller.MovementProfile.Gravity);
            }

            RotateToFaceTarget();

            _controller.AnimationController?.SetFloat(AnimationParameter.StrafeX, _controller.MoveInput.x);
            _controller.AnimationController?.SetFloat(AnimationParameter.StrafeY, _controller.MoveInput.y);

            if (_controller.MoveInput.magnitude > ServiceLocator.Get<InputService>().Profile.MovementMagnitudeThreshold)
            {
                Strafe();
            }
        }

        private void RotateToFaceTarget()
        {
            Vector3 direction = _target.position - _controller.transform.position;
            direction.y = 0;

            if (direction != Vector3.zero)
            {
                Quaternion targetRotation = Quaternion.LookRotation(direction);
                _controller.transform.rotation = Quaternion.RotateTowards(
                    _controller.transform.rotation,
                    targetRotation,
                    _controller.MovementProfile.LockOnRotationSpeed * Time.deltaTime
                );
            }
        }

        private void Strafe()
        {
            var cameraTransform = _cameraService.GetCameraTransform();

            Vector3 forward = cameraTransform.forward;
            Vector3 right = cameraTransform.right;
            forward.y = 0;
            right.y = 0;
            forward.Normalize();
            right.Normalize();

            Vector3 movement = forward * _controller.MoveInput.y + right * _controller.MoveInput.x;
            movement.Normalize();

            float speed = GetStrafeSpeed();
            _controller.Motor.Move(movement, speed);
        }

        private float GetStrafeSpeed()
        {
            if (_controller.IsSprinting)
            {
                return _controller.MovementProfile.StrafeSprintSpeed;
            }

            // Could add running detection here if needed
            return _controller.MovementProfile.StrafeWalkSpeed;
        }

        private void UpdateCamera()
        {
            var virtualCamera = _cameraService.ActiveVirtualCamera;
            var thirdPersonCamera = virtualCamera.GetComponent<ThirdPersonCamera>();
            thirdPersonCamera.SetLockOnTarget(_target);
        }

        private void ClearCamera()
        {
            var virtualCamera = _cameraService.ActiveVirtualCamera;
            var thirdPersonCamera = virtualCamera.GetComponent<ThirdPersonCamera>();
            thirdPersonCamera.ClearLockOnTarget();
        }

        private void HandleJump(JumpInputEvent evt)
        {
            if (evt.IsPressed && _controller.CanJump)
            {
                var jumpState = _stateMachine.GetState<TPLockOnJumpingState>();
                jumpState.SetTarget(_target);
                _stateMachine.TransitionTo<TPLockOnJumpingState>();
            }
        }

        private void HandleSprint(SprintInputEvent evt)
        {
            _controller.IsSprinting = evt.IsPressed;
        }

        private void HandleLockOn(LockOnInputEvent evt)
        {
            if (!evt.IsPressed)
            {
                _stateMachine.TransitionTo<TPIdleState>();
            }
        }

        public override void Exit()
        {
            base.Exit();
            ClearCamera();

            _controller.AnimationController?.SetLayerWeight(1, 0f);

            EventBus.Instance?.Unsubscribe<JumpInputEvent>(this);
            EventBus.Instance?.Unsubscribe<SprintInputEvent>(this);
            EventBus.Instance?.Unsubscribe<LockOnInputEvent>(this);
        }
    }
}