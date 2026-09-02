using System;
using UnityEngine;

namespace MarioTest.Player
{
    [Serializable]
    public sealed class PlayerMovementSettings
    {
        [SerializeField] private float _groundDecelerationMultiplier = 1f;
        [SerializeField] private float _airborneAccelerationMultiplier = 0.25f;
        [SerializeField] private float _airborneDecelerationMultiplier = 0.15f;
        [SerializeField] private float _minAirborneFallSpeedFraction = 0.2f;

        public float GroundDecelerationMultiplier => _groundDecelerationMultiplier;
        public float AirborneAccelerationMultiplier => _airborneAccelerationMultiplier;
        public float AirborneDecelerationMultiplier => _airborneDecelerationMultiplier;
        public float MinAirborneFallSpeedFraction => _minAirborneFallSpeedFraction;
    }
}
