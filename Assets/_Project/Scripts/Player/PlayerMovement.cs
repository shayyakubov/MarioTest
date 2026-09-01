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

        public void ApplyMovement(Rigidbody rigidbody, float fixedDeltaTime)
        {
            ApplyHorizontalMovement(rigidbody, fixedDeltaTime);
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

            float maxAcceleration = _tuning.MaxAcceleration;
            float accelerationSqr = accelerationNeeded.sqrMagnitude;
            if (accelerationSqr > maxAcceleration * maxAcceleration)
            {
                accelerationNeeded = accelerationNeeded.normalized * maxAcceleration;
            }

            rigidbody.AddForce(accelerationNeeded, ForceMode.Acceleration);
        }
    }
}
