using UnityEngine;

namespace MarioTest.Camera
{
    public sealed class FollowCamera
    {
        private readonly CameraTuning _tuning;
        private Vector3 _positionVelocity;
        private bool _initialized;

        public FollowCamera(CameraTuning tuning)
        {
            _tuning = tuning;
        }

        public void SnapToTarget(
            Transform cameraTransform,
            Vector3 targetPosition,
            Vector3 horizontalVelocity,
            Vector3 worldForward,
            float leadReferenceSpeed)
        {
            ApplyPose(cameraTransform, targetPosition, horizontalVelocity, worldForward, leadReferenceSpeed, 0f, true);
            _positionVelocity = Vector3.zero;
            _initialized = true;
        }

        public void Tick(
            Transform cameraTransform,
            Vector3 targetPosition,
            Vector3 horizontalVelocity,
            Vector3 worldForward,
            float leadReferenceSpeed,
            float deltaTime)
        {
            if (!_initialized)
            {
                SnapToTarget(cameraTransform, targetPosition, horizontalVelocity, worldForward, leadReferenceSpeed);
                return;
            }

            ApplyPose(cameraTransform, targetPosition, horizontalVelocity, worldForward, leadReferenceSpeed, deltaTime, false);
        }

        private void ApplyPose(
            Transform cameraTransform,
            Vector3 targetPosition,
            Vector3 horizontalVelocity,
            Vector3 worldForward,
            float leadReferenceSpeed,
            float deltaTime,
            bool snap)
        {
            Vector3 flatForward = worldForward;
            flatForward.y = 0f;
            flatForward.Normalize();

            Vector3 pivot = targetPosition + Vector3.up * _tuning.PivotHeight;
            Vector3 lead = ComputeLead(horizontalVelocity, flatForward, leadReferenceSpeed);
            Vector3 lookTarget = pivot + lead;

            float yaw = Mathf.Atan2(flatForward.x, flatForward.z) * Mathf.Rad2Deg;
            Vector3 backOffset = -flatForward * _tuning.Distance;
            Vector3 desiredPosition = lookTarget + backOffset + Vector3.up * _tuning.Height;
            float maxSpeed = _tuning.MaxCameraSpeed > 0f ? _tuning.MaxCameraSpeed : Mathf.Infinity;

            Vector3 currentPosition = cameraTransform.position;
            Vector3 smoothedPosition;

            if (snap)
            {
                smoothedPosition = desiredPosition;
            }
            else
            {
                float smoothX = Mathf.SmoothDamp(
                    currentPosition.x,
                    desiredPosition.x,
                    ref _positionVelocity.x,
                    _tuning.SmoothTimeHorizontal,
                    maxSpeed,
                    deltaTime);

                float smoothZ = Mathf.SmoothDamp(
                    currentPosition.z,
                    desiredPosition.z,
                    ref _positionVelocity.z,
                    _tuning.SmoothTimeHorizontal,
                    maxSpeed,
                    deltaTime);

                float smoothY = Mathf.SmoothDamp(
                    currentPosition.y,
                    desiredPosition.y,
                    ref _positionVelocity.y,
                    _tuning.SmoothTimeVertical,
                    maxSpeed,
                    deltaTime);

                smoothedPosition = new Vector3(smoothX, smoothY, smoothZ);
            }

            cameraTransform.position = smoothedPosition;
            cameraTransform.rotation = Quaternion.Euler(_tuning.Pitch, yaw, 0f);
        }

        private Vector3 ComputeLead(Vector3 horizontalVelocity, Vector3 worldForward, float leadReferenceSpeed)
        {
            float forwardSpeed = Vector3.Dot(horizontalVelocity, worldForward);

            if (Mathf.Abs(forwardSpeed) < _tuning.LeadMinSpeed || leadReferenceSpeed <= 0f)
            {
                return Vector3.zero;
            }

            float leadScale = Mathf.Clamp01(Mathf.Abs(forwardSpeed) / leadReferenceSpeed);
            return worldForward * (Mathf.Sign(forwardSpeed) * _tuning.LeadDistance * leadScale);
        }
    }
}
