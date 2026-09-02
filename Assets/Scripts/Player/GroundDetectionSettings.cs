using System;
using UnityEngine;

namespace MarioTest.Player
{
    [Serializable]
    public sealed class GroundDetectionSettings
    {
        [SerializeField] private float _checkDistance = 0.1f;
        [SerializeField] private float _probePadding = 0.02f;
        [SerializeField] private float _probeRadius = 0.12f;
        [SerializeField] private float _maxSlopeAngle = 45f;
        [SerializeField] private float _maxFootprintRadiusScale = 0.35f;

        public float CheckDistance => _checkDistance;
        public float ProbePadding => _probePadding;
        public float ProbeRadius => _probeRadius;
        public float MinGroundNormalY => Mathf.Cos(_maxSlopeAngle * Mathf.Deg2Rad);
        public float MaxFootprintRadiusScale => _maxFootprintRadiusScale;
    }
}
