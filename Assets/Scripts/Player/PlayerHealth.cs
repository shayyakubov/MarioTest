using System;
using MarioTest.Core;
using UnityEngine;

namespace MarioTest.Player
{
    [DisallowMultipleComponent]
    public sealed class PlayerHealth : MonoBehaviour, ILifeTarget
    {
        [SerializeField] private int _startingLives = 3;

        private int _lives;
        private bool _isDead;

        public int Lives => _lives;
        public bool IsDead => _isDead;
        public bool AcceptsHits { get; private set; } = true;

        public event Action<int> LivesChanged;
        public event Action Hit;
        public event Action Died;

        private void Awake()
        {
            _lives = _startingLives;
        }

        private void Start()
        {
            LivesChanged?.Invoke(_lives);
        }

        public void TakeHit(bool respawnAtCheckpoint = true)
        {
            if (_isDead || !AcceptsHits)
            {
                return;
            }

            _lives--;
            LivesChanged?.Invoke(_lives);

            if (_lives <= 0)
            {
                _isDead = true;
                Died?.Invoke();
                return;
            }

            if (respawnAtCheckpoint)
            {
                Hit?.Invoke();
            }
        }

        public void SetAcceptsHits(bool acceptsHits)
        {
            AcceptsHits = acceptsHits;
        }
    }
}
