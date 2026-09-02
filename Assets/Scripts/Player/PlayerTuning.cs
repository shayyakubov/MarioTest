using System;
using UnityEngine;

namespace MarioTest.Player
{
    [Serializable]
    public sealed class PlayerTuning
    {
        [SerializeField] private float _maxSpeed = 6f;
        [SerializeField] private float _maxAllowedAcceleration = 50f;
        [SerializeField] private float _moveInputDeadzone = 0.1f;
        [SerializeField] private float _riseGravity = -32f;
        [SerializeField] private float _fallGravity = -40f;
        [SerializeField] private float _maxFallSpeed = 20f;
        [SerializeField] private float _jumpVelocity = 14.1f;
        [SerializeField] private float _stompVelocity = 14.1f;
        [SerializeField] private float _coyoteTime = 0.2f;
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

        public PlayerTuning()
        {
        }

        public PlayerTuning(
            float maxSpeed,
            float maxAllowedAcceleration,
            float moveInputDeadzone,
            float riseGravity = -32f,
            float fallGravity = -40f,
            float maxFallSpeed = 20f,
            float jumpVelocity = 14.1f,
            float coyoteTime = 0.2f,
            float jumpBuffer = 0.12f,
            float lowJumpGravity = -80f)
        {
            _maxSpeed = maxSpeed;
            _maxAllowedAcceleration = maxAllowedAcceleration;
            _moveInputDeadzone = moveInputDeadzone;
            _riseGravity = riseGravity;
            _fallGravity = fallGravity;
            _maxFallSpeed = maxFallSpeed;
            _jumpVelocity = jumpVelocity;
            _coyoteTime = coyoteTime;
            _jumpBuffer = jumpBuffer;
            _lowJumpGravity = lowJumpGravity;
        }
    }
}
