using MarioTest.Core;
using UnityEngine;

namespace MarioTest.Player
{
    [RequireComponent(typeof(Rigidbody))]
    [RequireComponent(typeof(CapsuleCollider))]
    public class PlayerController : MonoBehaviour, IKnockbackReceiver
    {
        [SerializeField] private PlayerTuning _tuning;
        [SerializeField] private PlayerMovementSettings _movementSettings = new();
        [SerializeField] private GroundDetectionSettings _groundDetection = new();
        [SerializeField] private Transform _cameraTransform;

        [SerializeField] private bool _debugGround;

        private Rigidbody _rigidbody;
        private CapsuleCollider _capsule;
        private IPlayerInput _input;
        private PlayerMovement _movement;
        private GroundDetector _groundDetector;

        public bool IsGrounded => _groundDetector != null && _groundDetector.IsGrounded;
        public Vector3 GroundNormal => _groundDetector != null ? _groundDetector.GroundNormal : Vector3.up;

        public void ApplyKnockback(Vector3 velocity)
        {
            _movement?.ApplyKnockback(velocity);
            _rigidbody?.WakeUp();
        }

        public void Initialize(IPlayerInput input)
        {
            _input = input;
            _movement = new PlayerMovement(_tuning, _movementSettings);
        }

        private void Awake()
        {
            _rigidbody = GetComponent<Rigidbody>();
            _capsule = GetComponent<CapsuleCollider>();
            _groundDetector = new GroundDetector(_capsule, _groundDetection);
        }

        private void Update()
        {
            _movement.SetJumpInput(_input.JumpPressedThisFrame, _input.JumpHeld);

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
            _groundDetector.Detect(_rigidbody.position, _rigidbody.rotation);
            _movement.ApplyMovement(
                _rigidbody,
                Time.fixedDeltaTime,
                _groundDetector.IsGrounded,
                _groundDetector.GroundCollider);
            DrawGroundDebug();
        }

        private void DrawGroundDebug()
        {
            if (!_debugGround)
            {
                return;
            }

            Color color = _groundDetector.IsGrounded ? Color.green : Color.red;
            Vector3 origin = _rigidbody.position;
            Debug.DrawLine(origin, origin + Vector3.down * 2f, color, Time.fixedDeltaTime, false);
            Debug.DrawRay(origin, _groundDetector.GroundNormal, Color.blue, Time.fixedDeltaTime, false);
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
