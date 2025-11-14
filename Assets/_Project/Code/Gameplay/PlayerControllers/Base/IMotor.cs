using UnityEngine;

namespace _Project.Code.Gameplay.PlayerControllers.Base
{
    public interface IMotor
    {
        Vector3 Velocity { get; }

        void Move(Vector3 direction, float speed);
        void ApplyGravity(float gravity);
        void Jump(float jumpForce);
        void CutJumpVelocity(float multiplier);
        void SetRotation(Quaternion rotation);
        void Stop();
    }
}
