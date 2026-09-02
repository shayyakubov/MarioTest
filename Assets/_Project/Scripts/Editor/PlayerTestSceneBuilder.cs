#if UNITY_EDITOR
using MarioTest.Core;
using MarioTest.Enemies;
using MarioTest.Input;
using MarioTest.Platforms;
using MarioTest.Player;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace MarioTest.Editor
{
    public static class PlayerTestSceneBuilder
    {
        private const string ScenePath = "Assets/_Project/Scenes/PlayerMovementTest.unity";
        private const string CrumbleMaterialPath = "Assets/_Project/Materials/Crumble.mat";
        private const string IceMaterialPath = "Assets/_Project/Materials/Ice.mat";

        [MenuItem("MarioTest/Add Patrol Shooter Enemy To Scene")]
        public static void AddPatrolShooterEnemyToScene()
        {
            if (Object.FindAnyObjectByType<PatrolShooterEnemy>() != null)
            {
                Debug.LogWarning("Scene already contains a PatrolShooterEnemy.");
                return;
            }

            PlayerController player = Object.FindAnyObjectByType<PlayerController>();
            if (player == null)
            {
                Debug.LogError("No PlayerController found in the open scene.");
                return;
            }

            SceneSetupUtility.CreateEnemyManager(player);

            Vector3 patrolCenter = player.transform.position;
            patrolCenter.y = TestSceneLayout.PatrolHeight;
            SceneSetupUtility.CreatePatrolShooterEnemy(
                patrolCenter + TestSceneLayout.PatrolOffsetA,
                patrolCenter + TestSceneLayout.PatrolOffsetB);

            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
            AssetDatabase.SaveAssets();
            Debug.Log("Patrol shooter enemy added to scene.");
        }

        [MenuItem("MarioTest/Create Player Movement Test Scene")]
        public static void CreateScene()
        {
            PhysicsLayersSetup.EnsureConfigured();

            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);

            SceneSetupUtility.CreateGround(TestSceneLayout.GroundPosition, TestSceneLayout.GroundScale);
            CreateTestPlatforms();

            Transform cameraTransform = SceneSetupUtility.SetupCamera();
            PlayerController playerController = SceneSetupUtility.CreatePlayer(
                cameraTransform,
                TestSceneLayout.PlayerSpawn);
            SceneSetupUtility.WireFollowCamera(cameraTransform, playerController.transform);

            MobileTouchInput mobileTouchInput = SceneSetupUtility.CreateMobileTouchInput();
            SceneSetupUtility.CreateBootstrap(playerController, mobileTouchInput);

            EditorSceneManager.SaveScene(scene, ScenePath);
            PhysicsLayersSetup.ConfigurePrefabs();
            AssetDatabase.SaveAssets();
            Debug.Log($"Player movement test scene saved to {ScenePath}");
        }

        private static void CreateTestPlatforms()
        {
            CreateGroundPlatform("Platform_High", new Vector3(6f, 2f, 0f), new Vector3(8f, 0.5f, 8f));
            CreateGroundPlatform("Platform_Ledge", new Vector3(-5f, 1.5f, 4f), new Vector3(6f, 0.5f, 6f));
            CreateMovingPlatform("Platform_Moving", new Vector3(-5f, 1f, -4f), new Vector3(6f, 0.5f, 6f));
            CreateIcePlatform("Platform_Ice", new Vector3(0f, 0.5f, 6f), new Vector3(10f, 0.5f, 10f));
            CreateMovingIcePlatform("Platform_Moving_Ice", new Vector3(10f, 1f, -4f), new Vector3(6f, 0.5f, 6f));
            CreateCrumblePlatform("Platform_Crumble", new Vector3(5f, 1.5f, -6f), new Vector3(6f, 0.5f, 6f));
        }

        private static GameObject CreateGroundPlatform(string name, Vector3 position, Vector3 scale)
        {
            GameObject platform = GameObject.CreatePrimitive(PrimitiveType.Cube);
            platform.name = name;
            platform.transform.position = position;
            platform.transform.localScale = scale;
            PhysicsLayersSetup.SetLayer(platform, PhysicsLayers.Ground);
            return platform;
        }

        private static void CreateMovingPlatform(string name, Vector3 position, Vector3 scale)
        {
            GameObject platform = CreateGroundPlatform(name, position, scale);
            platform.AddComponent<MovingPlatformBehaviour>();
        }

        private static void CreateIcePlatform(string name, Vector3 position, Vector3 scale)
        {
            GameObject platform = CreateGroundPlatform(name, position, scale);
            platform.AddComponent<IceSurfaceBehaviour>();
            ApplyIceMaterial(platform);
        }

        private static void CreateMovingIcePlatform(string name, Vector3 position, Vector3 scale)
        {
            GameObject platform = CreateGroundPlatform(name, position, scale);
            platform.AddComponent<MovingPlatformBehaviour>();
            platform.AddComponent<IceSurfaceBehaviour>();
            ApplyIceMaterial(platform);
        }

        private static void ApplyIceMaterial(GameObject platform)
        {
            Material iceMaterial = AssetDatabase.LoadAssetAtPath<Material>(IceMaterialPath);
            if (iceMaterial != null)
            {
                platform.GetComponent<Renderer>().sharedMaterial = iceMaterial;
            }
        }

        private static void CreateCrumblePlatform(string name, Vector3 position, Vector3 scale)
        {
            GameObject platform = CreateGroundPlatform(name, position, scale);

            Material crumbleMaterial = AssetDatabase.LoadAssetAtPath<Material>(CrumbleMaterialPath);
            if (crumbleMaterial != null)
            {
                platform.GetComponent<Renderer>().sharedMaterial = crumbleMaterial;
            }

            platform.AddComponent<CrumblePlatformBehaviour>();
        }
    }
}
#endif
