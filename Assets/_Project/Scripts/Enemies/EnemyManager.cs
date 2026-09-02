using System.Collections.Generic;
using UnityEngine;

namespace MarioTest.Enemies
{
    [DefaultExecutionOrder(-9)]
    public sealed class EnemyManager : MonoBehaviour
    {
        [SerializeField] private Transform _targetTransform;
        [SerializeField] private Rigidbody _targetRigidbody;
        [SerializeField] private List<MonoBehaviour> _enemies = new();

        private void Awake()
        {
            InitializeEnemies();
        }

        private void OnValidate()
        {
#if UNITY_EDITOR
            if (Application.isPlaying)
            {
                return;
            }

            RefreshEnemyList();
#endif
        }

        public void RefreshEnemyList()
        {
#if UNITY_EDITOR
            _enemies.Clear();

            MonoBehaviour[] behaviours = FindObjectsByType<MonoBehaviour>(
                FindObjectsInactive.Include,
                FindObjectsSortMode.None);

            foreach (MonoBehaviour behaviour in behaviours)
            {
                if (behaviour is not IEnemy || behaviour.gameObject.scene != gameObject.scene)
                {
                    continue;
                }

                _enemies.Add(behaviour);
            }
#endif
        }

        private void InitializeEnemies()
        {
            for (int i = 0; i < _enemies.Count; i++)
            {
                if (_enemies[i] is IEnemy enemy)
                {
                    enemy.Initialize(_targetTransform, _targetRigidbody);
                }
            }
        }
    }
}
