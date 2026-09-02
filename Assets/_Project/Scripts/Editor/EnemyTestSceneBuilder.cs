#if UNITY_EDITOR
using MarioTest.Player;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace MarioTest.Editor
{
    public static class EnemyTestSceneBuilder
    {
        private const string ScenePath = "Assets/_Project/Scenes/EnemyTest.unity";

        [MenuItem("MarioTest/Create Enemy Test Scene")]
        public static void CreateScene()
        {
            PhysicsLayersSetup.EnsureConfigured();

            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);

            SceneSetupUtility.CreateGround(TestSceneLayout.GroundPosition, TestSceneLayout.GroundScale);

            Transform cameraTransform = SceneSetupUtility.SetupCamera();
            PlayerController playerController = SceneSetupUtility.CreatePlayer(
                cameraTransform,
                TestSceneLayout.EnemyTestPlayerSpawn);
            SceneSetupUtility.WireFollowCamera(cameraTransform, playerController.transform);

            var mobileTouchInput = SceneSetupUtility.CreateMobileTouchInput();
            SceneSetupUtility.CreateBootstrap(playerController, mobileTouchInput);
            SceneSetupUtility.CreateEnemyManager(playerController);

            SceneSetupUtility.CreatePatrolShooterEnemy(
                TestSceneLayout.EnemyPatrolA,
                TestSceneLayout.EnemyPatrolB);

            EditorSceneManager.SaveScene(scene, ScenePath);
            PhysicsLayersSetup.ConfigurePrefabs();
            AssetDatabase.SaveAssets();
            Debug.Log($"Enemy test scene saved to {ScenePath}");
        }
    }
}
#endif
