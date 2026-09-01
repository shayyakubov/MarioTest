using MarioTest.Player;
using UnityEngine;
using UnityEngine.InputSystem;

namespace MarioTest.Bootstrap
{
    [DefaultExecutionOrder(-100)]
    public class GameBootstrap : MonoBehaviour
    {
        [SerializeField] private InputActionAsset _inputActions;
        [SerializeField] private PlayerController _playerController;

        private PlayerInputReader _inputReader;

        private void Awake()
        {
            _inputReader = new PlayerInputReader(_inputActions);
            _playerController.Initialize(_inputReader);
        }

        private void OnEnable()
        {
            _inputReader.Enable();
        }

        private void OnDisable()
        {
            _inputReader.Disable();
        }
    }
}
