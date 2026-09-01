using MarioTest.Platforms;
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
        private Vector3 _surfaceVelocity;
        private Collider _surfaceCollider;
        private Vector3 _knockbackVelocity;

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

        public void ApplyKnockback(Vector3 velocity)
        {
            velocity.y = 0f;
            _knockbackVelocity += velocity;
        }

        public void ApplyMovement(Rigidbody rigidbody, float fixedDeltaTime, bool isGrounded, Collider groundCollider)
        {
            if (!isGrounded)
            {
                rigidbody.WakeUp();
            }

            UpdateSurfaceVelocity(isGrounded, groundCollider);
            ApplyHorizontalMovement(rigidbody, fixedDeltaTime, isGrounded, groundCollider);

            Vector3 velocity = rigidbody.linearVelocity;
            TryExecuteJump(ref velocity.y, isGrounded, fixedDeltaTime);
            rigidbody.linearVelocity = velocity;

            ApplyGravity(rigidbody, fixedDeltaTime, isGrounded);
            DecayKnockback(fixedDeltaTime);

            _jumpPressedLatched = false;
            _wasGrounded = isGrounded;
        }

        private void UpdateSurfaceVelocity(bool isGrounded, Collider groundCollider)
        {
            if (!isGrounded)
            {
                _surfaceCollider = null;
                return;
            }

            if (groundCollider != _surfaceCollider)
            {
                _surfaceCollider = groundCollider;
                _surfaceVelocity = Vector3.zero;
            }

            if (TryGetSurfaceHorizontal(groundCollider, out Vector3 surfaceHorizontal))
            {
                _surfaceVelocity = surfaceHorizontal;
            }
        }

        private static bool TryGetSurfaceHorizontal(Collider groundCollider, out Vector3 surfaceHorizontal)
        {
            if (groundCollider.TryGetComponent(out IMovingSurface movingSurface))
            {
                surfaceHorizontal = new Vector3(movingSurface.Velocity.x, 0f, movingSurface.Velocity.z);
                return true;
            }

            movingSurface = groundCollider.GetComponentInParent<IMovingSurface>();
            if (movingSurface != null)
            {
                surfaceHorizontal = new Vector3(movingSurface.Velocity.x, 0f, movingSurface.Velocity.z);
                return true;
            }

            surfaceHorizontal = Vector3.zero;
            return false;
        }

        private void TryExecuteJump(ref float verticalVelocity, bool isGrounded, float fixedDeltaTime)
        {
            UpdateJumpTimers(verticalVelocity, isGrounded, fixedDeltaTime);

            bool jumpRequested = _jumpPressedLatched || _jumpBufferRemaining > 0f;
            if (!jumpRequested)
            {
                return;
            }

            bool canJump = isGrounded || (_coyoteTimeRemaining > 0f && verticalVelocity <= 0f);
            if (!canJump)
            {
                return;
            }

            _coyoteTimeRemaining = 0f;
            _jumpBufferRemaining = 0f;
            verticalVelocity = _tuning.JumpVelocity;
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

            // Ledge/corner contact can zero vy while our ground probe says airborne — keep falling.
            if (velocity.y <= 0f)
            {
                velocity.y = Mathf.Min(velocity.y, -_tuning.MaxFallSpeed * 0.2f);
            }

            rigidbody.linearVelocity = velocity;
        }

        private void ApplyHorizontalMovement(
            Rigidbody rigidbody,
            float fixedDeltaTime,
            bool isGrounded,
            Collider groundCollider)
        {
            if (_inputMagnitude > _tuning.MoveInputDeadzone || _surfaceVelocity.sqrMagnitude > 0.0001f)
            {
                rigidbody.WakeUp();
            }

            float accelerationMultiplier = 1f;

            if (isGrounded && groundCollider != null
                && groundCollider.TryGetComponent(out IMovementModifierSurface modifierSurface))
            {
                accelerationMultiplier = modifierSurface.AccelerationMultiplier;
            }

            Vector3 velocity = rigidbody.linearVelocity;
            Vector3 horizontalVelocity = new Vector3(velocity.x, 0f, velocity.z);
            Vector3 targetVelocity = _moveDirection * (_tuning.MaxSpeed * _inputMagnitude) + _surfaceVelocity;

            bool isDecelerating = horizontalVelocity.sqrMagnitude > targetVelocity.sqrMagnitude + 0.0001f;
            float movementMultiplier = ResolveMovementMultiplier(
                isGrounded,
                isDecelerating,
                accelerationMultiplier);

            float maxDelta = _tuning.MaxAllowedAcceleration * movementMultiplier * fixedDeltaTime;
            Vector3 newHorizontalVelocity = Vector3.MoveTowards(horizontalVelocity, targetVelocity, maxDelta);

            velocity.x = newHorizontalVelocity.x + _knockbackVelocity.x;
            velocity.z = newHorizontalVelocity.z + _knockbackVelocity.z;
            rigidbody.linearVelocity = velocity;
        }

        private void DecayKnockback(float fixedDeltaTime)
        {
            _knockbackVelocity = Vector3.MoveTowards(
                _knockbackVelocity,
                Vector3.zero,
                _tuning.KnockbackDecay * fixedDeltaTime);
        }

        private float ResolveMovementMultiplier(
            bool isGrounded,
            bool isDecelerating,
            float surfaceAccelerationMultiplier)
        {
            if (isGrounded)
            {
                return isDecelerating
                    ? _movementSettings.GroundDecelerationMultiplier * surfaceAccelerationMultiplier
                    : surfaceAccelerationMultiplier;
            }

            return isDecelerating
                ? _movementSettings.AirborneDecelerationMultiplier
                : _movementSettings.AirborneAccelerationMultiplier;
        }
    }
}
