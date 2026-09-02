using MarioTest.Core;
using UnityEngine;

namespace MarioTest.Platforms
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Collider))]
    [RequireComponent(typeof(CrumblePlatformVisual))]
    public sealed class CrumblePlatformBehaviour : MonoBehaviour, IWorldRestorable
    {
        [SerializeField] private float _standDuration = 0.5f;
        [SerializeField] private float _warnDuration = 0.25f;
        [SerializeField] private float _occupancyVerticalTolerance = 0.2f;
        [SerializeField] private float _occupancyBoxVerticalPadding = 0.25f;
        [SerializeField] private float _occupancyBoxHorizontalScale = 0.85f;

        private Collider _collider;
        private CrumblePlatformVisual _visual;
        private readonly CrumblePlatformState _state = new();
        private readonly Collider[] _overlapResults = new Collider[4];
        private bool _isOccupied;

        private void Awake()
        {
            _collider = GetComponent<Collider>();
            _visual = GetComponent<CrumblePlatformVisual>();
        }

        private void FixedUpdate()
        {
            _isOccupied = DetectOccupancy();
            _state.Tick(Time.fixedDeltaTime, _isOccupied, _standDuration, _warnDuration);

            if (_state.Phase == CrumblePlatformPhase.Crumbled)
            {
                _collider.enabled = false;
            }
        }

        private void Update()
        {
            _visual.Apply(_state.Phase, _isOccupied, Time.deltaTime);
        }

        public void ResetToInitialState()
        {
            _state.Reset();
            _collider.enabled = true;
            _visual.ResetVisual();
        }

        private bool DetectOccupancy()
        {
            if (_state.IsCrumbled)
            {
                return false;
            }

            Bounds bounds = _collider.bounds;
            Vector3 halfExtents = new Vector3(
                bounds.extents.x * _occupancyBoxHorizontalScale,
                _occupancyBoxVerticalPadding,
                bounds.extents.z * _occupancyBoxHorizontalScale);
            Vector3 center = new Vector3(
                bounds.center.x,
                bounds.max.y + _occupancyBoxVerticalPadding,
                bounds.center.z);

            int count = Physics.OverlapBoxNonAlloc(
                center,
                halfExtents,
                _overlapResults,
                transform.rotation,
                Physics.DefaultRaycastLayers,
                QueryTriggerInteraction.Ignore);

            for (int i = 0; i < count; i++)
            {
                Collider hit = _overlapResults[i];
                if (hit == _collider)
                {
                    continue;
                }

                if (IsColliderStandingOnPlatform(hit))
                {
                    return true;
                }
            }

            return false;
        }

        private bool IsColliderStandingOnPlatform(Collider other)
        {
            if (other.attachedRigidbody == null || other.attachedRigidbody.isKinematic)
            {
                return false;
            }

            Bounds platformBounds = _collider.bounds;
            Bounds otherBounds = other.bounds;

            if (otherBounds.min.y < platformBounds.max.y - _occupancyVerticalTolerance)
            {
                return false;
            }

            return OverlapsHorizontally(platformBounds, otherBounds);
        }

        private static bool OverlapsHorizontally(Bounds platformBounds, Bounds otherBounds)
        {
            return platformBounds.min.x <= otherBounds.max.x
                && platformBounds.max.x >= otherBounds.min.x
                && platformBounds.min.z <= otherBounds.max.z
                && platformBounds.max.z >= otherBounds.min.z;
        }
    }
}
