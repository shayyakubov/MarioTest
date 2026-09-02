using MarioTest.Player;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.EnhancedTouch;
using Touch = UnityEngine.InputSystem.EnhancedTouch.Touch;
using TouchPhase = UnityEngine.InputSystem.TouchPhase;

namespace MarioTest.Input
{
    [DefaultExecutionOrder(-50)]
    public sealed class MobileTouchInput : MonoBehaviour
    {
        private const int MousePointerId = -2;

        [SerializeField] private TouchInputSettings _settings = new();
        [SerializeField] private RectTransform _joystickRoot;
        [SerializeField] private RectTransform _joystickBackground;
        [SerializeField] private RectTransform _joystickHandle;
        [SerializeField] private Canvas _canvas;

        private static bool _touchSupportInitialized;

        private PlayerInputReader _inputReader;
        private int _joystickFingerId = -1;
        private int _jumpFingerId = -1;
        private Vector2 _joystickCenterScreen;

        public void Initialize(PlayerInputReader inputReader)
        {
            _inputReader = inputReader;
        }

        private void OnEnable()
        {
            EnsureTouchSupportEnabled();
        }

        private void OnDisable()
        {
            _joystickFingerId = -1;
            _jumpFingerId = -1;
            HideJoystick();
        }

        private static void EnsureTouchSupportEnabled()
        {
            if (_touchSupportInitialized)
            {
                return;
            }

            EnhancedTouchSupport.Enable();
            TouchSimulation.Enable();
            _touchSupportInitialized = true;
        }

        private void Update()
        {
            if (_inputReader == null)
            {
                return;
            }

#if UNITY_EDITOR
            if (Mouse.current != null)
            {
                ProcessMousePointer();
                return;
            }
#endif
            ProcessTouches();
        }

        private void ProcessMousePointer()
        {
            Mouse mouse = Mouse.current;
            Vector2 screenPosition = mouse.position.ReadValue();

            if (mouse.leftButton.wasPressedThisFrame)
            {
                TryStartJoystick(MousePointerId, screenPosition);
                TryStartJump(MousePointerId, screenPosition);
            }

            if (_joystickFingerId == MousePointerId)
            {
                if (!mouse.leftButton.isPressed)
                {
                    EndJoystick();
                }
                else
                {
                    UpdateJoystick(screenPosition);
                }
            }

            if (_jumpFingerId == MousePointerId && !mouse.leftButton.isPressed)
            {
                EndJump();
            }
        }

        private void ProcessTouches()
        {
            foreach (Touch touch in Touch.activeTouches)
            {
                int fingerId = touch.finger.index;
                Vector2 screenPosition = touch.screenPosition;

                if (touch.phase == TouchPhase.Began)
                {
                    TryStartJoystick(fingerId, screenPosition);
                    TryStartJump(fingerId, screenPosition);
                }

                if (fingerId == _joystickFingerId)
                {
                    if (touch.phase == TouchPhase.Ended || touch.phase == TouchPhase.Canceled)
                    {
                        EndJoystick();
                    }
                    else
                    {
                        UpdateJoystick(screenPosition);
                    }
                }

                if (fingerId == _jumpFingerId
                    && (touch.phase == TouchPhase.Ended || touch.phase == TouchPhase.Canceled))
                {
                    EndJump();
                }
            }
        }

        private void TryStartJoystick(int fingerId, Vector2 screenPosition)
        {
            if (_joystickFingerId >= 0 || !_settings.IsLeftScreen(screenPosition))
            {
                return;
            }

            _joystickFingerId = fingerId;
            _joystickCenterScreen = screenPosition;
            ShowJoystick(screenPosition);
            UpdateJoystick(screenPosition);
        }

        private void TryStartJump(int fingerId, Vector2 screenPosition)
        {
            if (_jumpFingerId >= 0 || !_settings.IsRightScreen(screenPosition))
            {
                return;
            }

            _jumpFingerId = fingerId;
            _inputReader.SetTouchJumpHeld(true);
            _inputReader.SetTouchJumpPressed();
        }

        private void UpdateJoystick(Vector2 screenPosition)
        {
            Vector2 offset = screenPosition - _joystickCenterScreen;
            float radius = _settings.GetJoystickRadiusPixels();
            Vector2 clamped = Vector2.ClampMagnitude(offset, radius);
            float magnitude = clamped.magnitude / radius;

            if (magnitude < _settings.JoystickDeadzone)
            {
                _inputReader.SetTouchMove(Vector2.zero, true);
            }
            else
            {
                Vector2 direction = clamped.normalized;
                _inputReader.SetTouchMove(direction * magnitude, true);
            }

            UpdateJoystickVisual(clamped);
        }

        private void EndJoystick()
        {
            _joystickFingerId = -1;
            _inputReader.ClearTouchMove();
            HideJoystick();
        }

        private void EndJump()
        {
            _jumpFingerId = -1;
            _inputReader.SetTouchJumpHeld(false);
        }

        private void ShowJoystick(Vector2 screenPosition)
        {
            if (_joystickRoot == null)
            {
                return;
            }

            _joystickRoot.gameObject.SetActive(true);
            SetRectScreenPosition(_joystickBackground, screenPosition);
            SetRectScreenPosition(_joystickHandle, screenPosition);
        }

        private void HideJoystick()
        {
            if (_joystickRoot != null)
            {
                _joystickRoot.gameObject.SetActive(false);
            }
        }

        private void UpdateJoystickVisual(Vector2 clampedOffset)
        {
            if (_joystickBackground == null || _joystickHandle == null)
            {
                return;
            }

            Vector2 handleScreen = _joystickCenterScreen + clampedOffset;
            SetRectScreenPosition(_joystickHandle, handleScreen);
        }

        private void SetRectScreenPosition(RectTransform rectTransform, Vector2 screenPosition)
        {
            if (rectTransform == null || _canvas == null)
            {
                return;
            }

            RectTransform canvasRect = _canvas.transform as RectTransform;
            UnityEngine.Camera canvasCamera = _canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : _canvas.worldCamera;

            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                canvasRect,
                screenPosition,
                canvasCamera,
                out Vector2 localPoint);

            rectTransform.anchoredPosition = localPoint;
        }
    }
}
