using UnityEngine;
using _Project.Code.Core.ServiceLocator;
using _Project.Code.Gameplay.Input;

namespace _Project.Code.Gameplay.PlayerControllers.Grid.States
{
    public class GridMovingState : GridBaseState
    {
        private Vector3 _moveDirection;
        private bool _hasProcessedInput;

        public GridMovingState(GridController controller) : base(controller)
        {
        }

        public override void Enter()
        {
            base.Enter();
            _hasProcessedInput = false;
        }

        public override void Update()
        {
            // Process input once when entering or if movement completes
            if (!_hasProcessedInput)
            {
                ProcessMovementInput();
                _hasProcessedInput = true;
            }

            // Check if movement is complete
            if (!_controller.IsMoving)
            {
                // Check if there's still input
                if (_controller.MoveInput.magnitude > ServiceLocator.Get<InputService>().Profile.MovementMagnitudeThreshold)
                {
                    // Continue moving with new input
                    _hasProcessedInput = false;
                }
                else
                {
                    _stateMachine.TransitionTo<GridIdleState>();
                }
            }
        }

        private void ProcessMovementInput()
        {
            _moveDirection = _controller.GetGridDirection();

            if (_moveDirection.sqrMagnitude > 0.01f)
            {
                var targetPosition = _controller.CurrentGridPosition + _moveDirection * _controller.MovementProfile.GridSize;

                // Check if we can move to the target position
                if (_controller.CanMoveToPosition(targetPosition))
                {
                    // Rotate to face direction if not strafing
                    if (!_controller.MovementProfile.AllowStrafe)
                    {
                        _controller.RotateToDirection(_moveDirection);
                    }

                    // Move to target position
                    _controller.MoveToGridPosition(targetPosition);
                }
                else
                {
                    // Can't move, return to idle
                    _stateMachine.TransitionTo<GridIdleState>();
                }
            }
        }

        protected override void HandleMove(MoveInputEvent evt)
        {
            // Input is handled in Update to respect grid timing
        }
    }
}
