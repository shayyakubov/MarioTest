#if UNITY_EDITOR
using MarioTest.Core;
using MarioTest.Enemies;
using MarioTest.Platforms;
using MarioTest.Player;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace MarioTest.Editor
{
    public static class PhysicsLayersSetup
    {
        private static readonly string[] PrefabLayerPairs =
        {
            "Assets/_Project/Prefabs/EnemyProjectile.prefab", PhysicsLayers.Projectile,
            "Assets/_Project/Prefabs/Player.prefab", PhysicsLayers.Player,
            "Assets/_Project/Prefabs/Crate.prefab", PhysicsLayers.Pushable,
            "Assets/_Project/Prefabs/UnflipableCrate.prefab", PhysicsLayers.Pushable,
            "Assets/_Project/Prefabs/Platform_Moving.prefab", PhysicsLayers.Ground,
            "Assets/_Project/Prefabs/Platform_Moving_Ice.prefab", PhysicsLayers.Ground,
            "Assets/_Project/Prefabs/Platform_Ice.prefab", PhysicsLayers.Ground,
            "Assets/_Project/Prefabs/Platform_Crumble.prefab", PhysicsLayers.Ground,
            "Assets/_Project/Prefabs/StompableEnemy.prefab", PhysicsLayers.Enemy,
        };

        [MenuItem("MarioTest/Apply Physics Layers")]
        public static void ApplyAll()
        {
            ConfigureCollisionMatrix();
            ConfigurePrefabs();
            ApplyToOpenScene();
            AssetDatabase.SaveAssets();
        }

        public static void EnsureConfigured()
        {
            ConfigureCollisionMatrix();
        }

        public static void SetLayer(GameObject gameObject, string layerName)
        {
            int layer = LayerMask.NameToLayer(layerName);
            if (layer < 0)
            {
                Debug.LogError($"Layer '{layerName}' is not defined in Tag Manager.");
                return;
            }

            gameObject.layer = layer;
        }

        public static void ConfigureCollisionMatrix()
        {
            int projectile = PhysicsLayers.ProjectileLayer;
            int enemy = PhysicsLayers.EnemyLayer;

            if (projectile < 0 || enemy < 0)
            {
                Debug.LogError("Missing Projectile or Enemy layer in Tag Manager.");
                return;
            }

            Physics.IgnoreLayerCollision(projectile, enemy, true);
            Physics.IgnoreLayerCollision(projectile, projectile, true);
        }

        public static void ConfigurePrefabs()
        {
            for (int i = 0; i < PrefabLayerPairs.Length; i += 2)
            {
                GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabLayerPairs[i]);
                if (prefab == null)
                {
                    continue;
                }

                SetLayer(prefab, PrefabLayerPairs[i + 1]);
                EditorUtility.SetDirty(prefab);
            }
        }

        public static void ApplyToOpenScene()
        {
            GameObject[] roots = EditorSceneManager.GetActiveScene().GetRootGameObjects();
            foreach (GameObject root in roots)
            {
                ApplyLayerRecursively(root);
            }

            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        }

        public static void ApplyLayerRecursively(GameObject gameObject)
        {
            ApplyLayerToObject(gameObject);

            Transform transform = gameObject.transform;
            for (int i = 0; i < transform.childCount; i++)
            {
                ApplyLayerRecursively(transform.GetChild(i).gameObject);
            }
        }

        public static void ApplyLayerToObject(GameObject gameObject)
        {
            if (gameObject.GetComponent<PlayerController>() != null)
            {
                SetLayer(gameObject, PhysicsLayers.Player);
                return;
            }

            if (gameObject.GetComponent<PatrolShooterEnemy>() != null
                || gameObject.GetComponent<StompableEnemy>() != null)
            {
                SetLayer(gameObject, PhysicsLayers.Enemy);
                return;
            }

            if (gameObject.GetComponent<EnemyProjectile>() != null)
            {
                SetLayer(gameObject, PhysicsLayers.Projectile);
                return;
            }

            if (gameObject.name is "Crate" or "UnflipableCrate")
            {
                SetLayer(gameObject, PhysicsLayers.Pushable);
                return;
            }

            if (gameObject.name == "Ground"
                || gameObject.GetComponent<MovingPlatformBehaviour>() != null
                || gameObject.GetComponent<CrumblePlatformBehaviour>() != null
                || gameObject.GetComponent<IceSurfaceBehaviour>() != null)
            {
                SetLayer(gameObject, PhysicsLayers.Ground);
            }
        }
    }
}
#endif
