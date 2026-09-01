using UnityEngine;

namespace MarioTest.Player
{
    public sealed class PlayerMovement
    {
        private readonly PlayerTuning _tuning;
        private Vector3 _moveDirection;
        private float _inputMagnitude;

        public PlayerMovement(PlayerTuning tuning)
        {
            _tuning = tuning;
        }

        public void SetMoveInput(Vector3 normalizedDirection, float magnitude)
        {
            _moveDirection = normalizedDirection;
            _inputMagnitude = magnitude;
        }

        public void ApplyMovement(Rigidbody rigidbody, float fixedDeltaTime, bool isGrounded)
        {
            ApplyGravity(rigidbody, fixedDeltaTime, isGrounded);
            ApplyHorizontalMovement(rigidbody, fixedDeltaTime);
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

            float gravity = velocity.y > 0f ? _tuning.RiseGravity : _tuning.FallGravity;
            velocity.y += gravity * fixedDeltaTime;
            velocity.y = Mathf.Max(velocity.y, -_tuning.MaxFallSpeed);

            rigidbody.linearVelocity = velocity;
        }

        private void ApplyHorizontalMovement(Rigidbody rigidbody, float fixedDeltaTime)
        {
            if (_inputMagnitude > _tuning.MoveInputDeadzone)
            {
                // Idle rigidbodies sleep; wake on input so movement responds immediately after standing still.
                rigidbody.WakeUp();
            }

            Vector3 velocity = rigidbody.linearVelocity;
            Vector3 currentHorizontal = new Vector3(velocity.x, 0f, velocity.z);
            Vector3 targetHorizontal = _moveDirection * (_tuning.MaxSpeed * _inputMagnitude);

            Vector3 deltaVelocity = targetHorizontal - currentHorizontal;
            Vector3 accelerationNeeded = deltaVelocity / fixedDeltaTime;

            float maxAllowedAcceleration = _tuning.MaxAllowedAcceleration;
            float accelerationSqr = accelerationNeeded.sqrMagnitude;
            if (accelerationSqr > maxAllowedAcceleration * maxAllowedAcceleration)
            {
                accelerationNeeded = accelerationNeeded.normalized * maxAllowedAcceleration;
            }

            rigidbody.AddForce(accelerationNeeded, ForceMode.Acceleration);
        }
    }
}
