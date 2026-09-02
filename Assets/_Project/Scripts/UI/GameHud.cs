using System;
using MarioTest.Player;
using UnityEngine;

namespace MarioTest.UI
{
    public sealed class GameHud : MonoBehaviour
    {
        [SerializeField] private LivesHud _livesHud;
        [SerializeField] private GameOverOverlay _gameOverOverlay;
        [SerializeField] private GameOverOverlay _courseWinOverlay;

        private PlayerHealth _playerHealth;

        public event Action RestartRequested;

        private void Awake()
        {
            SubscribeOverlay(_gameOverOverlay);
            SubscribeOverlay(_courseWinOverlay);
        }

        private void OnDestroy()
        {
            UnsubscribeOverlay(_gameOverOverlay);
            UnsubscribeOverlay(_courseWinOverlay);
            Unsubscribe();
        }

        public void Subscribe(PlayerHealth playerHealth)
        {
            Unsubscribe();
            _playerHealth = playerHealth;

            if (_playerHealth == null)
            {
                return;
            }

            _playerHealth.LivesChanged += OnLivesChanged;
            _playerHealth.Died += OnDied;
            Debug.Log("[GameHud] Subscribe");
        }

        public void ResetForRestart()
        {
            _gameOverOverlay?.Hide();
            _courseWinOverlay?.Hide();

            if (_playerHealth != null)
            {
                _livesHud?.SetLives(_playerHealth.Lives);
            }
        }

        public void ShowCourseWin()
        {
            _courseWinOverlay?.Show();
        }

        private void Unsubscribe()
        {
            if (_playerHealth == null)
            {
                return;
            }

            _playerHealth.LivesChanged -= OnLivesChanged;
            _playerHealth.Died -= OnDied;
        }

        private void SubscribeOverlay(GameOverOverlay overlay)
        {
            if (overlay != null)
            {
                overlay.RestartRequested += OnRestartRequested;
            }
        }

        private void UnsubscribeOverlay(GameOverOverlay overlay)
        {
            if (overlay != null)
            {
                overlay.RestartRequested -= OnRestartRequested;
            }
        }

        private void OnLivesChanged(int livesRemaining)
        {
            Debug.Log($"[GameHud] LivesChanged — {livesRemaining}");
            _livesHud?.SetLives(livesRemaining);
        }

        private void OnDied()
        {
            Debug.Log($"[GameHud] OnDied — overlay={(_gameOverOverlay != null ? _gameOverOverlay.name : "null")}");
            _gameOverOverlay?.Show();
        }

        private void OnRestartRequested()
        {
            RestartRequested?.Invoke();
        }
    }
}
