using UnityEngine;
using _Project.Code.Gameplay.PlayerControllers.Base;
using _Project.Code.Gameplay.PlayerControllers.Profiles;
using _Project.Code.Core.StateMachine;
using _Project.Code.Core.Events;
using _Project.Code.Core.ServiceLocator;
using _Project.Code.Gameplay.Input;
using _Project.Code.Gameplay.CameraSystems;
using _Project.Code.Gameplay.Player;
using _Project.Code.Gameplay.PlayerControllers.SideScroller.States;

namespace _Project.Code.Gameplay.PlayerControllers.SideScroller
{
    [RequireComponent(typeof(RigidbodyMotor), typeof(GroundCheck))]
    public class SideScrollerController : BasePlayerController
    {
        [field: SerializeField, Header("SideScroller Settings")]
        public JumpingMovementProfile MovementProfile { get; private set; }

        private RigidbodyMotor _motor;
        private PlayerService _playerService;
        private bool _isSprintPressed;
        private int _airJumpsRemaining;
        private float _lastDashTime;
        private bool _hasAirDashed;

        public RigidbodyMotor Motor => _motor;
        public bool IsGrounded => GroundCheck != null && GroundCheck.IsGrounded;
        public bool CanJump => GroundCheck != null && GroundCheck.TimeSinceGrounded < MovementProfile.CoyoteTime;
        public bool IsSprinting => _isSprintPressed;
        public Vector2 MoveInput { get; set; }
        public bool IsJumpHeld { get; set; }
        public float LastJumpInputTime { get; set; }

        // Advanced mechanics tracking
        public int AirJumpsRemaining { get => _airJumpsRemaining; set => _airJumpsRemaining = value; }
        public bool CanAirDash => MovementProfile.EnableAirDash &&
                                   (!MovementProfile.AirDashOncePerJump || !_hasAirDashed) &&
                                   Time.time >= _lastDashTime + MovementProfile.AirDashCooldown;
        public void ConsumeAirDash()
        {
            _hasAirDashed = true;
            _lastDashTime = Time.time;
        }

        public void ResetAirMechanics()
        {
            _airJumpsRemaining = MovementProfile.MaxAirJumps;
            _hasAirDashed = false;
        }

        public bool CheckWall(out Vector3 wallNormal)
        {
            wallNormal = Vector3.zero;
            if (!MovementProfile.EnableWallMechanics) return false;

            // Check both left and right for walls
            float direction = Mathf.Sign(transform.rotation.eulerAngles.y == 90f ? 1f : -1f);
            Vector3 rayOrigin = transform.position;
            Vector3 rayDirection = new Vector3(direction, 0f, 0f);

            if (Physics.Raycast(rayOrigin, rayDirection, out RaycastHit hit,
                MovementProfile.WallCheckDistance, MovementProfile.WallLayers))
            {
                wallNormal = hit.normal;
                return true;
            }

            return false;
        }

        protected override void Awake()
        {
            base.Awake();
            _motor = GetComponent<RigidbodyMotor>();
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
            var idleState = new SideScrollIdleState(this);
            StateMachine = new FiniteStateMachine<IState>(idleState);

            StateMachine.AddState(new SideScrollWalkingState(this));
            StateMachine.AddState(new SideScrollRunningState(this));
            StateMachine.AddState(new SideScrollJumpingState(this));
            StateMachine.AddState(new SideScrollFallingState(this));
            StateMachine.AddState(new SideScrollWallSlideState(this));
            StateMachine.AddState(new SideScrollAirDashState(this));

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

            _playerService?.UnregisterPlayer(this);
            EventBus.Instance?.Unsubscribe<JumpInputEvent>(this);
            EventBus.Instance?.Unsubscribe<SprintInputEvent>(this);
        }
    }
}
