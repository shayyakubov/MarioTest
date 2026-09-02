using UnityEngine;
using UnityEngine.UI;

namespace MarioTest.UI
{
    public sealed class LivesHud : MonoBehaviour
    {
        [SerializeField] private Image[] _lifePips;
        [SerializeField] private Color _activeColor = new Color(0.95f, 0.2f, 0.2f, 0.95f);
        [SerializeField] private Color _lostColor = new Color(0.35f, 0.35f, 0.35f, 0.45f);

        public void SetLives(int livesRemaining)
        {
            if (_lifePips == null)
            {
                return;
            }

            for (int i = 0; i < _lifePips.Length; i++)
            {
                Image pip = _lifePips[i];
                if (pip == null)
                {
                    continue;
                }

                pip.color = i < livesRemaining ? _activeColor : _lostColor;
            }
        }
    }
}
