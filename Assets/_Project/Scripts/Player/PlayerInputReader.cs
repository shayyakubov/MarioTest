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
        private bool _inputEnabled = true;

        private Vector2 _touchMove;
        private bool _touchMoveActive;
        private bool _touchJumpHeld;
        private bool _touchJumpPressed;

        public PlayerInputReader(InputActionAsset inputActions)
        {
            InputActionMap playerMap = inputActions.FindActionMap("Player");
            _moveAction = playerMap.FindAction("Move");
            _jumpAction = playerMap.FindAction("Jump");
            _moveAction.Enable();
            _jumpAction.Enable();
        }

        public Vector2 Move => _move;

        public bool JumpHeld => _jumpHeld;

        public bool JumpPressedThisFrame => _jumpPressedThisFrame;

        public void Enable()
        {
            _inputEnabled = true;
        }

        public void Disable()
        {
            _inputEnabled = false;
            ClearBufferedInput();
        }

        public void Tick()
        {
            if (!_inputEnabled)
            {
                ClearBufferedInput();
                return;
            }

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

        private void ClearBufferedInput()
        {
            _move = Vector2.zero;
            _jumpHeld = false;
            _jumpPressedThisFrame = false;
            _touchMove = Vector2.zero;
            _touchMoveActive = false;
            _touchJumpHeld = false;
            _touchJumpPressed = false;
        }
    }
}
