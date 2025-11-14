using UnityEngine;
using _Project.Code.Core.StateMachine;
using _Project.Code.Core.Events;
using _Project.Code.Core.ServiceLocator;
using _Project.Code.Gameplay.Input;
using _Project.Code.Gameplay.Animation;

namespace _Project.Code.Gameplay.PlayerControllers.Base
{
    public abstract class BasePlayerController : MonoBehaviour
    {
        protected Animator Animator { get; private set; }

        public FiniteStateMachine<IState> StateMachine { get; protected set; }
        protected GroundCheck GroundCheck { get; private set; }
        public PlayerAnimationController AnimationController { get; private set; }

        protected virtual void Awake()
        {
            if (Animator == null)
            {
                Animator = GetComponentInChildren<Animator>();
            }

            GroundCheck = GetComponent<GroundCheck>();
            AnimationController = GetComponent<PlayerAnimationController>();
            
        }

        protected virtual void Start()
        {
            Initialize();
        }

        protected virtual void Update()
        {
            StateMachine?.Update();
        }

        protected virtual void FixedUpdate()
        {
            StateMachine?.FixedUpdate();
        }

        public abstract void Initialize();

        protected virtual void OnDestroy()
        {
        }

        protected void PublishEvent<T>(T eventData) where T : IEvent
        {
            EventBus.Instance.Publish(eventData);
        }

        protected Vector3 GetCameraRelativeDirection(Vector2 input, Transform cameraTransform)
        {
            if (cameraTransform == null) return Vector3.zero;

            Vector3 forward = cameraTransform.forward;
            Vector3 right = cameraTransform.right;

            forward.y = 0;
            right.y = 0;

            forward.Normalize();
            right.Normalize();

            return forward * input.y + right * input.x;
        }

        protected void RotateTowardsDirection(Vector3 direction, float rotationSpeed)
        {
            if (direction.magnitude > ServiceLocator.Get<InputService>().Profile.RotationThreshold)
            {
                Quaternion targetRotation = Quaternion.LookRotation(direction);
                Quaternion newRotation = Quaternion.RotateTowards(
                    transform.rotation,
                    targetRotation,
                    rotationSpeed * Time.deltaTime
                );

                transform.rotation = newRotation;
            }
        }
    }
}
