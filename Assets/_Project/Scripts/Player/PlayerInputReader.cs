using UnityEngine;
using UnityEngine.InputSystem;

namespace MarioTest.Player
{
    public sealed class PlayerInputReader : IPlayerInput
    {
        private readonly InputAction _moveAction;
        private readonly InputAction _jumpAction;
        private Vector2 _touchMove;
        private bool _hasTouchMove;

        public PlayerInputReader(InputActionAsset inputActions)
        {
            InputActionMap playerMap = inputActions.FindActionMap("Player");
            _moveAction = playerMap.FindAction("Move");
            _jumpAction = playerMap.FindAction("Jump");
        }

        public Vector2 Move
        {
            get
            {
                if (_hasTouchMove)
                {
                    return _touchMove;
                }

                return _moveAction.ReadValue<Vector2>();
            }
        }

        public bool IsJumpPressed => _jumpAction.IsPressed();

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

        public void SetTouchMove(Vector2 move)
        {
            _touchMove = move;
            _hasTouchMove = move.sqrMagnitude > 0f;
        }

        public void ClearTouchMove()
        {
            _touchMove = Vector2.zero;
            _hasTouchMove = false;
        }
    }
}
