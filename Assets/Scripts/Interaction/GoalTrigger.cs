using System;
using MarioTest.Core;
using MarioTest.Player;
using UnityEngine;

namespace MarioTest.Interaction
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Collider))]
    public sealed class GoalTrigger : MonoBehaviour
    {
        private bool _completed;

        public event Action CourseReached;

        private void Reset()
        {
            Collider collider = GetComponent<Collider>();
            collider.isTrigger = true;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (_completed)
            {
                return;
            }

            if (other.attachedRigidbody == null || other.gameObject.layer != PhysicsLayers.PlayerLayer)
            {
                return;
            }

            PlayerController controller = other.GetComponent<PlayerController>()
                ?? other.GetComponentInParent<PlayerController>();

            if (controller == null)
            {
                return;
            }

            _completed = true;
            CourseReached?.Invoke();
        }
    }
}
