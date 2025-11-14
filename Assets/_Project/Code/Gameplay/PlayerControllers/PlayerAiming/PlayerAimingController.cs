using UnityEngine;
using _Project.Code.Gameplay.PlayerControllers.Base;
using _Project.Code.Gameplay.PlayerControllers.Profiles;
using _Project.Code.Core.StateMachine;
using _Project.Code.Core.ServiceLocator;
using _Project.Code.Core.Events;
using _Project.Code.Gameplay.Input;
using _Project.Code.Gameplay.CameraSystems;
using _Project.Code.Gameplay.Player;
using _Project.Code.Gameplay.PlayerControllers.PlayerAiming.States;

namespace _Project.Code.Gameplay.PlayerControllers.PlayerAiming
{
    [RequireComponent(typeof(CharacterControllerMotor), typeof(GroundCheck))]
    public class PlayerAimingController : BasePlayerController
    {
        [field: SerializeField, Header("Movement Settings")]
        public PlayerAimingProfile MovementProfile { get; private set; }


        
        private CharacterControllerMotor _motor;
        private PlayerService _playerService;
        private bool _isSprintPressed;
        private Transform _headTarget;

        public CharacterControllerMotor Motor => _motor;
        public bool IsGrounded => GroundCheck != null && GroundCheck.IsGrounded;
        public bool CanJump => GroundCheck != null && GroundCheck.TimeSinceGrounded < MovementProfile.CoyoteTime;
        public bool IsSprinting => _isSprintPressed;
        public Vector2 MoveInput { get; set; }
        public bool IsJumpHeld { get; set; }
        public float LastJumpInputTime { get; set; }
        public Transform HeadTarget => _headTarget;

        protected override void Awake()
        {
            base.Awake();
            _motor = GetComponent<CharacterControllerMotor>();

            // Create head target for camera to follow
            var headGO = new GameObject("HeadTarget");
            headGO.transform.SetParent(transform);
            headGO.transform.localPosition = new Vector3(0, 1.6f, 0); // Eye height
            _headTarget = headGO.transform;
        }

        protected override void Start()
        {
            base.Start();

            // Register with PlayerService
            _playerService = ServiceLocator.Get<PlayerService>();
            _playerService.RegisterPlayer(this);
        }

        public override void Initialize()
        {
            var idleState = new PAIdleState(this);
            StateMachine = new FiniteStateMachine<IState>(idleState);

            StateMachine.AddState(new PAWalkingState(this));
            StateMachine.AddState(new PARunningState(this));
            StateMachine.AddState(new PAJumpingState(this));
            StateMachine.AddState(new PAFallingState(this));

            EventBus.Instance.Subscribe<JumpInputEvent>(this, HandleJumpInput);
            EventBus.Instance.Subscribe<SprintInputEvent>(this, HandleSprint);
        }

        private void HandleJumpInput(JumpInputEvent evt)
        {
            IsJumpHeld = evt.IsPressed;
            if (evt.IsPressed)
            {
                LastJumpInputTime = Time.time;
            }
        }

        private void HandleSprint(SprintInputEvent evt)
        {
            _isSprintPressed = evt.IsPressed;
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            // Unregister from PlayerService
            _playerService.UnregisterPlayer(this);

            EventBus.Instance?.Unsubscribe<JumpInputEvent>(this);
            EventBus.Instance?.Unsubscribe<SprintInputEvent>(this);
        }
    }
}
