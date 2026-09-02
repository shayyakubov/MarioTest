using System;
using MarioTest.Interaction;
using UnityEngine;

namespace MarioTest.Systems
{
    public sealed class PickupsManager : MonoBehaviour
    {
        [SerializeField] private CoinPickup[] _coinPickups = System.Array.Empty<CoinPickup>();

        public event Action CoinCollected;

        private void Start()
        {
            SubscribeToPickups();
        }

        private void OnDestroy()
        {
            UnsubscribeFromPickups();
        }

        private void SubscribeToPickups()
        {
            for (int i = 0; i < _coinPickups.Length; i++)
            {
                CoinPickup pickup = _coinPickups[i];
                if (pickup != null)
                {
                    pickup.Collected += OnCoinCollected;
                }
            }
        }

        private void UnsubscribeFromPickups()
        {
            for (int i = 0; i < _coinPickups.Length; i++)
            {
                CoinPickup pickup = _coinPickups[i];
                if (pickup != null)
                {
                    pickup.Collected -= OnCoinCollected;
                }
            }
        }

        private void OnCoinCollected(CoinPickup pickup)
        {
            if (pickup == null)
            {
                return;
            }

            pickup.Collected -= OnCoinCollected;
            CoinCollected?.Invoke();
            Destroy(pickup.gameObject);
        }
    }
}
