using UnityEngine;

namespace MarioTest.Platforms
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Rigidbody))]
    [DefaultExecutionOrder(-5)]
    public sealed class MovingPlatformBehaviour : MonoBehaviour, IMovingSurface
    {
        [SerializeField] private Rigidbody _rigidbody;
        [SerializeField] private float _speed = 2f;
        [SerializeField] private Transform _startTransform;
        [SerializeField] private Transform _endTransform;
        [SerializeField] private Vector3 _endOffset = new Vector3(4f, 0f, 0f);
        [SerializeField] private float _waypointArrivalDistance = 0.01f;

        private Vector3 _fallbackStartPosition;
        private Vector3 _fallbackEndPosition;
        private Vector3 _velocity;
        private float _direction = 1f;

        public Vector3 Velocity => _velocity;

        private void Awake()
        {
            if (_rigidbody == null)
            {
                _rigidbody = GetComponent<Rigidbody>();
            }

            _rigidbody.isKinematic = true;
            _rigidbody.useGravity = false;
            _rigidbody.interpolation = RigidbodyInterpolation.Interpolate;

            _fallbackStartPosition = _rigidbody.position;
            _fallbackEndPosition = _fallbackStartPosition + _endOffset;
        }

        private void FixedUpdate()
        {
            Vector3 startPosition = _startTransform != null ? _startTransform.position : _fallbackStartPosition;
            Vector3 endPosition = _endTransform != null ? _endTransform.position : _fallbackEndPosition;

            Vector3 previousPosition = _rigidbody.position;
            Vector3 target = _direction > 0f ? endPosition : startPosition;
            Vector3 next = Vector3.MoveTowards(previousPosition, target, _speed * Time.fixedDeltaTime);

            float arrivalDistanceSqr = _waypointArrivalDistance * _waypointArrivalDistance;
            if ((next - target).sqrMagnitude < arrivalDistanceSqr)
            {
                _direction *= -1f;
            }

            _rigidbody.MovePosition(next);
            _velocity = (next - previousPosition) / Time.fixedDeltaTime;
        }
    }
}
