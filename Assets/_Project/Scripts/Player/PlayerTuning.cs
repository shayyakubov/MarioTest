using System;
using UnityEngine;

namespace MarioTest.Player
{
    [Serializable]
    public sealed class PlayerTuning
    {
        [SerializeField] private float _maxSpeed = 6f;
        [SerializeField] private float _maxAcceleration = 50f;
        [SerializeField] private float _moveInputDeadzone = 0.1f;

        public float MaxSpeed => _maxSpeed;
        public float MaxAcceleration => _maxAcceleration;
        public float MoveInputDeadzone => _moveInputDeadzone;

        public PlayerTuning()
        {
        }

        public PlayerTuning(float maxSpeed, float maxAcceleration, float moveInputDeadzone)
        {
            _maxSpeed = maxSpeed;
            _maxAcceleration = maxAcceleration;
            _moveInputDeadzone = moveInputDeadzone;
        }
    }
}
