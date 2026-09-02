using MarioTest.Core;
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
        private Vector3 _motorHorizontalVelocity;
        // Stomp: upward impulse queued here; hold vs hop is decided in ApplyGravity.
        private float _bounceVelocity;

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
            _knockbackVelocity = Vector3.ClampMagnitude(velocity, _tuning.MaxKnockbackSpeed);
        }

        /// <summary>
        /// Enemy stomp: queue upward speed only. Hold vs release height comes from
        /// <see cref="ApplyGravity"/> (same rise/low-jump gravity as a normal jump when vy &gt; 0).
        /// </summary>
        public void ApplyBounce()
        {
            _bounceVelocity = _tuning.StompVelocity;
        }

        public void Reset()
        {
            _moveDirection = Vector3.zero;
            _inputMagnitude = 0f;
            _jumpHeld = false;
            _jumpPressedLatched = false;
            _coyoteTimeRemaining = 0f;
            _jumpBufferRemaining = 0f;
            _wasGrounded = false;
            _surfaceVelocity = Vector3.zero;
            _surfaceCollider = null;
            _knockbackVelocity = Vector3.zero;
            _motorHorizontalVelocity = Vector3.zero;
            _bounceVelocity = 0f;
        }

        public void ApplyMovement(Rigidbody rigidbody, float fixedDeltaTime, bool isGrounded, Collider groundCollider)
        {
            if (!isGrounded)
            {
                rigidbody.WakeUp();
            }

            ApplyHorizontalMovement(rigidbody, fixedDeltaTime, isGrounded, groundCollider);

            Vector3 velocity = rigidbody.linearVelocity;

            if (_bounceVelocity > 0f)
            {
                // Stomp frame: impulse here, then ApplyGravity below shapes hold vs hop.
                velocity.y = Mathf.Max(velocity.y, _bounceVelocity);
                _bounceVelocity = 0f;
                _jumpPressedLatched = false;
                _jumpBufferRemaining = 0f;
                _coyoteTimeRemaining = 0f;
            }
            else
            {
                velocity.y = TryExecuteJump(velocity.y, isGrounded, fixedDeltaTime);
            }

            rigidbody.linearVelocity = velocity;

            ApplyGravity(rigidbody, fixedDeltaTime, isGrounded);
            DecayKnockback(fixedDeltaTime);

            _jumpPressedLatched = false;
            _wasGrounded = isGrounded;
        }

        /// <summary>
        /// Tracks horizontal velocity from the ground collider (e.g. moving platforms).
        /// When still on the same surface and its speed changes, applies the delta to the
        /// Rigidbody and run-speed state so the player picks up acceleration without waiting
        /// for the next target-velocity step.
        /// While airborne, keeps the last <see cref="_surfaceVelocity"/> until grounded again.
        /// </summary>
        private void UpdateSurfaceTracking(Rigidbody rigidbody, bool isGrounded, Collider groundCollider)
        {
            if (!isGrounded || groundCollider == null)
            {
                return;
            }

            bool onMovingSurface = TryGetSurfaceHorizontal(groundCollider, out Vector3 surfaceHorizontal);
            if (!onMovingSurface)
            {
                surfaceHorizontal = Vector3.zero;
            }

            bool isSameSurface = groundCollider == _surfaceCollider;

            if (onMovingSurface && isSameSurface)
            {
                Vector3 delta = surfaceHorizontal - _surfaceVelocity;
                if (delta.sqrMagnitude > GameplayEpsilon.VelocitySqr)
                {
                    Vector3 velocity = rigidbody.linearVelocity;
                    velocity.x += delta.x;
                    velocity.z += delta.z;
                    rigidbody.linearVelocity = velocity;

                    _motorHorizontalVelocity.x += delta.x;
                    _motorHorizontalVelocity.z += delta.z;
                }
            }

            _surfaceCollider = groundCollider;
            _surfaceVelocity = surfaceHorizontal;
        }

        private static bool TryGetSurfaceHorizontal(Collider groundCollider, out Vector3 surfaceHorizontal)
        {
            if (!groundCollider.TryGetComponent(out IMovingSurface movingSurface))
            {
                movingSurface = groundCollider.GetComponentInParent<IMovingSurface>();
            }

            if (movingSurface == null)
            {
                surfaceHorizontal = Vector3.zero;
                return false;
            }

            Vector3 velocity = movingSurface.Velocity;
            surfaceHorizontal = new Vector3(velocity.x, 0f, velocity.z);
            return true;
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
            else if (_coyoteTimeRemaining > 0f)
            {
                _coyoteTimeRemaining = Mathf.Max(_coyoteTimeRemaining - fixedDeltaTime, 0f);
            }

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
                // Variable jump height: held = riseGravity, released = lowJumpGravity.
                // Applies to normal jumps and stomp bounces alike (any rising arc).
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
                velocity.y = Mathf.Min(
                    velocity.y,
                    -_tuning.MaxFallSpeed * _movementSettings.MinAirborneFallSpeedFraction);
            }

            rigidbody.linearVelocity = velocity;
        }

        private void ApplyHorizontalMovement(
            Rigidbody rigidbody,
            float fixedDeltaTime,
            bool isGrounded,
            Collider groundCollider)
        {

            UpdateSurfaceTracking(rigidbody, isGrounded, groundCollider);

            if (_inputMagnitude > _tuning.MoveInputDeadzone || _surfaceVelocity.sqrMagnitude > GameplayEpsilon.VelocitySqr)
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
            Vector3 knockback = new Vector3(_knockbackVelocity.x, 0f, _knockbackVelocity.z);
            Vector3 targetVelocity = _moveDirection * (_tuning.MaxSpeed * _inputMagnitude) + _surfaceVelocity;

            bool isDecelerating = _motorHorizontalVelocity.sqrMagnitude > targetVelocity.sqrMagnitude + GameplayEpsilon.VelocitySqr;
            float movementMultiplier = ResolveMovementMultiplier(
                isGrounded,
                isDecelerating,
                accelerationMultiplier);

            float maxDelta = _tuning.MaxAllowedAcceleration * movementMultiplier * fixedDeltaTime;
            _motorHorizontalVelocity = Vector3.MoveTowards(_motorHorizontalVelocity, targetVelocity, maxDelta);

            velocity.x = _motorHorizontalVelocity.x + knockback.x;
            velocity.z = _motorHorizontalVelocity.z + knockback.z;
            rigidbody.linearVelocity = velocity;
        }

        // Exponential decay: knockback drops as a fraction of what's left, so one decay
        // value stays punchy across different hit strengths. Linear MoveTowards would
        // also work — raise MaxKnockbackSpeed and set decay ≈ speed / desired stop time.
        private void DecayKnockback(float fixedDeltaTime)
        {
            _knockbackVelocity *= Mathf.Exp(-_tuning.KnockbackDecay * fixedDeltaTime);

            if (_knockbackVelocity.sqrMagnitude < _tuning.KnockbackStopSpeedSqr)
            {
                _knockbackVelocity = Vector3.zero;
            }
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
