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
        public int GroundLayer { get; private set; } = -1;

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
                PhysicsLayers.GroundMask,
                QueryTriggerInteraction.Ignore);

            if (hitCount > 0
                && _hits[0].normal.y >= _settings.MinGroundNormalY
                && IsHitUnderFeet(cast.BottomCenter, cast.Radius, _hits[0].point))
            {
                IsGrounded = true;
                GroundNormal = _hits[0].normal;
                GroundDistance = _hits[0].distance;
                GroundLayer = _hits[0].collider.gameObject.layer;
                return;
            }

            IsGrounded = false;
            GroundNormal = Vector3.up;
            GroundDistance = float.PositiveInfinity;
            GroundLayer = -1;
        }

        private static bool IsHitUnderFeet(Vector3 bottomCenter, float radius, Vector3 hitPoint)
        {
            Vector3 horizontalOffset = hitPoint - bottomCenter;
            horizontalOffset.y = 0f;
            return horizontalOffset.sqrMagnitude <= radius * radius;
        }

        private GroundCast BuildCast(Vector3 position, Quaternion rotation)
        {
            Vector3 scale = _capsule.transform.lossyScale;
            Vector3 capsuleCenter = position + rotation * Vector3.Scale(_capsule.center, scale);

            (Vector3 up, float radiusScale, float heightScale) = GetCapsuleAxes(rotation);

            float fullRadius = _capsule.radius * radiusScale;
            float radius = Mathf.Max(fullRadius - _settings.SkinWidth, 0.01f);

            float halfHeight = Mathf.Max(_capsule.height * 0.5f * heightScale - fullRadius, 0f);
            Vector3 bottomCenter = capsuleCenter - up * halfHeight;

            float skin = _settings.SkinWidth;
            return new GroundCast(
                bottomCenter,
                bottomCenter + up * skin,
                radius,
                -up,
                _settings.CheckDistance + skin);
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
            public GroundCast(Vector3 bottomCenter, Vector3 origin, float radius, Vector3 direction, float distance)
            {
                BottomCenter = bottomCenter;
                Origin = origin;
                Radius = radius;
                Direction = direction;
                Distance = distance;
            }

            public Vector3 BottomCenter { get; }
            public Vector3 Origin { get; }
            public float Radius { get; }
            public Vector3 Direction { get; }
            public float Distance { get; }
        }
    }
}
