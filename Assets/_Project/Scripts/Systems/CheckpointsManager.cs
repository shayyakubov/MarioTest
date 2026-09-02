using MarioTest.Interaction;
using UnityEngine;

namespace MarioTest.Systems
{
    public sealed class CheckpointsManager : MonoBehaviour
    {
        [SerializeField] private Transform _defaultCheckpoint;

        [SerializeField] private CheckpointTrigger[] _checkpointTriggers = System.Array.Empty<CheckpointTrigger>();

        private Transform _activeCheckpoint;

        private void Start()
        {
            _activeCheckpoint = _defaultCheckpoint;
            SubscribeToTriggers();
        }

        private void OnDestroy()
        {
            UnsubscribeFromTriggers();
        }

        public Transform GetSpawnPoint()
        {
            if (_activeCheckpoint != null)
            {
                return _activeCheckpoint;
            }

            return _defaultCheckpoint;
        }

        private void SubscribeToTriggers()
        {
            for (int i = 0; i < _checkpointTriggers.Length; i++)
            {
                CheckpointTrigger trigger = _checkpointTriggers[i];
                if (trigger != null)
                {
                    trigger.Activated += OnCheckpointActivated;
                }
            }
        }

        private void UnsubscribeFromTriggers()
        {
            for (int i = 0; i < _checkpointTriggers.Length; i++)
            {
                CheckpointTrigger trigger = _checkpointTriggers[i];
                if (trigger != null)
                {
                    trigger.Activated -= OnCheckpointActivated;
                }
            }
        }

        private void OnCheckpointActivated(CheckpointTrigger trigger)
        {
            if (trigger == null)
            {
                return;
            }

            _activeCheckpoint = trigger.SpawnPoint;
        }
    }
}
