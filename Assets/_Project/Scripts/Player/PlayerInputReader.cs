using UnityEngine;
using UnityEngine.InputSystem;

namespace MarioTest.Player
{
    public sealed class PlayerInputReader : IPlayerInput
    {
        private readonly InputAction _moveAction;
        private readonly InputAction _jumpAction;

        private Vector2 _move;
        private bool _jumpHeld;
        private bool _jumpPressedThisFrame;

        private Vector2 _touchMove;
        private bool _touchMoveActive;
        private bool _touchJumpHeld;
        private bool _touchJumpPressed;

        public PlayerInputReader(InputActionAsset inputActions)
        {
            InputActionMap playerMap = inputActions.FindActionMap("Player");
            _moveAction = playerMap.FindAction("Move");
            _jumpAction = playerMap.FindAction("Jump");
        }

        public Vector2 Move => _move;

        public bool JumpHeld => _jumpHeld;

        public bool JumpPressedThisFrame => _jumpPressedThisFrame;

        public void Enable()
        {
            _moveAction.Enable();
            _jumpAction.Enable();
        }

        public void Disable()
        {
            _moveAction.Disable();
            _jumpAction.Disable();
        }

        public void Tick()
        {
            _move = _touchMoveActive ? _touchMove : _moveAction.ReadValue<Vector2>();
            _jumpHeld = _touchJumpHeld || _jumpAction.IsPressed();
            _jumpPressedThisFrame = _touchJumpPressed || _jumpAction.WasPressedThisFrame();
            _touchJumpPressed = false;
        }

        public void SetTouchMove(Vector2 move, bool active)
        {
            _touchMove = move;
            _touchMoveActive = active;
        }

        public void ClearTouchMove()
        {
            _touchMove = Vector2.zero;
            _touchMoveActive = false;
        }

        public void SetTouchJumpHeld(bool held)
        {
            _touchJumpHeld = held;
        }

        public void SetTouchJumpPressed()
        {
            _touchJumpPressed = true;
        }
    }
}
