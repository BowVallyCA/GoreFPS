using System;
using UnityEngine;
using UnityEngine.InputSystem;
using _Project.Code.Core.ServiceLocator;
using _Project.Code.Core.Events;

namespace _Project.Code.Gameplay.Input
{
    public class InputService : MonoBehaviourService
    {
        private PlayerInputActions _inputActions;

        [SerializeField] private InputProfile _profile;

        public InputProfile Profile => _profile;

        public override void Initialize()
        {
            _inputActions = new PlayerInputActions();

            _inputActions.Gameplay.Move.performed += HandleMovePerformed;
            _inputActions.Gameplay.Move.canceled += HandleMoveCanceled;
            _inputActions.Gameplay.Look.performed += HandleLookPerformed;
            _inputActions.Gameplay.Look.canceled += HandleLookCanceled;
            _inputActions.Gameplay.Jump.performed += HandleJumpPerformed;
            _inputActions.Gameplay.Jump.canceled += HandleJumpCanceled;
            _inputActions.Gameplay.Sprint.performed += HandleSprintPerformed;
            _inputActions.Gameplay.Sprint.canceled += HandleSprintCanceled;
            _inputActions.Gameplay.Attack.performed += HandleAttackPerformed;
            _inputActions.Gameplay.Interact.performed += HandleInteractPerformed;
            _inputActions.Gameplay.Dodge.performed += HandleDodgePerformed;
            _inputActions.Gameplay.LockOn.performed += HandleLockOnPerformed;
            _inputActions.Gameplay.LockOn.canceled += HandleLockOnCanceled;

            _inputActions.Gameplay.Enable();
            
        }

        private void HandleMovePerformed(InputAction.CallbackContext context)
        {
            EventBus.Instance.Publish(new MoveInputEvent { Input = context.ReadValue<Vector2>() });
        }

        private void HandleMoveCanceled(InputAction.CallbackContext context)
        {
            EventBus.Instance.Publish(new MoveInputEvent { Input = Vector2.zero });
        }

        private void HandleLookPerformed(InputAction.CallbackContext context)
        {
            EventBus.Instance.Publish(new LookInputEvent { Input = context.ReadValue<Vector2>() });
        }

        private void HandleLookCanceled(InputAction.CallbackContext context)
        {
            EventBus.Instance.Publish(new LookInputEvent { Input = Vector2.zero });
        }

        private void HandleJumpPerformed(InputAction.CallbackContext context)
        {
            EventBus.Instance.Publish(new JumpInputEvent { IsPressed = true });
        }

        private void HandleJumpCanceled(InputAction.CallbackContext context)
        {
            EventBus.Instance.Publish(new JumpInputEvent { IsPressed = false });
        }

        private void HandleSprintPerformed(InputAction.CallbackContext context)
        {
            EventBus.Instance.Publish(new SprintInputEvent { IsPressed = true });
        }

        private void HandleSprintCanceled(InputAction.CallbackContext context)
        {
            EventBus.Instance.Publish(new SprintInputEvent { IsPressed = false });
        }

        private void HandleAttackPerformed(InputAction.CallbackContext context)
        {
            EventBus.Instance.Publish(new AttackInputEvent());
        }

        private void HandleInteractPerformed(InputAction.CallbackContext context)
        {
            EventBus.Instance.Publish(new InteractInputEvent());
        }

        private void HandleDodgePerformed(InputAction.CallbackContext context)
        {
            EventBus.Instance.Publish(new DodgeInputEvent());
        }

        private void HandleLockOnPerformed(InputAction.CallbackContext context)
        {
            EventBus.Instance.Publish(new LockOnInputEvent { IsPressed = true });
        }

        private void HandleLockOnCanceled(InputAction.CallbackContext context)
        {
            EventBus.Instance.Publish(new LockOnInputEvent { IsPressed = false });
        }

        public void EnableGameplayActions()
        {
            _inputActions.Gameplay.Enable();
            _inputActions.UI.Disable();
        }

        public void EnableUIActions()
        {
            _inputActions.Gameplay.Disable();
            _inputActions.UI.Enable();
        }

        public void DisableAllActions()
        {
            _inputActions.Gameplay.Disable();
            _inputActions.UI.Disable();
        }

        public override void Dispose()
        {
            if (_inputActions != null)
            {
                _inputActions.Gameplay.Move.performed -= HandleMovePerformed;
                _inputActions.Gameplay.Move.canceled -= HandleMoveCanceled;
                _inputActions.Gameplay.Look.performed -= HandleLookPerformed;
                _inputActions.Gameplay.Look.canceled -= HandleLookCanceled;
                _inputActions.Gameplay.Jump.performed -= HandleJumpPerformed;
                _inputActions.Gameplay.Jump.canceled -= HandleJumpCanceled;
                _inputActions.Gameplay.Sprint.performed -= HandleSprintPerformed;
                _inputActions.Gameplay.Sprint.canceled -= HandleSprintCanceled;
                _inputActions.Gameplay.Attack.performed -= HandleAttackPerformed;
                _inputActions.Gameplay.Interact.performed -= HandleInteractPerformed;
                _inputActions.Gameplay.Dodge.performed -= HandleDodgePerformed;
                _inputActions.Gameplay.LockOn.performed -= HandleLockOnPerformed;

                _inputActions.Gameplay.Disable();
                _inputActions.Dispose();
            }
        }

        private void OnDestroy()
        {
            Dispose();
        }
    }
}
