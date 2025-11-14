using UnityEngine;
using _Project.Code.Core.StateMachine;
using _Project.Code.Gameplay.Input;
using _Project.Code.Gameplay.Animation;

namespace _Project.Code.Gameplay.PlayerControllers.TopDown.States
{
    public class TDMovingState : TDBaseState
    {
        private float _currentSpeed;

        public TDMovingState(TopDownController controller) : base(controller)
        {
        }

        public override void Update()
        {
            if (!_controller.IsGrounded)
            {
                _stateMachine.TransitionTo<TDFallingState>();
                return;
            }

            ExecuteMovement();
            HandleRotation();

            float normalizedSpeed = _currentSpeed / _controller.MovementProfile.WalkSpeed;
            _controller.AnimationController.SetFloat(AnimationParameter.Speed, normalizedSpeed);

            _controller.Motor.ApplyGravity(_controller.MovementProfile.Gravity);
        }

        private void ExecuteMovement()
        {
            var moveDirection = _controller.GetMovementDirection();

            if (moveDirection.magnitude > 0)
            {
                // Normalize diagonal movement if configured
                if (_controller.MovementProfile.NormalizeDiagonalMovement && moveDirection.magnitude > 1f)
                {
                    moveDirection.Normalize();
                }

                // Move
                var targetSpeed = _controller.MovementProfile.WalkSpeed;
                _currentSpeed = Mathf.Lerp(_currentSpeed, targetSpeed, _controller.MovementProfile.Acceleration * Time.deltaTime);
                _controller.Motor.Move(moveDirection, _currentSpeed);
            }
        }

        private void HandleRotation()
        {
            if (_controller.MovementProfile.UseTwinStickAiming)
            {
                // Aim with right stick
                if (_controller.AimInput.magnitude > 0)
                {
                    _controller.RotateTowardsAim();
                }
            }
            else if (_controller.MovementProfile.AutoRotateToMovement)
            {
                // Auto-rotate to movement direction
                var direction = _controller.GetMovementDirection();
                if (direction.magnitude > 0)
                {
                    _controller.RotateTowardsMovement(direction);
                }
            }
        }

        protected override void HandleMove(MoveInputEvent evt)
        {
            base.HandleMove(evt);

            if (!_controller.IsMoving)
            {
                _stateMachine.TransitionTo<TDIdleState>();
            }
        }
    }
}