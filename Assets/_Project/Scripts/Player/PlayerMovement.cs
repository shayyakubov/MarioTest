using UnityEngine;

namespace MarioTest.Player
{
    public sealed class PlayerMovement
    {
        private readonly PlayerTuning _tuning;
        private readonly PlayerMovementSettings _movementSettings;
        private Vector3 _moveDirection;
        private float _inputMagnitude;
        private bool _jumpHeld;
        private bool _jumpPressedLatched;
        private float _coyoteTimeRemaining;
        private float _jumpBufferRemaining;
        private bool _wasGrounded;

        public PlayerMovement(PlayerTuning tuning, PlayerMovementSettings movementSettings)
        {
            _tuning = tuning;
            _movementSettings = movementSettings;
        }

        public void SetMoveInput(Vector3 normalizedDirection, float magnitude)
        {
            _moveDirection = normalizedDirection;
            _inputMagnitude = magnitude;
        }

        public void SetJumpInput(bool pressedThisFrame, bool held)
        {
            _jumpHeld = held;

            if (pressedThisFrame)
            {
                _jumpPressedLatched = true;
                _jumpBufferRemaining = _tuning.JumpBuffer;
            }
        }

        public void ApplyMovement(Rigidbody rigidbody, float fixedDeltaTime, bool isGrounded, int groundLayer)
        {
            float accelerationMultiplier = _movementSettings.GetAccelerationMultiplier(isGrounded, groundLayer);

            Vector3 velocity = rigidbody.linearVelocity;
            float verticalVelocity = TryExecuteJump(velocity.y, isGrounded, fixedDeltaTime);
            velocity.y = verticalVelocity;
            rigidbody.linearVelocity = velocity;

            ApplyGravity(rigidbody, fixedDeltaTime, isGrounded);
            ApplyHorizontalMovement(rigidbody, fixedDeltaTime, accelerationMultiplier);

            _jumpPressedLatched = false;
            _wasGrounded = isGrounded;
        }

        private float TryExecuteJump(float verticalVelocity, bool isGrounded, float fixedDeltaTime)
        {
            UpdateJumpTimers(verticalVelocity, isGrounded, fixedDeltaTime);

            bool jumpRequested = _jumpPressedLatched || _jumpBufferRemaining > 0f;
            if (!jumpRequested)
            {
                return verticalVelocity;
            }

            bool canJump = isGrounded || (_coyoteTimeRemaining > 0f && verticalVelocity <= 0f);
            if (!canJump)
            {
                return verticalVelocity;
            }

            _coyoteTimeRemaining = 0f;
            _jumpBufferRemaining = 0f;
            return _tuning.JumpVelocity;
        }

        private void UpdateJumpTimers(float verticalVelocity, bool isGrounded, float fixedDeltaTime)
        {
            if (_wasGrounded && !isGrounded && verticalVelocity <= 0f)
            {
                _coyoteTimeRemaining = _tuning.CoyoteTime;
            }

            _coyoteTimeRemaining = Mathf.Max(_coyoteTimeRemaining - fixedDeltaTime, 0f);
            _jumpBufferRemaining = Mathf.Max(_jumpBufferRemaining - fixedDeltaTime, 0f);
        }

        private void ApplyGravity(Rigidbody rigidbody, float fixedDeltaTime, bool isGrounded)
        {
            Vector3 velocity = rigidbody.linearVelocity;

            if (isGrounded && velocity.y <= 0f)
            {
                velocity.y = 0f;
                rigidbody.linearVelocity = velocity;
                return;
            }

            float gravity;
            if (velocity.y > 0f)
            {
                gravity = _jumpHeld ? _tuning.RiseGravity : _tuning.LowJumpGravity;
            }
            else
            {
                gravity = _tuning.FallGravity;
            }

            velocity.y += gravity * fixedDeltaTime;
            velocity.y = Mathf.Max(velocity.y, -_tuning.MaxFallSpeed);

            rigidbody.linearVelocity = velocity;
        }

        private void ApplyHorizontalMovement(Rigidbody rigidbody, float fixedDeltaTime, float accelerationMultiplier)
        {
            if (_inputMagnitude > _tuning.MoveInputDeadzone)
            {
                rigidbody.WakeUp();
            }

            Vector3 velocity = rigidbody.linearVelocity;
            Vector3 currentHorizontal = new Vector3(velocity.x, 0f, velocity.z);
            Vector3 targetHorizontal = _moveDirection * (_tuning.MaxSpeed * _inputMagnitude);

            float maxDelta = _tuning.MaxAllowedAcceleration * accelerationMultiplier * fixedDeltaTime;
            Vector3 newHorizontal = Vector3.MoveTowards(currentHorizontal, targetHorizontal, maxDelta);

            velocity.x = newHorizontal.x;
            velocity.z = newHorizontal.z;
            rigidbody.linearVelocity = velocity;
        }
    }
}
