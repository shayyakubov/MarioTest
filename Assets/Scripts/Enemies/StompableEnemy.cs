using MarioTest.Core;
using UnityEngine;

namespace MarioTest.Enemies
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Collider))]
    [RequireComponent(typeof(Rigidbody))]
    public sealed class StompableEnemy : MonoBehaviour
    {
        [SerializeField] private float _sideKnockbackSpeed = 20f;
        [SerializeField] private float _minFallSpeed = 2f;
        [SerializeField] private float _stompTopTolerance = 0.55f;
        [SerializeField] private float _contactCooldown = 0.35f;

        private Collider _collider;
        private float _contactCooldownRemaining;

        private void Awake()
        {
            _collider = GetComponent<Collider>();

            Rigidbody rigidbody = GetComponent<Rigidbody>();
            rigidbody.isKinematic = true;
            rigidbody.useGravity = false;
            rigidbody.interpolation = RigidbodyInterpolation.Interpolate;
            rigidbody.constraints = RigidbodyConstraints.FreezeRotation;
        }

        private void Update()
        {
            if (_contactCooldownRemaining > 0f)
            {
                _contactCooldownRemaining -= Time.deltaTime;
            }
        }

        private void OnCollisionEnter(Collision collision)
        {
            if (_contactCooldownRemaining > 0f)
            {
                return;
            }

            if (TryHandleStomp(collision))
            {
                return;
            }

            if (TryHandleSideHit(collision))
            {
                _contactCooldownRemaining = _contactCooldown;
            }
        }

        private bool TryHandleStomp(Collision collision)
        {
            Rigidbody playerRigidbody = collision.rigidbody;
            if (playerRigidbody == null)
            {
                return false;
            }

            if (!IsStomp(collision, playerRigidbody))
            {
                return false;
            }

            if (!playerRigidbody.TryGetComponent(out IBounceReceiver bounceReceiver))
            {
                bounceReceiver = playerRigidbody.GetComponentInParent<IBounceReceiver>();
            }

            if (bounceReceiver == null)
            {
                return false;
            }

            bounceReceiver.ApplyBounce();
            Destroy(gameObject);
            return true;
        }

        private bool IsStomp(Collision collision, Rigidbody playerRigidbody)
        {
            Collider playerCollider = collision.collider;
            if (playerCollider == null)
            {
                return false;
            }

            float fallSpeed = Mathf.Max(-collision.relativeVelocity.y, -playerRigidbody.linearVelocity.y);
            if (fallSpeed < _minFallSpeed)
            {
                return false;
            }

            Bounds enemyBounds = _collider.bounds;
            Bounds playerBounds = playerCollider.bounds;

            if (playerBounds.min.y < enemyBounds.max.y - _stompTopTolerance)
            {
                return false;
            }

            // Feet above enemy midline — avoids side grazes without requiring player center high above enemy center.
            if (playerBounds.min.y < enemyBounds.center.y)
            {
                return false;
            }

            return true;
        }

        private bool TryHandleSideHit(Collision collision)
        {
            Rigidbody playerRigidbody = collision.rigidbody;
            if (playerRigidbody == null)
            {
                return false;
            }

            if (IsStomp(collision, playerRigidbody))
            {
                return false;
            }

            if (!playerRigidbody.TryGetComponent(out ILifeTarget lifeTarget))
            {
                lifeTarget = playerRigidbody.GetComponentInParent<ILifeTarget>();
            }

            if (lifeTarget == null)
            {
                return false;
            }

            TryApplySideKnockback(collision, playerRigidbody);
            lifeTarget.TakeHit(respawnAtCheckpoint: false);
            return true;
        }

        private void TryApplySideKnockback(Collision collision, Rigidbody playerRigidbody)
        {
            if (!playerRigidbody.TryGetComponent(out IKnockbackReceiver knockbackReceiver))
            {
                knockbackReceiver = playerRigidbody.GetComponentInParent<IKnockbackReceiver>();
            }

            if (knockbackReceiver == null)
            {
                return;
            }

            Vector3 knockback = playerRigidbody.position - transform.position;
            knockback.y = 0f;

            if (knockback.sqrMagnitude < GameplayEpsilon.VelocitySqr)
            {
                knockback = -collision.GetContact(0).normal;
                knockback.y = 0f;
            }

            if (knockback.sqrMagnitude < GameplayEpsilon.VelocitySqr)
            {
                return;
            }

            knockbackReceiver.ApplyKnockback(knockback.normalized * _sideKnockbackSpeed);
        }
    }
}
