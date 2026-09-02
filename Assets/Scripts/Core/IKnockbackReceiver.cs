using UnityEngine;

namespace MarioTest.Core
{
    public interface IKnockbackReceiver
    {
        void ApplyKnockback(Vector3 velocity);
    }
}
