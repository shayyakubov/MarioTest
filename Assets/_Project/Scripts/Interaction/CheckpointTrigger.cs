using MarioTest.Core;
using MarioTest.Systems;
using UnityEngine;

namespace MarioTest.Interaction
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Collider))]
    public sealed class CheckpointTrigger : MonoBehaviour
    {
        [SerializeField] private Transform _checkpointTransform;

        private CheckpointsManager _checkpointsManager;

        private void Awake()
        {
            _checkpointsManager = Object.FindAnyObjectByType<CheckpointsManager>();
        }

        private void Reset()
        {
            Collider collider = GetComponent<Collider>();
            collider.isTrigger = true;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (_checkpointsManager == null || other.attachedRigidbody == null)
            {
                return;
            }

            if (other.gameObject.layer != PhysicsLayers.PlayerLayer)
            {
                return;
            }

            Transform checkpoint = _checkpointTransform != null ? _checkpointTransform : transform;
            _checkpointsManager.SetCheckpoint(checkpoint);
        }
    }
}
