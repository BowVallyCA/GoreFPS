using UnityEngine;
using _Project.Code.Core.ServiceLocator;
using _Project.Code.Gameplay.Input;

namespace _Project.Code.Gameplay.PlayerControllers.Flying.States
{
    public class FlyFlyingState : FlyBaseState
    {
        public FlyFlyingState(FlyingController controller) : base(controller)
        {
        }

        public override void Update()
        {
            // Apply flight physics
            var moveInput = _controller.MoveInput;

            // Pitch and roll based on input
            _controller.ApplyPitchAndRoll(moveInput.y, moveInput.x);

            // Forward movement
            var forwardSpeed = _controller.MovementProfile.WalkSpeed;
            var moveDirection = _controller.transform.forward;
            _controller.Motor.Move(moveDirection, forwardSpeed);

            // Vertical movement (climb/descend) - could use additional input
            // For now, pitch affects vertical movement
            var verticalSpeed = Mathf.Sin(_controller.CurrentPitch * Mathf.Deg2Rad) * _controller.MovementProfile.ClimbRate;
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
            else if (_controller.IsBoosting)
            {
                _stateMachine.TransitionTo<FlyBoostingState>();
            }
        }
    }
}
