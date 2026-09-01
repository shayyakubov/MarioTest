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
        [SerializeField] private float _knockbackSpeed = 10f;
        [SerializeField] private float _lifetime = 4f;

        private Rigidbody _rigidbody;
        private float _lifeRemaining;

        private void Awake()
        {
            _rigidbody = GetComponent<Rigidbody>();
            _rigidbody.useGravity = false;
            _rigidbody.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        }

        public void Launch(Vector3 direction)
        {
            Vector3 normalizedDirection = direction.sqrMagnitude > 0.0001f
                ? direction.normalized
                : transform.forward;

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
            TryApplyKnockback(collision.collider);
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

            if (knockback.sqrMagnitude < 0.0001f)
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
