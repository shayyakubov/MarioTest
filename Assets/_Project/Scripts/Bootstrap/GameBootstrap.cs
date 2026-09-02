using MarioTest.Camera;
using MarioTest.Input;
using MarioTest.Interaction;
using MarioTest.Player;
using MarioTest.Systems;
using MarioTest.UI;
using UnityEngine;
using UnityEngine.InputSystem;

namespace MarioTest.Bootstrap
{
    [DefaultExecutionOrder(-10)]
    public class GameBootstrap : MonoBehaviour
    {
        [SerializeField] private InputActionAsset _inputActions;
        [SerializeField] private PlayerController _playerController;
        [SerializeField] private MobileTouchInput _mobileTouchInput;
        [SerializeField] private GameSession _gameSession;
        [SerializeField] private GameHud _gameHud;
        [SerializeField] private FollowCameraController _followCamera;
        [SerializeField] private GoalTrigger _goalTrigger;

        private PlayerInputReader _inputReader;

        private void Awake()
        {
            _inputReader = new PlayerInputReader(_inputActions);
            _inputReader.Enable();
            _playerController.Initialize(_inputReader);

            if (_mobileTouchInput != null)
            {
                _mobileTouchInput.Initialize(_inputReader);
            }

            InitializeGameSession();
        }

        private void InitializeGameSession()
        {
            if (_gameSession == null || _playerController == null)
            {
                return;
            }

            PlayerHealth playerHealth = _playerController.GetComponent<PlayerHealth>();
            Rigidbody playerRigidbody = _playerController.GetComponent<Rigidbody>();
            CheckpointsManager checkpointsManager = _gameSession.GetComponent<CheckpointsManager>();

            if (playerHealth == null || playerRigidbody == null)
            {
                return;
            }

            _gameSession.Initialize(
                _playerController,
                playerHealth,
                playerRigidbody,
                checkpointsManager,
                _followCamera,
                _gameHud,
                _goalTrigger);
        }

        private void Update()
        {
            _inputReader?.Tick();
        }
    }
}
