using MarioTest.Core;
using MarioTest.UI;
using UnityEngine;

namespace MarioTest.Interaction
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Collider))]
    public sealed class CoinPickup : MonoBehaviour
    {
        private void Reset()
        {
            Collider collider = GetComponent<Collider>();
            collider.isTrigger = true;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.attachedRigidbody == null || other.gameObject.layer != PhysicsLayers.PlayerLayer)
            {
                return;
            }

            CoinsHud coinsHud = Object.FindAnyObjectByType<CoinsHud>();
            coinsHud?.AddCoin();
            Destroy(gameObject);
        }
    }
}
