using UnityEngine;
using _Project.Code.Gameplay.Input;
using _Project.Code.Core.ServiceLocator;

namespace _Project.Code.Gameplay.PlayerControllers.Base
{
    [RequireComponent(typeof(Rigidbody))]
    public class RigidbodyMotor : MonoBehaviour, IMotor
    {
        private Rigidbody _rigidbody;

        public Vector3 Velocity => _rigidbody.linearVelocity;

        private void Awake()
        {
            _rigidbody = GetComponent<Rigidbody>();
            _rigidbody.freezeRotation = true;
        }

        public void Move(Vector3 direction, float speed)
        {
            if (direction.magnitude > ServiceLocator.Get<InputService>().Profile.MovementMagnitudeThreshold)
            {
                Vector3 targetVelocity = direction.normalized * speed;
                targetVelocity.y = _rigidbody.linearVelocity.y;
                _rigidbody.linearVelocity = targetVelocity;
            }
        }

        public void ApplyGravity(float gravity)
        {
            _rigidbody.AddForce(Vector3.up * gravity * Time.fixedDeltaTime, ForceMode.Acceleration);
        }

        public void Jump(float jumpForce)
        {
            _rigidbody.linearVelocity = new Vector3(_rigidbody.linearVelocity.x, jumpForce, _rigidbody.linearVelocity.z);
        }

        public void CutJumpVelocity(float multiplier)
        {
            if (_rigidbody.linearVelocity.y > 0)
            {
                _rigidbody.linearVelocity = new Vector3(
                    _rigidbody.linearVelocity.x,
                    _rigidbody.linearVelocity.y * multiplier,
                    _rigidbody.linearVelocity.z
                );
            }
        }

        public void SetRotation(Quaternion rotation)
        {
            _rigidbody.MoveRotation(rotation);
        }

        public void Stop()
        {
            _rigidbody.linearVelocity = Vector3.zero;
            _rigidbody.angularVelocity = Vector3.zero;
        }

        public void SetVelocity(Vector3 velocity)
        {
            _rigidbody.linearVelocity = velocity;
        }
    }
}
