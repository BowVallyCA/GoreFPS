using UnityEngine;
using _Project.Code.Core.ServiceLocator;
using _Project.Code.Gameplay.Input;

namespace _Project.Code.Gameplay.PlayerControllers.Flying.States
{
    public class FlyBoostingState : FlyBaseState
    {
        public FlyBoostingState(FlyingController controller) : base(controller)
        {
        }

        public override void Update()
        {
            // Apply flight physics with boost
            var moveInput = _controller.MoveInput;

            // Pitch and roll based on input
            _controller.ApplyPitchAndRoll(moveInput.y, moveInput.x);

            // Forward movement with boost
            var boostSpeed = _controller.MovementProfile.RunSpeed; // Use run speed as boost speed
            var moveDirection = _controller.transform.forward;
            _controller.Motor.Move(moveDirection, boostSpeed);

            // Vertical movement (climb/descend)
            var verticalSpeed = Mathf.Sin(_controller.CurrentPitch * Mathf.Deg2Rad) * _controller.MovementProfile.ClimbRate * 1.5f; // Faster climb when boosting
            var currentVelocity = _controller.Motor.Velocity;
            currentVelocity.y = verticalSpeed;
            _controller.Motor.SetVelocity(currentVelocity);

            // Apply altitude limits
            _controller.ApplyAltitudeLimits();
        }

        protected override void HandleMove(MoveInputEvent evt)
        {
            base.HandleMove(evt);

            if (evt.Input.magnitude < ServiceLocator.Get<InputService>().Profile.MovementMagnitudeThreshold)
            {
                _stateMachine.TransitionTo<FlyIdleState>();
            }
            else if (!_controller.IsBoosting)
            {
                _stateMachine.TransitionTo<FlyFlyingState>();
            }
        }
    }
}
