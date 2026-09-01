using UnityEngine;

namespace MarioTest.Enemies
{
    [DisallowMultipleComponent]
    public sealed class PatrolShooterEnemy : MonoBehaviour
    {
        [SerializeField] private Transform _patrolPointA;
        [SerializeField] private Transform _patrolPointB;
        [SerializeField] private Transform _player;
        [SerializeField] private EnemyProjectile _projectilePrefab;
        [SerializeField] private Transform _muzzle;
        [SerializeField] private float _patrolSpeed = 2f;
        [SerializeField] private float _shootRange = 12f;
        [SerializeField] private float _fireInterval = 1.5f;

        private Transform _patrolTarget;
        private float _fireCooldown;

        private void Awake()
        {
            _patrolTarget = _patrolPointA;
        }

        private void Update()
        {
            Patrol();
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

            transform.position = Vector3.MoveTowards(
                transform.position,
                _patrolTarget.position,
                _patrolSpeed * Time.deltaTime);

            if ((transform.position - _patrolTarget.position).sqrMagnitude < 0.01f)
            {
                _patrolTarget = _patrolTarget == _patrolPointA ? _patrolPointB : _patrolPointA;
            }
        }

        private void TryShoot()
        {
            if (_player == null || _projectilePrefab == null)
            {
                return;
            }

            Vector3 toPlayer = _player.position - transform.position;
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

            Fire(toPlayer);
            _fireCooldown = _fireInterval;
        }

        private void Fire(Vector3 direction)
        {
            if (direction.sqrMagnitude < 0.0001f)
            {
                direction = transform.forward;
            }

            Vector3 origin = _muzzle != null ? _muzzle.position : transform.position;
            EnemyProjectile projectile = Instantiate(_projectilePrefab, origin, Quaternion.identity);
            projectile.Launch(direction);
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
