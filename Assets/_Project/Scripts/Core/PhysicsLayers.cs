using System;
using UnityEngine;

namespace MarioTest.Core
{
    public static class PhysicsLayers
    {
        private const string GroundLayerName = "Ground";

        public static int Ground => RequireLayer(GroundLayerName);
        public static LayerMask GroundMask => RequireMask(GroundLayerName);

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ValidateOnPlayModeStartup()
        {
            _ = Ground;
            _ = GroundMask;
        }

#if UNITY_EDITOR
        [UnityEditor.InitializeOnLoadMethod]
        private static void ValidateInEditor()
        {
            _ = Ground;
            _ = GroundMask;
        }
#endif

        private static int RequireLayer(string layerName)
        {
            int layer = LayerMask.NameToLayer(layerName);
            if (layer < 0)
            {
                Fail(layerName);
            }

            return layer;
        }

        private static LayerMask RequireMask(string layerName)
        {
            LayerMask mask = LayerMask.GetMask(layerName);
            if (mask.value == 0)
            {
                Fail(layerName);
            }

            return mask;
        }

        private static void Fail(string layerName)
        {
            throw new InvalidOperationException(
                $"Physics layer '{layerName}' is not defined. Add it under Edit > Project Settings > Tags and Layers.");
        }
    }
}
