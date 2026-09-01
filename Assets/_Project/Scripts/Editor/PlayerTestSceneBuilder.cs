#if UNITY_EDITOR
using MarioTest.Bootstrap;
using MarioTest.Core;
using MarioTest.Player;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

namespace MarioTest.Editor
{
    public static class PlayerTestSceneBuilder
    {
        private const string ScenePath = "Assets/_Project/Scenes/PlayerMovementTest.unity";
        private const string InputActionsPath = "Assets/_Project/Input/PlayerInputActions.inputactions";

        [MenuItem("MarioTest/Create Player Movement Test Scene")]
        public static void CreateScene()
        {
            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);

            CreateGround();
            CreateTestPlatforms();
            Transform cameraTransform = SetupCamera();
            PlayerController playerController = CreatePlayer(cameraTransform);
            CreateBootstrap(playerController);

            EditorSceneManager.SaveScene(scene, ScenePath);
            AssetDatabase.SaveAssets();
            Debug.Log($"Player movement test scene saved to {ScenePath}");
        }

        private static void CreateGround()
        {
            GameObject ground = GameObject.CreatePrimitive(PrimitiveType.Cube);
            ground.name = "Ground";
            ground.layer = PhysicsLayers.Ground;
            ground.transform.position = new Vector3(0f, -0.5f, 0f);
            ground.transform.localScale = new Vector3(20f, 1f, 20f);
        }

        private static void CreateTestPlatforms()
        {
            CreateGroundPlatform("Platform_High", new Vector3(6f, 2f, 0f), new Vector3(4f, 0.5f, 4f));
            CreateGroundPlatform("Platform_Ledge", new Vector3(-5f, 1.5f, 4f), new Vector3(3f, 0.5f, 3f));
        }

        private static void CreateGroundPlatform(string name, Vector3 position, Vector3 scale)
        {
            GameObject platform = GameObject.CreatePrimitive(PrimitiveType.Cube);
            platform.name = name;
            platform.layer = PhysicsLayers.Ground;
            platform.transform.position = position;
            platform.transform.localScale = scale;
        }

        private static Transform SetupCamera()
        {
            Camera camera = Camera.main;
            if (camera == null)
            {
                GameObject cameraObject = new GameObject("Main Camera");
                camera = cameraObject.AddComponent<Camera>();
                cameraObject.AddComponent<AudioListener>();
                cameraObject.tag = "MainCamera";
            }

            camera.transform.position = new Vector3(0f, 6f, -10f);
            camera.transform.rotation = Quaternion.Euler(25f, 0f, 0f);
            return camera.transform;
        }

        private static PlayerController CreatePlayer(Transform cameraTransform)
        {
            GameObject player = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            player.name = "Player";
            player.transform.position = new Vector3(0f, 1f, 0f);

            Rigidbody rigidbody = player.AddComponent<Rigidbody>();
            rigidbody.useGravity = false;
            rigidbody.linearDamping = 0f;
            rigidbody.angularDamping = 0f;
            rigidbody.interpolation = RigidbodyInterpolation.Interpolate;
            rigidbody.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
            rigidbody.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;

            PlayerController controller = player.AddComponent<PlayerController>();

            SerializedObject controllerSerialized = new SerializedObject(controller);
            controllerSerialized.FindProperty("_cameraTransform").objectReferenceValue = cameraTransform;
            controllerSerialized.ApplyModifiedPropertiesWithoutUndo();

            return controller;
        }

        private static void CreateBootstrap(PlayerController playerController)
        {
            InputActionAsset inputActions = AssetDatabase.LoadAssetAtPath<InputActionAsset>(InputActionsPath);
            if (inputActions == null)
            {
                Debug.LogError($"Missing input actions asset at {InputActionsPath}");
                return;
            }

            GameObject bootstrapObject = new GameObject("GameBootstrap");
            GameBootstrap bootstrap = bootstrapObject.AddComponent<GameBootstrap>();

            SerializedObject bootstrapSerialized = new SerializedObject(bootstrap);
            bootstrapSerialized.FindProperty("_inputActions").objectReferenceValue = inputActions;
            bootstrapSerialized.FindProperty("_playerController").objectReferenceValue = playerController;
            bootstrapSerialized.ApplyModifiedPropertiesWithoutUndo();
        }
    }
}
#endif
