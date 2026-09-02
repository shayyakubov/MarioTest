using System;
using MarioTest.Core;
using UnityEngine;

namespace MarioTest.Interaction
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Collider))]
    public sealed class CheckpointTrigger : MonoBehaviour
    {
        [SerializeField] private Transform _checkpointTransform;

        public event Action<CheckpointTrigger> Activated;

        public Transform SpawnPoint => _checkpointTransform != null ? _checkpointTransform : transform;

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

            Activated?.Invoke(this);
        }
    }
}
