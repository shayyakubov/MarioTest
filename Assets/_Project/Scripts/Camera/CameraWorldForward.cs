using UnityEngine;

namespace MarioTest.Camera
{
    public sealed class CameraWorldForward
    {
        private Vector3 _currentForward = Vector3.forward;
        private Vector3 _targetForward = Vector3.forward;
        private Vector3 _blendStartForward = Vector3.forward;
        private float _blendDuration;
        private float _blendElapsed;
        private bool _isBlending;

        public Vector3 CurrentForward => _currentForward;
        public Vector3 TargetForward => _targetForward;
        public bool IsBlending => _isBlending;

        public void SnapTo(Vector3 forward)
        {
            _currentForward = NormalizeForward(forward);
            _targetForward = _currentForward;
            _blendStartForward = _currentForward;
            _isBlending = false;
            _blendElapsed = 0f;
        }

        public void BlendTo(Vector3 forward, float duration)
        {
            _targetForward = NormalizeForward(forward);

            if (duration <= 0f)
            {
                SnapTo(forward);
                return;
            }

            _blendStartForward = _currentForward;
            _blendDuration = duration;
            _blendElapsed = 0f;
            _isBlending = true;
        }

        public void Tick(float deltaTime)
        {
            if (!_isBlending)
            {
                return;
            }

            _blendElapsed += deltaTime;
            float blendProgress = Mathf.Clamp01(_blendElapsed / _blendDuration);
            float easedProgress = Mathf.SmoothStep(0f, 1f, blendProgress);

            float startYaw = ForwardToYaw(_blendStartForward);
            float targetYaw = ForwardToYaw(_targetForward);
            float blendedYaw = Mathf.LerpAngle(startYaw, targetYaw, easedProgress);
            _currentForward = YawToForward(blendedYaw);

            if (blendProgress >= 1f)
            {
                _currentForward = _targetForward;
                _isBlending = false;
            }
        }

        private static Vector3 NormalizeForward(Vector3 forward)
        {
            forward.y = 0f;

            if (forward.sqrMagnitude < 0.0001f)
            {
                return Vector3.forward;
            }

            return forward.normalized;
        }

        private static float ForwardToYaw(Vector3 forward)
        {
            return Mathf.Atan2(forward.x, forward.z) * Mathf.Rad2Deg;
        }

        private static Vector3 YawToForward(float yawDegrees)
        {
            float yawRadians = yawDegrees * Mathf.Deg2Rad;
            return new Vector3(Mathf.Sin(yawRadians), 0f, Mathf.Cos(yawRadians));
        }
    }
}
