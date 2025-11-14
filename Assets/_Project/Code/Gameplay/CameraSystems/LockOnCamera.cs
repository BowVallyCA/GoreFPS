using _Project.Code.Gameplay.CameraSystems.Profiles;
using UnityEngine;
using Unity.Cinemachine;

namespace _Project.Code.Gameplay.CameraSystems
{
    public class LockOnCamera : MonoBehaviour
    {
        [SerializeField] private CinemachineCamera _vcam;
        [SerializeField] private LockOnCameraProfile _profile;
        [SerializeField] private Transform _followTarget;

        private Transform _lockTarget;
        private Vector3 _currentPosition;
        private Quaternion _currentRotation;

        private void Awake()
        {
            if (_followTarget == null)
            {
                _followTarget = transform;
            }
        }

        private void Start()
        {
            if (_vcam != null && _profile != null)
            {
                var lens = LensSettings.Default;
                lens.FieldOfView = _profile.FieldOfView;
                _vcam.Lens = lens;
                _vcam.Priority = _profile.Priority;
            }
        }

        public void SetTarget(Transform target)
        {
            _lockTarget = target;
        }

        private void LateUpdate()
        {
            if (_profile == null || _followTarget == null || _vcam == null) return;

            if (_lockTarget != null)
            {
                // Locked camera - frame both player and target
                UpdateLockedCamera();
            }
            else
            {
                // Free camera - standard third person
                UpdateFreeCamera();
            }
        }

        private void UpdateLockedCamera()
        {
            // Calculate midpoint between player and target
            var midpoint = (_followTarget.position + _lockTarget.position) * 0.5f;

            // Offset midpoint based on profile
            var targetPosition = midpoint + _profile.LockedOffset;

            // Calculate look direction
            var lookDirection = midpoint - targetPosition;
            lookDirection.Normalize();

            // Add some height offset to look slightly down
            var lookTarget = midpoint + Vector3.up * _profile.LookHeightOffset;
            var targetRotation = Quaternion.LookRotation(lookTarget - targetPosition);

            // Smooth transitions
            _currentPosition = Vector3.Lerp(_currentPosition, targetPosition, _profile.FollowSmoothing * Time.deltaTime);
            _currentRotation = Quaternion.Slerp(_currentRotation, targetRotation, _profile.RotationSmoothing * Time.deltaTime);

            // Apply to camera
            _vcam.transform.position = _currentPosition;
            _vcam.transform.rotation = _currentRotation;

            // Adjust FOV based on distance between player and target
            var distance = Vector3.Distance(_followTarget.position, _lockTarget.position);
            var fovMultiplier = Mathf.Lerp(1f, _profile.MaxFOVMultiplier, distance / _profile.MaxTargetDistance);

            var lens = _vcam.Lens;
            lens.FieldOfView = _profile.FieldOfView * fovMultiplier;
            _vcam.Lens = lens;
        }

        private void UpdateFreeCamera()
        {
            // Standard third person camera
            var targetPosition = _followTarget.position + _profile.FreeOffset;
            var lookTarget = _followTarget.position + Vector3.up * _profile.LookHeightOffset;
            var targetRotation = Quaternion.LookRotation(lookTarget - targetPosition);

            _currentPosition = Vector3.Lerp(_currentPosition, targetPosition, _profile.FollowSmoothing * Time.deltaTime);
            _currentRotation = Quaternion.Slerp(_currentRotation, targetRotation, _profile.RotationSmoothing * Time.deltaTime);

            _vcam.transform.position = _currentPosition;
            _vcam.transform.rotation = _currentRotation;
        }
    }
}