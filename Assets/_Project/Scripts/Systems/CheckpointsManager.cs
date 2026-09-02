using UnityEngine;

namespace MarioTest.Systems
{
    public sealed class CheckpointsManager : MonoBehaviour
    {
        [SerializeField] private Transform _defaultCheckpoint;
        [SerializeField] private Transform _playerFallback;

        private Transform _activeCheckpoint;

        private void Start()
        {
            _activeCheckpoint = _defaultCheckpoint != null ? _defaultCheckpoint : _playerFallback;
        }

        public void SetCheckpoint(Transform checkpoint)
        {
            if (checkpoint == null)
            {
                return;
            }

            _activeCheckpoint = checkpoint;
        }

        public Transform GetSpawnPoint()
        {
            if (_activeCheckpoint != null)
            {
                return _activeCheckpoint;
            }

            return _playerFallback;
        }
    }
}
