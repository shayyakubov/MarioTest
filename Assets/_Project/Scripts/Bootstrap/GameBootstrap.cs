using MarioTest.Input;
using MarioTest.Player;
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

        private PlayerInputReader _inputReader;

        private void Awake()
        {
            _inputReader = new PlayerInputReader(_inputActions);
            _playerController.Initialize(_inputReader);

            if (_mobileTouchInput != null)
            {
                _mobileTouchInput.Initialize(_inputReader);
            }
        }

        private void OnEnable()
        {
            _inputReader.Enable();
        }

        private void OnDisable()
        {
            _inputReader.Disable();
        }

        private void Update()
        {
            _inputReader.Tick();
        }
    }
}
