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
        [SerializeField] private float _riseGravity = -25f;
        [SerializeField] private float _fallGravity = -40f;
        [SerializeField] private float _maxFallSpeed = 20f;

        public float MaxSpeed => _maxSpeed;
        public float MaxAllowedAcceleration => _maxAllowedAcceleration;
        public float MoveInputDeadzone => _moveInputDeadzone;
        public float RiseGravity => _riseGravity;
        public float FallGravity => _fallGravity;
        public float MaxFallSpeed => _maxFallSpeed;

        public PlayerTuning()
        {
        }

        public PlayerTuning(
            float maxSpeed,
            float maxAllowedAcceleration,
            float moveInputDeadzone,
            float riseGravity = -25f,
            float fallGravity = -40f,
            float maxFallSpeed = 20f)
        {
            _maxSpeed = maxSpeed;
            _maxAllowedAcceleration = maxAllowedAcceleration;
            _moveInputDeadzone = moveInputDeadzone;
            _riseGravity = riseGravity;
            _fallGravity = fallGravity;
            _maxFallSpeed = maxFallSpeed;
        }
    }
}
