namespace MarioTest.Core
{
    public interface ILifeTarget
    {
        void TakeHit(bool respawnAtCheckpoint = true);
    }
}
