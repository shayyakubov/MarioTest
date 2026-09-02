using MarioTest.Core;
using UnityEngine;

namespace MarioTest.Enemies
{
    [DisallowMultipleComponent]
    public sealed class PatrolShooterEnemy : MonoBehaviour, IEnemy
    {
        [SerializeField] private Transform _patrolPointA;
        [SerializeField] private Transform _patrolPointB;
        [SerializeField] private EnemyProjectile _projectilePrefab;
        [SerializeField] private Transform _muzzle;
        [SerializeField] private float _patrolSpeed = 2f;
        [SerializeField] private float _shootRange = 12f;
        [SerializeField] private float _fireInterval = 3f;
        [SerializeField] private float _maxShootElevationAngle = 20f;
        [SerializeField] private int _predictionIterations = 3;
        [SerializeField] private float _patrolArrivalDistance = 0.1f;
        [SerializeField] private float _fallbackMuzzleHeight = 0.3f;

        private Transform _patrolTarget;
        private Transform _targetTransform;
        private Rigidbody _rigidbody;
        private Rigidbody _targetRigidbody;
        private float _fireCooldown;

        public void Initialize(Transform targetTransform, Rigidbody targetRigidbody)
        {
            _targetTransform = targetTransform;
            _targetRigidbody = targetRigidbody;
            _patrolTarget = _patrolPointA;
        }

        private void Awake()
        {
            _rigidbody = GetComponent<Rigidbody>();
        }

        private void FixedUpdate()
        {
            Patrol();
        }

        private void Update()
        {
            TryShoot();
        }

        private void Patrol()
        {
            if (_patrolPointA == null || _patrolPointB == null)
            {
                return;
            }

            if (_patrolTarget == null)
            {
                _patrolTarget = _patrolPointA;
            }

            Vector3 previousPosition = _rigidbody.position;
            Vector3 nextPosition = Vector3.MoveTowards(
                previousPosition,
                _patrolTarget.position,
                _patrolSpeed * Time.fixedDeltaTime);

            _rigidbody.MovePosition(nextPosition);

            Vector3 moveDelta = nextPosition - previousPosition;
            moveDelta.y = 0f;
            if (moveDelta.sqrMagnitude > GameplayEpsilon.VelocitySqr)
            {
                transform.rotation = Quaternion.LookRotation(moveDelta);
            }

            float arrivalDistanceSqr = _patrolArrivalDistance * _patrolArrivalDistance;
            if ((nextPosition - _patrolTarget.position).sqrMagnitude < arrivalDistanceSqr)
            {
                _patrolTarget = _patrolTarget == _patrolPointA ? _patrolPointB : _patrolPointA;
            }
        }

        private void TryShoot()
        {
            if (_targetTransform == null || _projectilePrefab == null)
            {
                return;
            }

            Vector3 toPlayer = _targetTransform.position - transform.position;
            toPlayer.y = 0f;

            if (toPlayer.sqrMagnitude > _shootRange * _shootRange)
            {
                return;
            }

            _fireCooldown -= Time.deltaTime;
            if (_fireCooldown > 0f)
            {
                return;
            }

            Vector3 shotOrigin = GetShotOrigin();
            if (!IsWithinShootElevation(shotOrigin, _targetTransform.position))
            {
                return;
            }

            transform.rotation = Quaternion.LookRotation(toPlayer.normalized);
            Fire();
            _fireCooldown = _fireInterval;
        }

        private void Fire()
        {
            Vector3 origin = GetShotOrigin();
            Vector3 targetVelocity = _targetRigidbody != null ? _targetRigidbody.linearVelocity : Vector3.zero;
            Vector3 predictedTarget = ProjectilePrediction.PredictPosition(
                origin,
                _targetTransform.position,
                targetVelocity,
                _projectilePrefab.Speed,
                _predictionIterations);
            predictedTarget = ClampShotTarget(origin, predictedTarget);

            EnemyProjectile projectile = Instantiate(_projectilePrefab, origin, Quaternion.identity);
            projectile.LaunchAt(predictedTarget);
        }

        private Vector3 GetShotOrigin()
        {
            return _muzzle != null
                ? _muzzle.position
                : transform.position + Vector3.up * _fallbackMuzzleHeight;
        }

        private bool IsWithinShootElevation(Vector3 origin, Vector3 targetPosition)
        {
            Vector3 delta = targetPosition - origin;
            float horizontalDistance = new Vector3(delta.x, 0f, delta.z).magnitude;
            if (horizontalDistance < GameplayEpsilon.MinSpeed)
            {
                return delta.y <= 0f;
            }

            float elevationDegrees = Mathf.Atan2(delta.y, horizontalDistance) * Mathf.Rad2Deg;
            return elevationDegrees <= _maxShootElevationAngle;
        }

        private Vector3 ClampShotTarget(Vector3 origin, Vector3 targetPosition)
        {
            Vector3 delta = targetPosition - origin;
            float horizontalDistance = new Vector3(delta.x, 0f, delta.z).magnitude;
            if (horizontalDistance < GameplayEpsilon.MinSpeed)
            {
                delta.y = Mathf.Min(delta.y, 0f);
                return origin + delta;
            }

            float maxRise = horizontalDistance * Mathf.Tan(_maxShootElevationAngle * Mathf.Deg2Rad);
            delta.y = Mathf.Min(delta.y, maxRise);
            return origin + delta;
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, _shootRange);

            if (_patrolPointA != null && _patrolPointB != null)
            {
                Gizmos.color = Color.cyan;
                Gizmos.DrawLine(_patrolPointA.position, _patrolPointB.position);
            }
        }
    }
}
