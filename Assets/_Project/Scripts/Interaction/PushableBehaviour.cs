using MarioTest.Core;
using UnityEngine;

namespace MarioTest.Interaction
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Rigidbody))]
    public sealed class PushableBehaviour : MonoBehaviour, IWorldRestorable
    {
        private Rigidbody _rigidbody;
        private Vector3 _initialPosition;
        private Quaternion _initialRotation;

        private void Awake()
        {
            _rigidbody = GetComponent<Rigidbody>();
            _initialPosition = transform.position;
            _initialRotation = transform.rotation;
        }

        public void ResetToInitialState()
        {
            transform.SetPositionAndRotation(_initialPosition, _initialRotation);
            _rigidbody.linearVelocity = Vector3.zero;
            _rigidbody.angularVelocity = Vector3.zero;
            _rigidbody.Sleep();
        }
    }
}
