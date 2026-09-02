#if UNITY_EDITOR
using MarioTest.Bootstrap;
using MarioTest.Enemies;
using MarioTest.Input;
using MarioTest.Interaction;
using MarioTest.Player;
using MarioTest.Platforms;
using MarioTest.Systems;
using MarioTest.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace MarioTest.Editor
{
    public static class CourseSceneBuilder
    {
        private const string ScenePath = "Assets/_Project/Scenes/Course.unity";

        [MenuItem("MarioTest/Build Course Scene")]
        public static void BuildScene()
        {
            PhysicsLayersSetup.EnsureConfigured();

            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);

            GameObject courseRoot = new GameObject("Course");
            BuildCourseGeometry(courseRoot.transform);

            Transform cameraTransform = SceneSetupUtility.SetupCamera();
            PlayerController playerController = SceneSetupUtility.CreatePlayerFromPrefab(
                cameraTransform,
                CourseLayout.PlayerSpawn);
            SceneSetupUtility.WireFollowCamera(cameraTransform, playerController.transform);

            GameHudSetup gameHud = SceneSetupUtility.CreateGameHud(includeCourseWin: true, includeCoins: true);
            SceneSetupUtility.WireLivesSystem(
                playerController,
                cameraTransform,
                CourseLayout.PlayerSpawn,
                gameHud.TouchInput);

            WirePickups(courseRoot.transform);

            SceneSetupUtility.CreateEnemyManager(playerController);
            BuildEnemies(courseRoot.transform);
            WireGoalTrigger();

            SceneSetupUtility.CreateDeathPlane(
                new Vector3(0f, CourseLayout.DeathPlaneY, CourseLayout.CourseLength * 0.5f),
                new Vector3(
                    CourseLayout.CourseHalfWidth * 2f,
                    CourseLayout.DeathPlaneThickness,
                    CourseLayout.CourseLength + 20f));

            WireCheckpoint(Object.FindAnyObjectByType<GameSession>());

            EditorSceneManager.SaveScene(scene, ScenePath);
            PhysicsLayersSetup.ConfigurePrefabs();
            AssetDatabase.SaveAssets();
        }

        private static void BuildCourseGeometry(Transform parent)
        {
            CreatePlatform(parent, "Start", 0f, 8f, 10f);
            CreatePlatform(parent, "JumpIntro", 10.5f, 5f, 5f);
            CreatePlatform(parent, "StompArena", 19.5f, 6f, 6f);
            CreatePlatform(parent, "CrateApproach", 26f, 5f, 5f);
            CreatePlatform(parent, "CrateLanding", 35f, 6f, 6f);

            SceneSetupUtility.InstantiatePrefab(
                SceneSetupUtility.CratePrefabPath,
                new Vector3(0f, 1f, 35f),
                parent);

            CreatePlatform(parent, "MovingApproach", 42.5f, 4f, 4f);
            CreateMovingPlatform(parent, new Vector3(0f, CourseLayout.PlatformY, 47f), new Vector3(0f, 0f, 4f));
            CreatePlatform(parent, "MovingExit", 54f, 5f, 5f);

            CreateScaledPrefab(
                parent,
                SceneSetupUtility.PlatformIcePrefabPath,
                "IceRun",
                CourseLayout.PlatformCenter(64f, 4f, 12f),
                CourseLayout.PlatformScale(4f, 12f));

            CreateCrumblePlatform(parent, "Crumble_1", 74f);
            CreateCrumblePlatform(parent, "Crumble_2", 78f);
            CreateCrumblePlatform(parent, "Crumble_3", 82f);

            CreatePlatform(parent, "CheckpointPad", 89f, 6f, 6f);
            CreatePlatform(parent, "ShooterArena", 99f, 8f, 8f);
            CreatePlatform(parent, "GoalApproach", 108f, 5f, 5f);
            CreatePlatform(parent, "GoalPad", 115f, 6f, 6f);
        }

        private static void BuildEnemies(Transform parent)
        {
            SceneSetupUtility.CreateStompableEnemy(new Vector3(0f, 1f, 19.5f), parent);

            SceneSetupUtility.CreatePatrolShooterEnemy(
                new Vector3(-3f, 1f, 96f),
                new Vector3(3f, 1f, 102f));

            SceneSetupUtility.CreateGoalFlag(new Vector3(0f, 0f, 115f), parent);
        }

        private static void WireGoalTrigger()
        {
            GameBootstrap bootstrap = Object.FindAnyObjectByType<GameBootstrap>();
            GoalTrigger goalTrigger = Object.FindAnyObjectByType<GoalTrigger>();
            SceneSetupUtility.WireBootstrapGoalTrigger(bootstrap, goalTrigger);
        }

        private static void WireCheckpoint(GameSession gameSession)
        {
            if (gameSession == null)
            {
                return;
            }

            CheckpointsManager checkpointsManager = gameSession.GetComponent<CheckpointsManager>();
            if (checkpointsManager == null)
            {
                return;
            }

            Transform checkpointsRoot = SceneSetupUtility.GetOrCreateCheckpointsRoot();
            Transform midMarker = SceneSetupUtility.CreateCheckpointMarker(
                "Checkpoint_Mid",
                new Vector3(0f, 0.5f, 89f),
                checkpointsRoot);

            CheckpointTrigger midTrigger = SceneSetupUtility.CreateCheckpointTrigger(
                new Vector3(0f, 1f, 89f),
                new Vector3(6f, 3f, 6f),
                midMarker,
                checkpointsRoot);

            SceneSetupUtility.RegisterCheckpointTrigger(checkpointsManager, midTrigger);
        }

        private static void WirePickups(Transform parent)
        {
            PickupsManager pickupsManager = SceneSetupUtility.EnsurePickupsManager();
            CreateCoins(parent, pickupsManager);

            GameBootstrap bootstrap = Object.FindAnyObjectByType<GameBootstrap>();
            SceneSetupUtility.WireBootstrapPickupsManager(bootstrap, pickupsManager);
        }

        private static void CreatePlatform(Transform parent, string name, float centerZ, float width, float depth)
        {
            SceneSetupUtility.CreatePlatformFromPrefab(
                parent,
                name,
                CourseLayout.PlatformCenter(centerZ, width, depth),
                CourseLayout.PlatformScale(width, depth));
        }

        private static void CreateMovingPlatform(Transform parent, Vector3 position, Vector3 endOffset)
        {
            SceneSetupUtility.CreatePlatformFromPrefab(
                parent,
                "MovingBridge",
                position,
                new Vector3(3f, CourseLayout.PlatformThickness, 3f),
                moving: true,
                endOffset: endOffset);
        }

        private static void CreateCrumblePlatform(Transform parent, string name, float centerZ)
        {
            CreateScaledPrefab(
                parent,
                SceneSetupUtility.PlatformCrumblePrefabPath,
                name,
                CourseLayout.PlatformCenter(centerZ, 4f, 4f),
                CourseLayout.PlatformScale(4f, 4f));
        }

        private static void CreateScaledPrefab(
            Transform parent,
            string prefabPath,
            string name,
            Vector3 position,
            Vector3 scale)
        {
            GameObject instance = SceneSetupUtility.InstantiatePrefab(prefabPath, position, parent);
            if (instance == null)
            {
                return;
            }

            instance.name = name;
            instance.transform.localScale = scale;
        }

        private static void CreateCoins(Transform parent, PickupsManager pickupsManager)
        {
            float coinHeight = 1.2f;
            Vector3[] coinPositions =
            {
                new(0f, coinHeight, 8f),
                new(0f, coinHeight, 17f),
                new(1.5f, coinHeight, 30f),
                new(-1.5f, coinHeight, 44f),
                new(0f, coinHeight, 62f),
                new(0f, coinHeight, 76f),
                new(0f, coinHeight, 92f),
                new(0f, coinHeight, 106f),
            };

            for (int i = 0; i < coinPositions.Length; i++)
            {
                CoinPickup pickup = SceneSetupUtility.CreateCoin(coinPositions[i], parent);
                SceneSetupUtility.RegisterCoinPickup(pickupsManager, pickup);
            }
        }
    }
}
#endif
