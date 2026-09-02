using UnityEngine;

namespace MarioTest.Platforms
{
    [DisallowMultipleComponent]
    public sealed class IceSurfaceBehaviour : MonoBehaviour, IMovementModifierSurface
    {
        [SerializeField] private float _accelerationMultiplier = 0.1f;

        public float AccelerationMultiplier => _accelerationMultiplier;
    }
}