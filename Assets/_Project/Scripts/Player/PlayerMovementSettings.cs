using System;
using UnityEngine;

namespace MarioTest.Player
{
    [Serializable]
    public sealed class LayerAccelerationEntry
    {
        [SerializeField] private LayerMask _layerMask;
        [SerializeField] private float _accelerationMultiplier = 1f;

        public bool Matches(int layer)
        {
            if (layer < 0 || _layerMask.value == 0)
            {
                return false;
            }

            return (_layerMask.value & (1 << layer)) != 0;
        }

        public float AccelerationMultiplier => _accelerationMultiplier;
    }

    [Serializable]
    public sealed class PlayerMovementSettings
    {
        [SerializeField] private float _airborneAccelerationMultiplier = 1f;
        [SerializeField] private float _defaultGroundAccelerationMultiplier = 1f;
        [SerializeField] private LayerAccelerationEntry[] _layerAccelerations = new LayerAccelerationEntry[1];

        public float AirborneAccelerationMultiplier => _airborneAccelerationMultiplier;
        public float DefaultGroundAccelerationMultiplier => _defaultGroundAccelerationMultiplier;

        public float GetAccelerationMultiplier(bool isGrounded, int groundLayer)
        {
            if (!isGrounded)
            {
                return _airborneAccelerationMultiplier;
            }

            if (groundLayer >= 0 && _layerAccelerations != null)
            {
                for (int i = 0; i < _layerAccelerations.Length; i++)
                {
                    LayerAccelerationEntry entry = _layerAccelerations[i];
                    if (entry.Matches(groundLayer))
                    {
                        return entry.AccelerationMultiplier;
                    }
                }
            }

            return _defaultGroundAccelerationMultiplier;
        }
    }
}
