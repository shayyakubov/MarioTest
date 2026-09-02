using System;
using MarioTest.Core;
using UnityEngine;

namespace MarioTest.Interaction
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Collider))]
    public sealed class CoinPickup : MonoBehaviour
    {
        public event Action<CoinPickup> Collected;

        private void Reset()
        {
            Collider collider = GetComponent<Collider>();
            collider.isTrigger = true;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.attachedRigidbody == null || other.gameObject.layer != PhysicsLayers.PlayerLayer)
            {
                return;
            }

            Collected?.Invoke(this);
        }
    }
}
