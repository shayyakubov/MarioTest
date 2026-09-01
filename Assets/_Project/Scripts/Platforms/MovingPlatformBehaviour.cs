using UnityEngine;

namespace MarioTest.Platforms
{
    [DisallowMultipleComponent]
    [DefaultExecutionOrder(-5)]
    public sealed class MovingPlatformBehaviour : MonoBehaviour, IMovingSurface
    {
        [SerializeField] private float _speed = 2f;
        [SerializeField] private Vector3 _endOffset = new Vector3(4f, 0f, 0f);

        private Vector3 _startPosition;
        private Vector3 _endPosition;
        private Vector3 _velocity;
        private float _direction = 1f;

        public Vector3 Velocity => _velocity;

        private void Awake()
        {
            _startPosition = transform.position;
            _endPosition = _startPosition + _endOffset;
        }

        private void FixedUpdate()
        {
            Vector3 previousPosition = transform.position;
            Vector3 target = _direction > 0f ? _endPosition : _startPosition;
            Vector3 next = Vector3.MoveTowards(previousPosition, target, _speed * Time.fixedDeltaTime);

            if ((next - target).sqrMagnitude < 0.0001f)
            {
                _direction *= -1f;
            }

            transform.position = next;
            _velocity = (next - previousPosition) / Time.fixedDeltaTime;
        }
    }
}
