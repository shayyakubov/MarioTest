#if UNITY_EDITOR
using MarioTest.Bootstrap;
using MarioTest.Camera;
using MarioTest.Core;
using MarioTest.Enemies;
using MarioTest.Input;
using MarioTest.Player;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

namespace MarioTest.Editor
{
    public static class SceneSetupUtility
    {
        public const string InputActionsPath = "Assets/_Project/Input/PlayerInputActions.inputactions";
        public const string PlayerPhysicsMaterialPath = "Assets/_Project/Physics/PlayerZeroFriction.physicMaterial";
        public const string ProjectilePrefabPath = "Assets/_Project/Prefabs/EnemyProjectile.prefab";
        public const string PatrolShooterEnemyPrefabPath = "Assets/_Project/Prefabs/PatrolShooterEnemy.prefab";

        private const float CanvasReferenceWidth = 1920f;
        private const float CanvasReferenceHeight = 1080f;
        private const float JoystickBackgroundSize = 160f;
        private const float JoystickHandleSize = 64f;
        private const float JoystickBackgroundAlpha = 0.2f;
        private const float JoystickHandleAlpha = 0.55f;

        public static void CreateGround(Vector3 position, Vector3 scale)
        {
            GameObject ground = GameObject.CreatePrimitive(PrimitiveType.Cube);
            ground.name = "Ground";
            ground.transform.position = position;
            ground.transform.localScale = scale;
            PhysicsLayersSetup.SetLayer(ground, PhysicsLayers.Ground);
        }

        public static Transform SetupCamera()
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

        public static void WireFollowCamera(Transform cameraTransform, Transform playerTransform)
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

        public static PlayerController CreatePlayer(Transform cameraTransform, Vector3 position)
        {
            GameObject player = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            player.name = "Player";
            player.transform.position = position;
            PhysicsLayersSetup.SetLayer(player, PhysicsLayers.Player);

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
            controllerSerialized.ApplyModifiedPropertiesWithoutUndo();

            return controller;
        }

        public static MobileTouchInput CreateMobileTouchInput()
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
            scaler.referenceResolution = new Vector2(CanvasReferenceWidth, CanvasReferenceHeight);
            canvasObject.AddComponent<GraphicRaycaster>();

            MobileTouchInput mobileTouchInput = canvasObject.AddComponent<MobileTouchInput>();

            RectTransform joystickRoot = CreateUiRect(canvasObject.transform, "Joystick", Vector2.zero, Vector2.zero);
            RectTransform joystickBackground = CreateUiImage(
                joystickRoot,
                "Background",
                new Vector2(JoystickBackgroundSize, JoystickBackgroundSize),
                new Color(1f, 1f, 1f, JoystickBackgroundAlpha));
            RectTransform joystickHandle = CreateUiImage(
                joystickRoot,
                "Handle",
                new Vector2(JoystickHandleSize, JoystickHandleSize),
                new Color(1f, 1f, 1f, JoystickHandleAlpha));

            joystickRoot.gameObject.SetActive(false);

            SerializedObject touchSerialized = new SerializedObject(mobileTouchInput);
            touchSerialized.FindProperty("_canvas").objectReferenceValue = canvas;
            touchSerialized.FindProperty("_joystickRoot").objectReferenceValue = joystickRoot;
            touchSerialized.FindProperty("_joystickBackground").objectReferenceValue = joystickBackground;
            touchSerialized.FindProperty("_joystickHandle").objectReferenceValue = joystickHandle;
            touchSerialized.ApplyModifiedPropertiesWithoutUndo();

            return mobileTouchInput;
        }

        public static void CreateBootstrap(PlayerController playerController, MobileTouchInput mobileTouchInput)
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

        public static EnemyManager CreateEnemyManager(PlayerController playerController)
        {
            EnemyManager manager = Object.FindAnyObjectByType<EnemyManager>();
            if (manager == null)
            {
                GameObject managerObject = new GameObject("EnemyManager");
                manager = managerObject.AddComponent<EnemyManager>();
            }

            SerializedObject managerSerialized = new SerializedObject(manager);
            managerSerialized.FindProperty("_targetTransform").objectReferenceValue = playerController.transform;
            managerSerialized.FindProperty("_targetRigidbody").objectReferenceValue = playerController.GetComponent<Rigidbody>();
            managerSerialized.ApplyModifiedPropertiesWithoutUndo();

            return manager;
        }

        public static PatrolShooterEnemy CreatePatrolShooterEnemy(Vector3 patrolPointAPosition, Vector3 patrolPointBPosition)
        {
            EnemyProjectile projectilePrefab = GetOrCreateProjectilePrefab();

            Transform patrolPointA = CreatePatrolPoint("Enemy_PatrolA", patrolPointAPosition);
            Transform patrolPointB = CreatePatrolPoint("Enemy_PatrolB", patrolPointBPosition);

            GameObject enemyPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(PatrolShooterEnemyPrefabPath);
            if (enemyPrefab == null)
            {
                Debug.LogError($"Missing enemy prefab at {PatrolShooterEnemyPrefabPath}");
                return null;
            }

            GameObject enemyObject = (GameObject)PrefabUtility.InstantiatePrefab(enemyPrefab);
            enemyObject.transform.position = patrolPointA.position;

            PatrolShooterEnemy shooter = enemyObject.GetComponent<PatrolShooterEnemy>();

            SerializedObject shooterSerialized = new SerializedObject(shooter);
            shooterSerialized.FindProperty("_patrolPointA").objectReferenceValue = patrolPointA;
            shooterSerialized.FindProperty("_patrolPointB").objectReferenceValue = patrolPointB;
            if (shooterSerialized.FindProperty("_projectilePrefab").objectReferenceValue == null)
            {
                shooterSerialized.FindProperty("_projectilePrefab").objectReferenceValue = projectilePrefab;
            }

            shooterSerialized.ApplyModifiedPropertiesWithoutUndo();

            EnemyManager manager = Object.FindAnyObjectByType<EnemyManager>();
            if (manager != null)
            {
                manager.RefreshEnemyList();
                EditorUtility.SetDirty(manager);
            }

            return shooter;
        }

        public static EnemyProjectile GetOrCreateProjectilePrefab()
        {
            EnemyProjectile existing = AssetDatabase.LoadAssetAtPath<EnemyProjectile>(ProjectilePrefabPath);
            if (existing != null)
            {
                return existing;
            }

            GameObject projectileObject = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            projectileObject.name = "EnemyProjectile";
            projectileObject.transform.localScale = Vector3.one * 0.35f;
            PhysicsLayersSetup.SetLayer(projectileObject, PhysicsLayers.Projectile);

            Rigidbody rigidbody = projectileObject.GetComponent<Rigidbody>();
            rigidbody.useGravity = false;
            rigidbody.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;

            projectileObject.AddComponent<EnemyProjectile>();

            if (!AssetDatabase.IsValidFolder("Assets/_Project/Prefabs"))
            {
                AssetDatabase.CreateFolder("Assets/_Project", "Prefabs");
            }

            GameObject prefab = PrefabUtility.SaveAsPrefabAsset(projectileObject, ProjectilePrefabPath);
            Object.DestroyImmediate(projectileObject);

            return prefab.GetComponent<EnemyProjectile>();
        }

        private static Transform CreatePatrolPoint(string name, Vector3 position)
        {
            GameObject point = new GameObject(name);
            point.transform.position = position;
            return point.transform;
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
    }
}
#endif
