using UnityEngine;

namespace MarioTest.Player
{
    [CreateAssetMenu(fileName = "PlayerTuning", menuName = "MarioTest/Player Tuning")]
    public sealed class PlayerTuning : ScriptableObject
    {
        [SerializeField] private float _maxSpeed = 8f;
        [SerializeField] private float _maxAllowedAcceleration = 50f;
        [SerializeField] private float _moveInputDeadzone = 0.1f;
        [SerializeField] private float _riseGravity = -25f;
        [SerializeField] private float _fallGravity = -40f;
        [SerializeField] private float _maxFallSpeed = 20f;
        [SerializeField] private float _jumpVelocity = 12.5f;
        [SerializeField] private float _stompVelocity = 14.1f;
        [SerializeField] private float _coyoteTime = 0.3f;
        [SerializeField] private float _jumpBuffer = 0.12f;
        [SerializeField] private float _lowJumpGravity = -80f;
        [SerializeField] private float _knockbackDecay = 5.5f;
        [SerializeField] private float _maxKnockbackSpeed = 25f;
        [SerializeField] private float _knockbackStopSpeed = 0.1f;

        public float MaxSpeed => _maxSpeed;
        public float MaxAllowedAcceleration => _maxAllowedAcceleration;
        public float MoveInputDeadzone => _moveInputDeadzone;
        public float RiseGravity => _riseGravity;
        public float FallGravity => _fallGravity;
        public float MaxFallSpeed => _maxFallSpeed;
        public float JumpVelocity => _jumpVelocity;
        public float StompVelocity => _stompVelocity;
        public float CoyoteTime => _coyoteTime;
        public float JumpBuffer => _jumpBuffer;
        public float LowJumpGravity => _lowJumpGravity;
        public float KnockbackDecay => _knockbackDecay;
        public float MaxKnockbackSpeed => _maxKnockbackSpeed;
        public float KnockbackStopSpeed => _knockbackStopSpeed;
        public float KnockbackStopSpeedSqr => _knockbackStopSpeed * _knockbackStopSpeed;
    }
}
