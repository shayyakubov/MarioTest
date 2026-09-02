using System.Collections;
using MarioTest.Camera;
using MarioTest.Core;
using MarioTest.Interaction;
using MarioTest.Player;
using MarioTest.UI;
using UnityEngine;

namespace MarioTest.Systems
{
    public sealed class GameSession : MonoBehaviour
    {
        [SerializeField] private float _respawnDelay = 0.5f;

        private readonly PlayerRespawn _playerRespawn = new();
        private PlayerController _playerController;
        private PlayerHealth _playerHealth;
        private Rigidbody _playerRigidbody;
        private CheckpointsManager _checkpointsManager;
        private FollowCameraController _followCamera;
        private GameHud _gameHud;
        private GoalTrigger _goalTrigger;
        private bool _isRespawning;
        private bool _initialized;

        public void Initialize(
            PlayerController playerController,
            PlayerHealth playerHealth,
            Rigidbody playerRigidbody,
            CheckpointsManager checkpointsManager,
            FollowCameraController followCamera,
            GameHud gameHud,
            GoalTrigger goalTrigger)
        {
            if (_initialized)
            {
                return;
            }

            if (playerController == null || playerHealth == null || playerRigidbody == null)
            {
                return;
            }

            _playerController = playerController;
            _playerHealth = playerHealth;
            _playerRigidbody = playerRigidbody;
            _checkpointsManager = checkpointsManager;
            _followCamera = followCamera;
            _gameHud = gameHud;
            _goalTrigger = goalTrigger;

            Subscribe();
            _initialized = true;
        }

        private void OnDestroy()
        {
            Unsubscribe();
        }

        public void RestartRun()
        {
            _gameHud?.ResetForRestart();

            UnityEngine.SceneManagement.SceneManager.LoadScene(
                UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex);
        }

        private void Subscribe()
        {
            _playerHealth.Hit += OnHit;
            _playerHealth.Died += OnDied;

            if (_gameHud != null)
            {
                _gameHud.RestartRequested += RestartRun;
            }

            if (_goalTrigger != null)
            {
                _goalTrigger.CourseReached += OnCourseCompleted;
            }
        }

        private void Unsubscribe()
        {
            if (_playerHealth != null)
            {
                _playerHealth.Hit -= OnHit;
                _playerHealth.Died -= OnDied;
            }

            if (_gameHud != null)
            {
                _gameHud.RestartRequested -= RestartRun;
            }

            if (_goalTrigger != null)
            {
                _goalTrigger.CourseReached -= OnCourseCompleted;
            }
        }

        private void OnCourseCompleted()
        {
            SetPlayerControlEnabled(false);
            _playerController.StopMotion();
            _gameHud?.ShowCourseWin();
        }

        private void OnHit()
        {
            if (_isRespawning)
            {
                return;
            }

            _isRespawning = true;
            _playerHealth.SetAcceptsHits(false);
            SetPlayerControlEnabled(false);
            StartCoroutine(RespawnRoutine());
        }

        private void OnDied()
        {
            StopAllCoroutines();
            _isRespawning = false;
            _playerHealth.SetAcceptsHits(false);
            SetPlayerControlEnabled(false);
            _playerController.StopMotion();
        }

        private void SetPlayerControlEnabled(bool enabled)
        {
            if (_playerController != null)
            {
                _playerController.enabled = enabled;
            }
        }

        private IEnumerator RespawnRoutine()
        {
            yield return new WaitForSeconds(_respawnDelay);

            if (!_playerHealth.IsDead)
            {
                _playerRespawn.RestoreWorld();
                _playerRespawn.TeleportTo(
                    _checkpointsManager.GetSpawnPoint(),
                    _playerRigidbody,
                    _playerController,
                    _followCamera);
            }

            FinishRespawn();
        }

        private void FinishRespawn()
        {
            _isRespawning = false;

            if (_playerHealth != null && !_playerHealth.IsDead)
            {
                _playerHealth.SetAcceptsHits(true);
                SetPlayerControlEnabled(true);
            }
        }
    }
}
