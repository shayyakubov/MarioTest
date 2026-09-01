using UnityEngine;

namespace MarioTest.Platforms
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Renderer))]
    public sealed class CrumblePlatformVisual : MonoBehaviour
    {
        private static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");

        [SerializeField] private float _fadeCycleDuration = 1.2f;
        [SerializeField] private float _minAlpha = 0.35f;

        private Renderer _renderer;
        private MaterialPropertyBlock _propertyBlock;
        private Color _baseColor;
        private float _fadeTimer;

        private void Awake()
        {
            _renderer = GetComponent<Renderer>();
            _propertyBlock = new MaterialPropertyBlock();
            _baseColor = _renderer.sharedMaterial.GetColor(BaseColorId);
        }

        public void Apply(CrumblePlatformPhase phase, bool isOccupied, float deltaTime)
        {
            if (phase == CrumblePlatformPhase.Crumbled)
            {
                _renderer.enabled = false;
                return;
            }

            if (!isOccupied)
            {
                _fadeTimer = 0f;
                SetAlpha(1f);
                return;
            }

            _fadeTimer += deltaTime;
            float wave = (Mathf.Cos(_fadeTimer / _fadeCycleDuration * Mathf.PI * 2f) + 1f) * 0.5f;
            SetAlpha(Mathf.Lerp(_minAlpha, 1f, wave));
        }

        public void ResetVisual()
        {
            _fadeTimer = 0f;
            _renderer.enabled = true;
            _renderer.SetPropertyBlock(null);
            SetAlpha(1f);
        }

        private void SetAlpha(float alpha)
        {
            _renderer.enabled = true;
            _renderer.GetPropertyBlock(_propertyBlock);
            Color color = _baseColor;
            color.a = alpha;
            _propertyBlock.SetColor(BaseColorId, color);
            _renderer.SetPropertyBlock(_propertyBlock);
        }
    }
}
