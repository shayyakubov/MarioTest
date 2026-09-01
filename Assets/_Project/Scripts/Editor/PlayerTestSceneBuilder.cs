#if UNITY_EDITOR
using MarioTest.Bootstrap;
using MarioTest.Camera;
using MarioTest.Core;
using MarioTest.Input;
using MarioTest.Player;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace MarioTest.Editor
{
    public static class PlayerTestSceneBuilder
    {
        private const string ScenePath = "Assets/_Project/Scenes/PlayerMovementTest.unity";
        private const string InputActionsPath = "Assets/_Project/Input/PlayerInputActions.inputactions";

        private const string PlayerPhysicsMaterialPath = "Assets/_Project/Physics/PlayerZeroFriction.physicMaterial";

        [MenuItem("MarioTest/Create Player Movement Test Scene")]
        public static void CreateScene()
        {
            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);

            CreateGround();
            CreateTestPlatforms();
            Transform cameraTransform = SetupCamera();
            PlayerController playerController = CreatePlayer(cameraTransform);
            WireFollowCamera(cameraTransform, playerController.transform);
            MobileTouchInput mobileTouchInput = CreateMobileTouchInput();
            CreateBootstrap(playerController, mobileTouchInput);

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
            UnityEngine.Camera camera = UnityEngine.Camera.main;
            if (camera == null)
            {
                GameObject cameraObject = new GameObject("Main Camera");
                camera = cameraObject.AddComponent<UnityEngine.Camera>();
                cameraObject.AddComponent<AudioListener>();
                cameraObject.tag = "MainCamera";
            }

            if (camera.GetComponent<FollowCameraController>() == null)
            {
                camera.gameObject.AddComponent<FollowCameraController>();
            }

            return camera.transform;
        }

        private static void WireFollowCamera(Transform cameraTransform, Transform playerTransform)
        {
            FollowCameraController followCamera = cameraTransform.GetComponent<FollowCameraController>();
            if (followCamera == null)
            {
                Debug.LogError("Main Camera is missing FollowCameraController.");
                return;
            }

            SerializedObject followSerialized = new SerializedObject(followCamera);
            followSerialized.FindProperty("_target").objectReferenceValue = playerTransform;
            followSerialized.ApplyModifiedPropertiesWithoutUndo();
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

            PhysicsMaterial playerPhysicsMaterial = AssetDatabase.LoadAssetAtPath<PhysicsMaterial>(PlayerPhysicsMaterialPath);
            if (playerPhysicsMaterial != null)
            {
                player.GetComponent<CapsuleCollider>().material = playerPhysicsMaterial;
            }
            else
            {
                Debug.LogWarning($"Missing physics material at {PlayerPhysicsMaterialPath}");
            }

            SerializedObject controllerSerialized = new SerializedObject(controller);
            controllerSerialized.FindProperty("_cameraTransform").objectReferenceValue = cameraTransform;

            SerializedProperty layerAccelerations = controllerSerialized
                .FindProperty("_movementSettings")
                .FindPropertyRelative("_layerAccelerations");
            if (layerAccelerations.arraySize == 0)
            {
                layerAccelerations.arraySize = 1;
            }

            layerAccelerations.GetArrayElementAtIndex(0)
                .FindPropertyRelative("_layerMask")
                .intValue = PhysicsLayers.GroundMask.value;

            controllerSerialized.ApplyModifiedPropertiesWithoutUndo();

            return controller;
        }

        private static MobileTouchInput CreateMobileTouchInput()
        {
            if (Object.FindAnyObjectByType<EventSystem>() == null)
            {
                GameObject eventSystem = new GameObject("EventSystem");
                eventSystem.AddComponent<EventSystem>();
                eventSystem.AddComponent<InputSystemUIInputModule>();
            }

            GameObject canvasObject = new GameObject("TouchInputCanvas");
            Canvas canvas = canvasObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;

            CanvasScaler scaler = canvasObject.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            canvasObject.AddComponent<GraphicRaycaster>();

            MobileTouchInput mobileTouchInput = canvasObject.AddComponent<MobileTouchInput>();

            RectTransform joystickRoot = CreateUiRect(canvasObject.transform, "Joystick", Vector2.zero, Vector2.zero);
            RectTransform joystickBackground = CreateUiImage(
                joystickRoot,
                "Background",
                new Vector2(160f, 160f),
                new Color(1f, 1f, 1f, 0.2f));
            RectTransform joystickHandle = CreateUiImage(
                joystickRoot,
                "Handle",
                new Vector2(64f, 64f),
                new Color(1f, 1f, 1f, 0.55f));

            joystickRoot.gameObject.SetActive(false);

            SerializedObject touchSerialized = new SerializedObject(mobileTouchInput);
            touchSerialized.FindProperty("_canvas").objectReferenceValue = canvas;
            touchSerialized.FindProperty("_joystickRoot").objectReferenceValue = joystickRoot;
            touchSerialized.FindProperty("_joystickBackground").objectReferenceValue = joystickBackground;
            touchSerialized.FindProperty("_joystickHandle").objectReferenceValue = joystickHandle;
            touchSerialized.ApplyModifiedPropertiesWithoutUndo();

            return mobileTouchInput;
        }

        private static RectTransform CreateUiRect(Transform parent, string name, Vector2 anchoredPosition, Vector2 sizeDelta)
        {
            GameObject gameObject = new GameObject(name, typeof(RectTransform));
            gameObject.transform.SetParent(parent, false);

            RectTransform rectTransform = gameObject.GetComponent<RectTransform>();
            rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
            rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
            rectTransform.pivot = new Vector2(0.5f, 0.5f);
            rectTransform.anchoredPosition = anchoredPosition;
            rectTransform.sizeDelta = sizeDelta;
            return rectTransform;
        }

        private static RectTransform CreateUiImage(Transform parent, string name, Vector2 sizeDelta, Color color)
        {
            RectTransform rectTransform = CreateUiRect(parent, name, Vector2.zero, sizeDelta);
            Image image = rectTransform.gameObject.AddComponent<Image>();
            image.color = color;
            image.raycastTarget = false;
            return rectTransform;
        }

        private static void CreateBootstrap(PlayerController playerController, MobileTouchInput mobileTouchInput)
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
            bootstrapSerialized.FindProperty("_mobileTouchInput").objectReferenceValue = mobileTouchInput;
            bootstrapSerialized.ApplyModifiedPropertiesWithoutUndo();
        }
    }
}
#endif
