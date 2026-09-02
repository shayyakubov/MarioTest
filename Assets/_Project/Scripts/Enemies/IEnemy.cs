using UnityEngine;

namespace MarioTest.Enemies
{
    public interface IEnemy
    {
        void Initialize(Transform targetTransform, Rigidbody targetRigidbody);
    }
}
