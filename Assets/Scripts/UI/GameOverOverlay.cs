using System;
using UnityEngine;
using UnityEngine.UI;

namespace MarioTest.UI
{
    public sealed class GameOverOverlay : MonoBehaviour
    {
        [SerializeField] private Button _restartButton;

        public event Action RestartRequested;

        private void Awake()
        {
            if (_restartButton != null)
            {
                _restartButton.onClick.AddListener(OnRestartClicked);
            }
        }

        private void OnDestroy()
        {
            if (_restartButton != null)
            {
                _restartButton.onClick.RemoveListener(OnRestartClicked);
            }
        }

        public void Show()
        {
            gameObject.SetActive(true);
        }

        public void Hide()
        {
            gameObject.SetActive(false);
        }

        private void OnRestartClicked()
        {
            RestartRequested?.Invoke();
        }
    }
}
