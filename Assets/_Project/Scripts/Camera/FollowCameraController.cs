using UnityEngine;

namespace MarioTest.Camera
{
    [DefaultExecutionOrder(10)]
    public class FollowCameraController : MonoBehaviour
    {
        [SerializeField] private Transform _target;
        [SerializeField] private CameraTuning _tuning = new();
        [SerializeField] private Vector3 _initialWorldForward = Vector3.forward;
        [SerializeField] private float _defaultForwardBlendTime = 1f;
        [SerializeField] private float _leadReferenceSpeed = 6f;

        private FollowCamera _followCamera;
        private CameraWorldForward _worldForward;
        private Rigidbody _targetRigidbody;

        public Vector3 WorldForward => _worldForward.CurrentForward;

        private void Awake()
        {
            _followCamera = new FollowCamera(_tuning);
            _worldForward = new CameraWorldForward();
            _worldForward.SnapTo(_initialWorldForward);

            if (_target != null)
            {
                _targetRigidbody = _target.GetComponent<Rigidbody>();
            }
        }

        private void Start()
        {
            if (_target == null)
            {
                return;
            }

            _followCamera.SnapToTarget(
                transform,
                _target.position,
                GetHorizontalVelocity(),
                _worldForward.CurrentForward,
                _leadReferenceSpeed);
        }

        private void LateUpdate()
        {
            if (_target == null)
            {
                return;
            }

            _worldForward.Tick(Time.deltaTime);

            _followCamera.Tick(
                transform,
                _target.position,
                GetHorizontalVelocity(),
                _worldForward.CurrentForward,
                _leadReferenceSpeed,
                Time.deltaTime);
        }

        public void SetWorldForward(Vector3 forward)
        {
            _worldForward.SnapTo(forward);
        }

        public void BlendWorldForward(Vector3 forward, float duration = -1f)
        {
            float blendTime = duration >= 0f ? duration : _defaultForwardBlendTime;
            _worldForward.BlendTo(forward, blendTime);
        }

        public void SnapToTarget()
        {
            if (_target == null)
            {
                return;
            }

            _followCamera.SnapToTarget(
                transform,
                _target.position,
                GetHorizontalVelocity(),
                _worldForward.CurrentForward,
                _leadReferenceSpeed);
        }

        private Vector3 GetHorizontalVelocity()
        {
            if (_targetRigidbody == null)
            {
                return Vector3.zero;
            }

            Vector3 velocity = _targetRigidbody.linearVelocity;
            return new Vector3(velocity.x, 0f, velocity.z);
        }

        private void OnValidate()
        {
            if (_target != null && _targetRigidbody == null)
            {
                _targetRigidbody = _target.GetComponent<Rigidbody>();
            }

            _initialWorldForward.y = 0f;
            if (_initialWorldForward.sqrMagnitude < 0.0001f)
            {
                _initialWorldForward = Vector3.forward;
            }
        }
    }
}
