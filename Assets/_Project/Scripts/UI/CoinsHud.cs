using UnityEngine;
using UnityEngine.UI;

namespace MarioTest.UI
{
    public sealed class CoinsHud : MonoBehaviour
    {
        [SerializeField] private Text _countText;

        private int _count;

        public void ResetCount()
        {
            _count = 0;
            Refresh();
        }

        public void AddCoin()
        {
            _count++;
            Refresh();
        }

        private void Refresh()
        {
            if (_countText != null)
            {
                _countText.text = _count.ToString();
            }
        }
    }
}
