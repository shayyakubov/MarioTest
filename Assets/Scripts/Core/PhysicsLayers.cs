using UnityEngine;

namespace MarioTest.Core
{
    public static class PhysicsLayers
    {
        public const string Ground = "Ground";
        public const string Player = "Player";
        public const string Enemy = "Enemy";
        public const string Projectile = "Projectile";
        public const string Pushable = "Pushable";
        public const string Default = "Default";

        public static int GroundLayer => LayerMask.NameToLayer(Ground);
        public static int PlayerLayer => LayerMask.NameToLayer(Player);
        public static int EnemyLayer => LayerMask.NameToLayer(Enemy);
        public static int ProjectileLayer => LayerMask.NameToLayer(Projectile);
        public static int PushableLayer => LayerMask.NameToLayer(Pushable);
        public static int DefaultLayer => LayerMask.NameToLayer(Default);

        public static int GroundMask => 1 << GroundLayer;
        public static int PlayerMask => 1 << PlayerLayer;
        public static int EnemyMask => 1 << EnemyLayer;
        public static int ProjectileMask => 1 << ProjectileLayer;
        public static int PushableMask => 1 << PushableLayer;
        public static int DefaultMask => 1 << DefaultLayer;

        /// <summary>Layers treated as standable surfaces for ground detection.</summary>
        public static int StandableMask => GroundMask | PushableMask | DefaultMask;
    }
}
