using MarioTest.Core;
using UnityEngine;

namespace MarioTest.Enemies
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Rigidbody))]
    [RequireComponent(typeof(SphereCollider))]
    public sealed class EnemyProjectile : MonoBehaviour
    {
        [SerializeField] private float _speed = 10f;
        [SerializeField] private float _knockbackSpeed = 25f;
        [SerializeField] private float _lifetime = 4f;
        [SerializeField] private float _spawnForwardOffset = 0.4f;

        private Rigidbody _rigidbody;
        private SphereCollider _collider;
        private float _lifeRemaining;
        private bool _hasHit;

        public float Speed => _speed;

        private void Awake()
        {
            _rigidbody = GetComponent<Rigidbody>();
            _collider = GetComponent<SphereCollider>();
            _rigidbody.useGravity = false;
            _rigidbody.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        }

        public void LaunchAt(Vector3 targetPoint)
        {
            Vector3 direction = targetPoint - transform.position;
            Launch(direction);
        }

        public void Launch(Vector3 direction)
        {
            Vector3 normalizedDirection = direction.sqrMagnitude > GameplayEpsilon.VelocitySqr
                ? direction.normalized
                : transform.forward;

            transform.position += normalizedDirection * _spawnForwardOffset;
            transform.rotation = Quaternion.LookRotation(normalizedDirection);
            _rigidbody.linearVelocity = normalizedDirection * _speed;
            _lifeRemaining = _lifetime;
        }

        private void FixedUpdate()
        {
            _lifeRemaining -= Time.fixedDeltaTime;
            if (_lifeRemaining <= 0f)
            {
                Destroy(gameObject);
            }
        }

        private void OnCollisionEnter(Collision collision)
        {
            if (_hasHit)
            {
                return;
            }

            _hasHit = true;
            TryApplyKnockback(collision.collider);

            _collider.enabled = false;
            _rigidbody.linearVelocity = Vector3.zero;
            Destroy(gameObject);
        }

        private void TryApplyKnockback(Collider collider)
        {
            if (!collider.TryGetComponent(out IKnockbackReceiver receiver))
            {
                receiver = collider.GetComponentInParent<IKnockbackReceiver>();
            }

            if (receiver == null)
            {
                return;
            }

            Vector3 knockback = _rigidbody.linearVelocity;
            knockback.y = 0f;

            if (knockback.sqrMagnitude < GameplayEpsilon.VelocitySqr)
            {
                knockback = transform.forward * _knockbackSpeed;
            }
            else
            {
                knockback = knockback.normalized * _knockbackSpeed;
            }

            receiver.ApplyKnockback(knockback);
        }
    }
}
