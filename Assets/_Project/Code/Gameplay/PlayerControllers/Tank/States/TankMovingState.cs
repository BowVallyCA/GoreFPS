using UnityEngine;
using _Project.Code.Core.StateMachine;
using _Project.Code.Core.Events;
using _Project.Code.Gameplay.Input;
using _Project.Code.Gameplay.Animation;

namespace _Project.Code.Gameplay.PlayerControllers.Tank.States
{
    public class TankMovingState : TankBaseState
    {
        private float _currentSpeed;

        public TankMovingState(TankController controller) : base(controller)
        {
        }

        public override void Enter()
        {
            base.Enter();
        }

        public override void Update()
        {
            if (!_controller.IsGrounded)
            {
                _stateMachine.TransitionTo<TankFallingState>();
                return;
            }

            ExecuteMovement();
            ExecuteTurning();

            float normalizedSpeed = _currentSpeed / _controller.MovementProfile.WalkSpeed;
            _controller.AnimationController.SetFloat(AnimationParameter.Speed, normalizedSpeed);

            _controller.Motor.ApplyGravity(_controller.MovementProfile.Gravity);
        }

        private void ExecuteMovement()
        {
            // Determine direction and speed based on forward/backward input
            if (_controller.IsMovingForward)
            {
                // Move forward
                var targetSpeed = _controller.MovementProfile.WalkSpeed;
                _currentSpeed = Mathf.Lerp(_currentSpeed, targetSpeed, _controller.MovementProfile.Acceleration * Time.deltaTime);

                var moveDirection = _controller.GetForwardDirection();
                _controller.Motor.Move(moveDirection, _currentSpeed);
            }
            else if (_controller.IsMovingBackward)
            {
                // Move backward at reduced speed
                var targetSpeed = _controller.MovementProfile.WalkSpeed * _controller.MovementProfile.BackwardSpeedMultiplier;
                _currentSpeed = Mathf.Lerp(_currentSpeed, targetSpeed, _controller.MovementProfile.Acceleration * Time.deltaTime);

                var moveDirection = _controller.GetBackwardDirection();
                _controller.Motor.Move(moveDirection, _currentSpeed);
            }
            else
            {
                // No forward/backward input, but might be turning in place
                _currentSpeed = 0f;
            }
        }

        private void ExecuteTurning()
        {
            // Always allow turning when there's turn input
            if (_controller.IsTurning)
            {
                // If moving and profile doesn't allow turning while moving, apply speed reduction
                if ((_controller.IsMovingForward || _controller.IsMovingBackward) &&
                    !_controller.MovementProfile.CanTurnWhileMoving)
                {
                    // Don't turn while moving if not allowed
                    return;
                }

                _controller.ExecuteTurn();
            }
        }

        protected override void HandleMove(MoveInputEvent evt)
        {
            base.HandleMove(evt);

            if (!_controller.IsMovingForward && !_controller.IsMovingBackward && !_controller.IsTurning)
            {
                _stateMachine.TransitionTo<TankIdleState>();
            }
        }

        public override void Exit()
        {
            base.Exit();
        }
    }
}
