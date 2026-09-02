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
        [SerializeField] private PlayerHealth _playerHealth;
        [SerializeField] private Rigidbody _playerRigidbody;
        [SerializeField] private MobileTouchInput _mobileTouchInput;
        [SerializeField] private GameSession _gameSession;
        [SerializeField] private CheckpointsManager _checkpointsManager;
        [SerializeField] private PickupsManager _pickupsManager;
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
            if (_gameSession == null
                || _playerController == null
                || _playerHealth == null
                || _playerRigidbody == null)
            {
                return;
            }

            _gameSession.Initialize(
                _playerController,
                _playerHealth,
                _playerRigidbody,
                _checkpointsManager,
                _pickupsManager,
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
