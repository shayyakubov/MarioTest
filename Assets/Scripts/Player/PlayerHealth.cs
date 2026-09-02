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

        public void TakeHit()
        {
            if (_isDead || !AcceptsHits)
            {
                Debug.Log($"[PlayerHealth] TakeHit ignored — isDead={_isDead}, acceptsHits={AcceptsHits}, lives={_lives}");
                return;
            }

            _lives--;
            Debug.Log($"[PlayerHealth] TakeHit — {_lives} lives left");
            LivesChanged?.Invoke(_lives);

            if (_lives <= 0)
            {
                _isDead = true;
                Debug.Log("[PlayerHealth] Died — invoking Died");
                Died?.Invoke();
                return;
            }

            Debug.Log("[PlayerHealth] Hit — invoking Hit");
            Hit?.Invoke();
        }

        public void SetAcceptsHits(bool acceptsHits)
        {
            Debug.Log($"[PlayerHealth] SetAcceptsHits {AcceptsHits} → {acceptsHits} (lives={_lives}, isDead={_isDead})");
            AcceptsHits = acceptsHits;
        }
    }
}
