using MarioTest.Core;
using UnityEngine;

namespace MarioTest.Player
{
    public sealed class GroundDetector
    {
        private readonly CapsuleCollider _capsule;
        private readonly GroundDetectionSettings _settings;
        private readonly RaycastHit[] _hits = new RaycastHit[1];

        public bool IsGrounded { get; private set; }
        public Vector3 GroundNormal { get; private set; } = Vector3.up;
        public float GroundDistance { get; private set; }
        public Collider GroundCollider { get; private set; }

        public GroundDetector(CapsuleCollider capsule, GroundDetectionSettings settings)
        {
            _capsule = capsule;
            _settings = settings;
        }

        public void Detect(Vector3 position, Quaternion rotation)
        {
            GroundCast cast = BuildCast(position, rotation);

            int hitCount = Physics.SphereCastNonAlloc(
                cast.Origin,
                cast.Radius,
                cast.Direction,
                _hits,
                cast.Distance,
                PhysicsLayers.GroundProbeMask,
                QueryTriggerInteraction.Ignore);

            if (hitCount > 0 && IsValidGroundHit(_hits[0], cast))
            {
                IsGrounded = true;
                GroundNormal = _hits[0].normal;
                GroundDistance = _hits[0].distance;
                GroundCollider = _hits[0].collider;
                return;
            }

            IsGrounded = false;
            GroundNormal = Vector3.up;
            GroundDistance = float.PositiveInfinity;
            GroundCollider = null;
        }

        private bool IsValidGroundHit(RaycastHit hit, GroundCast cast)
        {
            if (hit.collider == _capsule)
            {
                return false;
            }

            if (hit.rigidbody != null && hit.rigidbody == _capsule.attachedRigidbody)
            {
                return false;
            }

            if (hit.normal.y < _settings.MinGroundNormalY)
            {
                return false;
            }

            Vector3 supportPoint = hit.collider.ClosestPoint(cast.FeetPosition);
            float verticalGap = cast.FeetPosition.y - supportPoint.y;
            if (Mathf.Abs(verticalGap) > _settings.CheckDistance + _settings.ProbePadding)
            {
                return false;
            }

            Vector3 horizontalOffset = supportPoint - cast.FeetPosition;
            horizontalOffset.y = 0f;
            return horizontalOffset.sqrMagnitude <= cast.FootprintRadius * cast.FootprintRadius;
        }

        private GroundCast BuildCast(Vector3 position, Quaternion rotation)
        {
            Vector3 scale = _capsule.transform.lossyScale;
            Vector3 capsuleCenter = position + rotation * Vector3.Scale(_capsule.center, scale);

            (Vector3 up, float radiusScale, float heightScale) = GetCapsuleAxes(rotation);

            float fullRadius = _capsule.radius * radiusScale;
            float footprintRadius = fullRadius * _settings.MaxFootprintRadiusScale;
            float padding = _settings.ProbePadding;

            float halfHeight = Mathf.Max(_capsule.height * 0.5f * heightScale - fullRadius, 0f);
            Vector3 bottomCenter = capsuleCenter - up * halfHeight;
            Vector3 feetPosition = bottomCenter - up * fullRadius;

            return new GroundCast(
                feetPosition + up * padding,
                _settings.ProbeRadius,
                footprintRadius,
                feetPosition,
                -up,
                _settings.CheckDistance + padding);
        }

        private (Vector3 up, float radiusScale, float heightScale) GetCapsuleAxes(Quaternion rotation)
        {
            Vector3 scale = _capsule.transform.lossyScale;

            return _capsule.direction switch
            {
                0 => (
                    rotation * Vector3.right,
                    Mathf.Max(Mathf.Abs(scale.y), Mathf.Abs(scale.z)),
                    Mathf.Abs(scale.x)),
                2 => (
                    rotation * Vector3.forward,
                    Mathf.Max(Mathf.Abs(scale.x), Mathf.Abs(scale.y)),
                    Mathf.Abs(scale.z)),
                _ => (
                    rotation * Vector3.up,
                    Mathf.Max(Mathf.Abs(scale.x), Mathf.Abs(scale.z)),
                    Mathf.Abs(scale.y)),
            };
        }

        private readonly struct GroundCast
        {
            public GroundCast(
                Vector3 origin,
                float radius,
                float footprintRadius,
                Vector3 feetPosition,
                Vector3 direction,
                float distance)
            {
                Origin = origin;
                Radius = radius;
                FootprintRadius = footprintRadius;
                FeetPosition = feetPosition;
                Direction = direction;
                Distance = distance;
            }

            public Vector3 Origin { get; }
            public float Radius { get; }
            public float FootprintRadius { get; }
            public Vector3 FeetPosition { get; }
            public Vector3 Direction { get; }
            public float Distance { get; }
        }
    }
}
