using MarioTest.Camera;
using MarioTest.Core;
using MarioTest.Interaction;
using MarioTest.Platforms;
using MarioTest.Player;
using UnityEngine;

namespace MarioTest.Systems
{
    public sealed class PlayerRespawn
    {
        public void RestoreWorld()
        {
            ResetRestorables<CrumblePlatformBehaviour>();
            ResetRestorables<PushableBehaviour>();
        }

        public void TeleportTo(
            Transform spawnPoint,
            Rigidbody playerRigidbody,
            PlayerController playerController,
            FollowCameraController followCamera)
        {
            if (spawnPoint == null || playerRigidbody == null || playerController == null)
            {
                return;
            }

            playerRigidbody.position = spawnPoint.position;
            playerRigidbody.linearVelocity = Vector3.zero;
            playerRigidbody.angularVelocity = Vector3.zero;
            playerController.Reset();
            followCamera?.SnapToTarget();
        }

        private static void ResetRestorables<T>() where T : MonoBehaviour, IWorldRestorable
        {
            T[] restorables = Object.FindObjectsByType<T>(FindObjectsSortMode.None);
            for (int i = 0; i < restorables.Length; i++)
            {
                restorables[i].ResetToInitialState();
            }
        }
    }
}
