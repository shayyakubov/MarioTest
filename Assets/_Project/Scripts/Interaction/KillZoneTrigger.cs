using MarioTest.Core;
using UnityEngine;

namespace MarioTest.Interaction
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Collider))]
    public sealed class KillZoneTrigger : MonoBehaviour
    {
        private void Reset()
        {
            GetComponent<Collider>().isTrigger = true;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.attachedRigidbody == null || other.gameObject.layer != PhysicsLayers.PlayerLayer)
            {
                return;
            }

            Debug.Log($"[KillZoneTrigger] OnTriggerEnter — {other.name}");

            if (!other.TryGetComponent(out ILifeTarget lifeTarget))
            {
                lifeTarget = other.GetComponentInParent<ILifeTarget>();
            }

            if (lifeTarget == null)
            {
                Debug.Log("[KillZoneTrigger] no ILifeTarget found");
                return;
            }

            lifeTarget.TakeHit();
        }
    }
}
