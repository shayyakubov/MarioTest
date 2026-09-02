using UnityEngine;

namespace MarioTest.Editor
{
    internal static class TestSceneLayout
    {
        public static readonly Vector3 GroundPosition = new(0f, -0.5f, 0f);
        public static readonly Vector3 GroundScale = new(40f, 1f, 40f);
        public static readonly Vector3 PlayerSpawn = new(0f, 1f, 0f);
        public static readonly Vector3 EnemyTestPlayerSpawn = new(0f, 1f, -10f);
        public static readonly Vector3 EnemyPatrolA = new(0f, 1f, 8f);
        public static readonly Vector3 EnemyPatrolB = new(0f, 1f, 18f);
        public static readonly float PatrolHeight = 1f;
        public static readonly Vector3 PatrolOffsetA = new(-4f, 0f, 8f);
        public static readonly Vector3 PatrolOffsetB = new(4f, 0f, 8f);
    }
}
