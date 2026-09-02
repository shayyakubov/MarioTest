using UnityEngine;

namespace MarioTest.Editor
{
    /// <summary>
    /// Course geometry and published jump limits for gap sizing.
    /// Limits match PlayerTuning asset defaults (jumpVelocity 12.5, maxSpeed 8).
    /// </summary>
    internal static class CourseLayout
    {
        public const float PublishedMaxJumpHeight = 3.1f;
        public const float PublishedMaxJumpDistance = 5f;
        public const float SafeGap = 3.5f;

        public static readonly Vector3 PlayerSpawn = new(0f, 1f, -3f);
        public static readonly float DeathPlaneY = -10f;
        public const float DeathPlaneThickness = 10f;
        public static readonly float CourseHalfWidth = 12f;
        public static readonly float CourseLength = 130f;

        public static readonly float PlatformY = -0.25f;
        public static readonly float PlatformThickness = 0.5f;

        public static Vector3 PlatformCenter(float centerZ, float width, float depth)
        {
            return new Vector3(0f, PlatformY, centerZ);
        }

        public static Vector3 PlatformScale(float width, float depth)
        {
            return new Vector3(width, PlatformThickness, depth);
        }
    }
}
