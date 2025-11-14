using UnityEngine;
using _Project.Code.Gameplay.Input;
using _Project.Code.Core.ServiceLocator;

namespace _Project.Code.Gameplay.PlayerControllers.Base
{
    [RequireComponent(typeof(CharacterController))]
    public class CharacterControllerMotor : MonoBehaviour, IMotor
    {
        private CharacterController _controller;
        private Vector3 _velocity;

        public Vector3 Velocity => _velocity;

        private void Awake()
        {
            _controller = GetComponent<CharacterController>();
        }

        public void Move(Vector3 direction, float speed)
        {
            Vector3 moveVector = direction.normalized * speed * Time.deltaTime;
            _controller.Move(moveVector);

            _velocity.x = direction.normalized.x * speed;
            _velocity.z = direction.normalized.z * speed;
        }

        public void ApplyGravity(float gravity)
        {
            if (_controller.isGrounded && _velocity.y < 0)
            {
                _velocity.y = -ServiceLocator.Get<InputService>().Profile.GroundedVelocityThreshold;
            }
            else
            {
                _velocity.y += gravity * Time.deltaTime;
            }

            _controller.Move(_velocity.y * Time.deltaTime * Vector3.up);
        }

        public void Jump(float jumpForce)
        {
            _velocity.y = jumpForce;
        }

        public void CutJumpVelocity(float multiplier)
        {
            if (_velocity.y > 0)
            {
                _velocity.y *= multiplier;
            }
        }

        public void SetRotation(Quaternion rotation)
        {
            transform.rotation = rotation;
        }

        public void Stop()
        {
            _velocity = Vector3.zero;
        }
    }
}
