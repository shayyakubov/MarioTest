using System;
using MarioTest.Player;
using MarioTest.Systems;
using UnityEngine;

namespace MarioTest.UI
{
    public sealed class GameHud : MonoBehaviour
    {
        [SerializeField] private LivesHud _livesHud;
        [SerializeField] private CoinsHud _coinsHud;
        [SerializeField] private GameOverOverlay _gameOverOverlay;
        [SerializeField] private GameOverOverlay _courseWinOverlay;

        private PlayerHealth _playerHealth;
        private PickupsManager _pickupsManager;

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
            UnsubscribeHealth();
            UnsubscribePickups();
        }

        public void Initialize(PlayerHealth playerHealth, PickupsManager pickupsManager)
        {
            UnsubscribeHealth();
            UnsubscribePickups();

            _playerHealth = playerHealth;
            _pickupsManager = pickupsManager;

            if (_playerHealth != null)
            {
                _playerHealth.LivesChanged += OnLivesChanged;
                _playerHealth.Died += OnDied;
                _livesHud?.SetLives(_playerHealth.Lives);
            }

            if (_pickupsManager != null)
            {
                _pickupsManager.CoinCollected += OnCoinCollected;
            }
        }

        public void ResetForRestart()
        {
            _gameOverOverlay?.Hide();
            _courseWinOverlay?.Hide();

            if (_playerHealth != null)
            {
                _livesHud?.SetLives(_playerHealth.Lives);
            }

            _coinsHud?.ResetCount();
        }

        public void ShowCourseWin()
        {
            _courseWinOverlay?.Show();
        }

        private void UnsubscribeHealth()
        {
            if (_playerHealth == null)
            {
                return;
            }

            _playerHealth.LivesChanged -= OnLivesChanged;
            _playerHealth.Died -= OnDied;
            _playerHealth = null;
        }

        private void UnsubscribePickups()
        {
            if (_pickupsManager == null)
            {
                return;
            }

            _pickupsManager.CoinCollected -= OnCoinCollected;
            _pickupsManager = null;
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
            _livesHud?.SetLives(livesRemaining);
        }

        private void OnCoinCollected()
        {
            _coinsHud?.AddCoin();
        }

        private void OnDied()
        {
            _gameOverOverlay?.Show();
        }

        private void OnRestartRequested()
        {
            RestartRequested?.Invoke();
        }
    }
}
