using UnityEngine;
using DG.Tweening;
using _Project.Code.Gameplay.Input;
using _Project.Code.Core.ServiceLocator;

namespace _Project.Code.Gameplay.PlayerControllers.Base
{
    public class KinematicMotor : MonoBehaviour, IMotor
    {
        private Vector3 _velocity;
        private bool _isMoving;
        private Tween _currentMoveTween;
        private Tween _currentRotateTween;

        [field: SerializeField] public float GridSize { get; private set; } = 1f;
        [field: SerializeField] public float StepDuration { get; private set; } = 0.3f;
        [field: SerializeField] public Ease MoveEase { get; private set; } = Ease.InOutQuad;
        [field: SerializeField] public Ease RotateEase { get; private set; } = Ease.InOutQuad;

        public Vector3 Velocity => _velocity;
        public bool IsMoving => _isMoving;

        private void OnDestroy()
        {
            _currentMoveTween?.Kill();
            _currentRotateTween?.Kill();
        }

        public void Move(Vector3 direction, float speed)
        {
            if (!_isMoving && direction.magnitude > ServiceLocator.Get<InputService>().Profile.MovementMagnitudeThreshold)
            {
                Vector3 targetPosition = transform.position + SnapToGrid(direction);
                MoveToPosition(targetPosition, StepDuration);
            }
        }

        public void MoveToPosition(Vector3 targetPosition, float duration)
        {
            if (_isMoving) return;

            _isMoving = true;
            Vector3 startPosition = transform.position;

            _currentMoveTween = transform.DOMove(targetPosition, duration)
                .SetEase(MoveEase)
                .OnUpdate(() =>
                {
                    _velocity = (transform.position - startPosition) / Time.deltaTime;
                    startPosition = transform.position;
                })
                .OnComplete(() =>
                {
                    _isMoving = false;
                    _velocity = Vector3.zero;
                });
        }

        public void MoveToGridPosition(Vector3 direction)
        {
            if (!_isMoving)
            {
                Vector3 snappedDirection = SnapToGrid(direction);
                Vector3 targetPosition = transform.position + snappedDirection;
                MoveToPosition(targetPosition, StepDuration);
            }
        }

        public void RotateToDirection(Vector3 direction, float duration)
        {
            if (direction.magnitude > ServiceLocator.Get<InputService>().Profile.DirectionMagnitudeThreshold)
            {
                Quaternion targetRotation = Quaternion.LookRotation(direction);
                RotateToRotation(targetRotation, duration);
            }
        }

        public void RotateToRotation(Quaternion targetRotation, float duration)
        {
            _currentRotateTween?.Kill();
            _currentRotateTween = transform.DORotateQuaternion(targetRotation, duration)
                .SetEase(RotateEase);
        }

        public void ApplyGravity(float gravity)
        {
        }

        public void Jump(float jumpForce)
        {
        }

        public void CutJumpVelocity(float multiplier)
        {
            // Grid-based movement doesn't support jumping
        }

        public void SetRotation(Quaternion rotation)
        {
            _currentRotateTween?.Kill();
            transform.rotation = rotation;
        }

        public void Stop()
        {
            _currentMoveTween?.Kill();
            _currentRotateTween?.Kill();
            _isMoving = false;
            _velocity = Vector3.zero;
        }

        private Vector3 SnapToGrid(Vector3 direction)
        {
            Vector3 snapped = Vector3.zero;

            if (Mathf.Abs(direction.x) > Mathf.Abs(direction.z))
            {
                snapped.x = Mathf.Sign(direction.x) * GridSize;
            }
            else if (Mathf.Abs(direction.z) > ServiceLocator.Get<InputService>().Profile.DirectionMagnitudeThreshold)
            {
                snapped.z = Mathf.Sign(direction.z) * GridSize;
            }

            return snapped;
        }

        public void SetGridSize(float gridSize)
        {
            GridSize = gridSize;
        }

        public void SetStepDuration(float duration)
        {
            StepDuration = duration;
        }
    }
}
