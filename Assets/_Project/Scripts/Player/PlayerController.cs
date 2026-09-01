using UnityEngine;

namespace MarioTest.Player
{
    [RequireComponent(typeof(Rigidbody))]
    public class PlayerController : MonoBehaviour
    {
        [SerializeField] private PlayerTuning _tuning;
        [SerializeField] private Transform _cameraTransform;

        private Rigidbody _rigidbody;
        private IPlayerInput _input;
        private PlayerMovement _movement;

        public void Initialize(IPlayerInput input)
        {
            _input = input;
            _movement = new PlayerMovement(_tuning);
        }

        private void Awake()
        {
            _rigidbody = GetComponent<Rigidbody>();
        }

        private void Update()
        {
            Vector2 input = _input.Move;
            Vector3 worldDirection = GetCameraRelativeDirection(input);

            float magnitude = worldDirection.magnitude;
            if (magnitude < _tuning.MoveInputDeadzone)
            {
                _movement.SetMoveInput(Vector3.zero, 0f);
                return;
            }

            _movement.SetMoveInput(worldDirection / magnitude, Mathf.Min(magnitude, 1f));
        }

        private void FixedUpdate()
        {
            _movement.ApplyMovement(_rigidbody, Time.fixedDeltaTime);
        }

        private Vector3 GetCameraRelativeDirection(Vector2 input)
        {
            if (input.sqrMagnitude < 0.0001f)
            {
                return Vector3.zero;
            }

            if (_cameraTransform == null)
            {
                return new Vector3(input.x, 0f, input.y);
            }

            Vector3 forward = _cameraTransform.forward;
            Vector3 right = _cameraTransform.right;
            forward.y = 0f;
            right.y = 0f;
            forward.Normalize();
            right.Normalize();

            return right * input.x + forward * input.y;
        }
    }
}
