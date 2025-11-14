using UnityEngine;
using _Project.Code.Gameplay.PlayerControllers.Base;
using _Project.Code.Gameplay.PlayerControllers.Profiles;
using _Project.Code.Core.StateMachine;
using _Project.Code.Core.ServiceLocator;
using _Project.Code.Core.Events;
using _Project.Code.Gameplay.Input;
using _Project.Code.Gameplay.CameraSystems;
using _Project.Code.Gameplay.Player;
using _Project.Code.Gameplay.PlayerControllers.Tank.States;

namespace _Project.Code.Gameplay.PlayerControllers.Tank
{
    [RequireComponent(typeof(CharacterControllerMotor), typeof(GroundCheck))]
    public class TankController : BasePlayerController
    {
        [field: SerializeField, Header("Tank Settings")]
        public TankMovementProfile MovementProfile { get; private set; }

        private CharacterControllerMotor _motor;
        private PlayerService _playerService;

        public CharacterControllerMotor Motor => _motor;
        public bool IsGrounded => GroundCheck != null && GroundCheck.IsGrounded;
        public Vector2 MoveInput { get; set; }

        // Tank specific properties
        public float ForwardInput => MoveInput.y;
        public float TurnInput => MoveInput.x;
        public bool IsMovingForward => ForwardInput > ServiceLocator.Get<InputService>().Profile.DirectionMagnitudeThreshold;
        public bool IsMovingBackward => ForwardInput < -ServiceLocator.Get<InputService>().Profile.DirectionMagnitudeThreshold;
        public bool IsTurning => Mathf.Abs(TurnInput) > ServiceLocator.Get<InputService>().Profile.DirectionMagnitudeThreshold;

        protected override void Awake()
        {
            base.Awake();
            _motor = GetComponent<CharacterControllerMotor>();
        }

        protected override void Start()
        {
            base.Start();
            
            _playerService = ServiceLocator.Get<PlayerService>();
            _playerService.RegisterPlayer(this);
        }

        public override void Initialize()
        {
            var idleState = new TankIdleState(this);
            StateMachine = new FiniteStateMachine<IState>(idleState);

            StateMachine.AddState(new TankMovingState(this));
            StateMachine.AddState(new TankFallingState(this));
        }

        public void ExecuteTurn()
        {
            var rotationAmount = TurnInput * MovementProfile.TurnSpeed * Time.deltaTime;
            transform.Rotate(Vector3.up, rotationAmount);
        }

        public Vector3 GetForwardDirection()
        {
            return transform.forward;
        }

        public Vector3 GetBackwardDirection()
        {
            return -transform.forward;
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            _playerService?.UnregisterPlayer(this);
        }
    }
}