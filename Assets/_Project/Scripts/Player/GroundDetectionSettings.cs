using System;
using UnityEngine;

namespace MarioTest.Player
{
    [Serializable]
    public sealed class GroundDetectionSettings
    {
        [SerializeField] private float _checkDistance = 0.1f;
        [SerializeField] private float _skinWidth = 0.02f;
        [SerializeField] private float _maxSlopeAngle = 45f;

        public float CheckDistance => _checkDistance;
        public float SkinWidth => _skinWidth;
        public float MinGroundNormalY => Mathf.Cos(_maxSlopeAngle * Mathf.Deg2Rad);
    }
}
