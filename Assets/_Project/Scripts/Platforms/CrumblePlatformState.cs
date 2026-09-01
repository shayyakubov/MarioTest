namespace MarioTest.Platforms
{
    public sealed class CrumblePlatformState
    {
        private CrumblePlatformPhase _phase = CrumblePlatformPhase.Idle;
        private float _phaseTimer;

        public CrumblePlatformPhase Phase => _phase;

        public bool IsCrumbled => _phase == CrumblePlatformPhase.Crumbled;

        public void Tick(float deltaTime, bool isOccupied, float standDuration, float warnDuration)
        {
            if (_phase == CrumblePlatformPhase.Crumbled)
            {
                return;
            }

            if (!isOccupied)
            {
                if (_phase == CrumblePlatformPhase.Warning)
                {
                    _phase = CrumblePlatformPhase.Idle;
                }

                _phaseTimer = 0f;
                return;
            }

            if (_phase == CrumblePlatformPhase.Idle)
            {
                _phaseTimer += deltaTime;
                if (_phaseTimer >= standDuration)
                {
                    _phase = CrumblePlatformPhase.Warning;
                    _phaseTimer = 0f;
                }

                return;
            }

            _phaseTimer += deltaTime;
            if (_phaseTimer >= warnDuration)
            {
                _phase = CrumblePlatformPhase.Crumbled;
            }
        }

        public void Reset()
        {
            _phase = CrumblePlatformPhase.Idle;
            _phaseTimer = 0f;
        }
    }
}
