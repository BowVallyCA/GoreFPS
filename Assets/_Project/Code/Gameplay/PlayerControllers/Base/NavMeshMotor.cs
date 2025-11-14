using UnityEngine;
using UnityEngine.AI;

namespace _Project.Code.Gameplay.PlayerControllers.Base
{
    [RequireComponent(typeof(NavMeshAgent))]
    public class NavMeshMotor : MonoBehaviour, IMotor
    {
        private NavMeshAgent _agent;

        [field: SerializeField] public float ArrivalDistance { get; private set; } = 0.5f;

        public Vector3 Velocity => _agent.velocity;
        public bool HasReachedDestination => !_agent.pathPending && _agent.remainingDistance <= ArrivalDistance;

        private void Awake()
        {
            _agent = GetComponent<NavMeshAgent>();
        }

        public void Move(Vector3 direction, float speed)
        {
            _agent.speed = speed;

            if (direction != Vector3.zero)
            {
                SetDestination(transform.position + direction);
            }
        }

        public void SetDestination(Vector3 destination)
        {
            if (_agent.isOnNavMesh)
            {
                _agent.SetDestination(destination);
            }
        }

        public void ApplyGravity(float gravity)
        {
        }

        public void Jump(float jumpForce)
        {
        }

        public void CutJumpVelocity(float multiplier)
        {
            // NavMesh agents don't support jumping
        }

        public void SetRotation(Quaternion rotation)
        {
            transform.rotation = rotation;
        }

        public void Stop()
        {
            if (_agent.isOnNavMesh)
            {
                _agent.ResetPath();
                _agent.velocity = Vector3.zero;
            }
        }

        public void SetSpeed(float speed)
        {
            _agent.speed = speed;
        }

        public void SetStoppingDistance(float distance)
        {
            _agent.stoppingDistance = distance;
        }
    }
}
