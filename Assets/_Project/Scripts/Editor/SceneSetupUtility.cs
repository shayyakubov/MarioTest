#if UNITY_EDITOR
using MarioTest.Bootstrap;
using MarioTest.Camera;
using MarioTest.Core;
using MarioTest.Enemies;
using MarioTest.Input;
using MarioTest.Interaction;
using MarioTest.Platforms;
using MarioTest.Player;
using MarioTest.Systems;
using MarioTest.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
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
        public const string CratePrefabPath = "Assets/_Project/Prefabs/Crate.prefab";
        public const string PlatformMovingPrefabPath = "Assets/_Project/Prefabs/Platform_Moving.prefab";
        public const string PlatformIcePrefabPath = "Assets/_Project/Prefabs/Platform_Ice.prefab";
        public const string PlatformCrumblePrefabPath = "Assets/_Project/Prefabs/Platform_Crumble.prefab";
        public const string PlayerPrefabPath = "Assets/_Project/Prefabs/Player.prefab";
        public const string TouchInputCanvasPrefabPath = "Assets/_Project/Prefabs/TouchInputCanvas.prefab";
        public const string StompableEnemyPrefabPath = "Assets/_Project/Prefabs/StompableEnemy.prefab";

        private const float LifePipSize = 36f;
        private const float LifePipSpacing = 12f;
        private const float LifeHudMargin = 32f;
        private const float CoinHudMargin = 32f;
        private const float CoinHudSize = 48f;

        public static void CreateGround(Vector3 position, Vector3 scale)
        {
            GameObject ground = InstantiatePrefab(PlatformMovingPrefabPath, position);
            if (ground == null)
            {
                return;
            }

            ground.name = "Ground";
            ground.transform.localScale = scale;
            SetPlatformMovement(ground, moving: false);
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
            return CreatePlayerFromPrefab(cameraTransform, position);
        }

        public static PlayerController CreatePlayerFromPrefab(Transform cameraTransform, Vector3 position)
        {
            GameObject player = InstantiatePrefab(PlayerPrefabPath, position);
            if (player == null)
            {
                return null;
            }

            if (player.GetComponent<PlayerHealth>() == null)
            {
                player.AddComponent<PlayerHealth>();
            }

            PlayerController controller = player.GetComponent<PlayerController>();
            SerializedObject controllerSerialized = new SerializedObject(controller);
            controllerSerialized.FindProperty("_cameraTransform").objectReferenceValue = cameraTransform;
            controllerSerialized.ApplyModifiedPropertiesWithoutUndo();

            return controller;
        }

        public static GameHudSetup CreateGameHud(bool includeCourseWin = false, bool includeCoins = false)
        {
            EnsureEventSystem();

            GameObject gameHudRoot = new GameObject("GameHud");
            GameObject touchCanvasObject = InstantiatePrefab(TouchInputCanvasPrefabPath, Vector3.zero, gameHudRoot.transform);
            if (touchCanvasObject == null)
            {
                Object.DestroyImmediate(gameHudRoot);
                return null;
            }

            touchCanvasObject.name = "TouchInputCanvas";

            MobileTouchInput mobileTouchInput = touchCanvasObject.GetComponent<MobileTouchInput>();
            Canvas canvas = touchCanvasObject.GetComponent<Canvas>();

            GameHud gameHud = CreateLivesUi(canvas, gameHudRoot.transform);

            CoinsHud coinsHud = null;
            if (includeCoins)
            {
                coinsHud = CreateCoinsHud(canvas);
            }

            if (includeCourseWin)
            {
                GameOverOverlay courseWinOverlay = CreateCourseWinOverlay(canvas);
                SerializedObject gameHudSerialized = new SerializedObject(gameHud);
                gameHudSerialized.FindProperty("_courseWinOverlay").objectReferenceValue = courseWinOverlay;
                gameHudSerialized.ApplyModifiedPropertiesWithoutUndo();
            }

            return new GameHudSetup
            {
                TouchInput = mobileTouchInput,
                GameHud = gameHud,
                CoinsHud = coinsHud,
            };
        }

        public static MobileTouchInput CreateMobileTouchInput()
        {
            GameHudSetup gameHud = CreateGameHud();
            return gameHud?.TouchInput;
        }

        private static void EnsureEventSystem()
        {
            if (Object.FindAnyObjectByType<EventSystem>() != null)
            {
                return;
            }

            GameObject eventSystem = new GameObject("EventSystem");
            eventSystem.AddComponent<EventSystem>();
            eventSystem.AddComponent<InputSystemUIInputModule>();
        }

        public static void CreateBootstrap(
            PlayerController playerController,
            MobileTouchInput mobileTouchInput,
            GameSession gameSession = null,
            GameHud gameHud = null,
            FollowCameraController followCamera = null,
            GoalTrigger goalTrigger = null)
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
            bootstrapSerialized.FindProperty("_gameSession").objectReferenceValue = gameSession;
            bootstrapSerialized.FindProperty("_gameHud").objectReferenceValue = gameHud;
            bootstrapSerialized.FindProperty("_followCamera").objectReferenceValue = followCamera;
            bootstrapSerialized.FindProperty("_goalTrigger").objectReferenceValue = goalTrigger;
            bootstrapSerialized.ApplyModifiedPropertiesWithoutUndo();
        }

        public static Transform GetOrCreateCheckpointsRoot()
        {
            GameObject existing = GameObject.Find("Checkpoints");
            if (existing != null)
            {
                return existing.transform;
            }

            return new GameObject("Checkpoints").transform;
        }

        public static Transform CreateCheckpointMarker(string name, Vector3 position, Transform checkpointsRoot = null)
        {
            Transform root = checkpointsRoot != null ? checkpointsRoot : GetOrCreateCheckpointsRoot();
            GameObject marker = new GameObject(name);
            marker.transform.SetParent(root, false);
            marker.transform.position = position;
            return marker.transform;
        }

        public static GameSession CreateGameSession(
            PlayerController playerController,
            Vector3 defaultCheckpointPosition)
        {
            Transform checkpointsRoot = GetOrCreateCheckpointsRoot();
            Transform checkpointTransform = CreateCheckpointMarker(
                "Checkpoint_Spawn",
                defaultCheckpointPosition,
                checkpointsRoot);

            GameObject sessionObject = new GameObject("GameSession");
            GameSession session = sessionObject.AddComponent<GameSession>();
            CheckpointsManager checkpointsManager = sessionObject.AddComponent<CheckpointsManager>();

            SerializedObject checkpointsSerialized = new SerializedObject(checkpointsManager);
            checkpointsSerialized.FindProperty("_defaultCheckpoint").objectReferenceValue = checkpointTransform;
            checkpointsSerialized.FindProperty("_playerFallback").objectReferenceValue = playerController.transform;
            checkpointsSerialized.ApplyModifiedPropertiesWithoutUndo();

            return session;
        }

        private static void WireBootstrap(
            GameBootstrap bootstrap,
            PlayerController playerController,
            MobileTouchInput mobileTouchInput,
            GameSession gameSession,
            GameHud gameHud,
            FollowCameraController followCamera,
            GoalTrigger goalTrigger = null)
        {
            SerializedObject bootstrapSerialized = new SerializedObject(bootstrap);
            bootstrapSerialized.FindProperty("_playerController").objectReferenceValue = playerController;
            bootstrapSerialized.FindProperty("_mobileTouchInput").objectReferenceValue = mobileTouchInput;
            bootstrapSerialized.FindProperty("_gameSession").objectReferenceValue = gameSession;
            bootstrapSerialized.FindProperty("_gameHud").objectReferenceValue = gameHud;
            bootstrapSerialized.FindProperty("_followCamera").objectReferenceValue = followCamera;
            bootstrapSerialized.FindProperty("_goalTrigger").objectReferenceValue = goalTrigger;
            bootstrapSerialized.ApplyModifiedPropertiesWithoutUndo();
        }

        public static void WireBootstrapGoalTrigger(GameBootstrap bootstrap, GoalTrigger goalTrigger)
        {
            if (bootstrap == null || goalTrigger == null)
            {
                return;
            }

            SerializedObject bootstrapSerialized = new SerializedObject(bootstrap);
            bootstrapSerialized.FindProperty("_goalTrigger").objectReferenceValue = goalTrigger;
            bootstrapSerialized.ApplyModifiedPropertiesWithoutUndo();
        }

        public static GameHud CreateLivesUi(Canvas canvas, Transform gameHudHost)
        {
            RectTransform hudRoot = CreateUiRect(
                canvas.transform,
                "LivesHud",
                new Vector2(LifeHudMargin + LifePipSize * 0.5f, -LifeHudMargin - LifePipSize * 0.5f),
                Vector2.zero);
            hudRoot.anchorMin = new Vector2(0f, 1f);
            hudRoot.anchorMax = new Vector2(0f, 1f);
            hudRoot.pivot = new Vector2(0f, 1f);

            Image[] pips = new Image[3];
            for (int i = 0; i < pips.Length; i++)
            {
                float x = i * (LifePipSize + LifePipSpacing);
                RectTransform pipRect = CreateUiImage(
                    hudRoot,
                    $"LifePip_{i + 1}",
                    new Vector2(LifePipSize, LifePipSize),
                    new Color(0.95f, 0.2f, 0.2f, 0.95f));
                pipRect.anchoredPosition = new Vector2(x, 0f);
                pips[i] = pipRect.GetComponent<Image>();
            }

            LivesHud livesHud = hudRoot.gameObject.AddComponent<LivesHud>();
            SerializedObject livesSerialized = new SerializedObject(livesHud);
            livesSerialized.FindProperty("_lifePips").arraySize = pips.Length;
            for (int i = 0; i < pips.Length; i++)
            {
                livesSerialized.FindProperty("_lifePips").GetArrayElementAtIndex(i).objectReferenceValue = pips[i];
            }

            livesSerialized.ApplyModifiedPropertiesWithoutUndo();

            RectTransform overlayRoot = CreateUiRect(canvas.transform, "GameOverOverlay", Vector2.zero, Vector2.zero);
            overlayRoot.anchorMin = Vector2.zero;
            overlayRoot.anchorMax = Vector2.one;
            overlayRoot.offsetMin = Vector2.zero;
            overlayRoot.offsetMax = Vector2.zero;

            RectTransform panelRect = CreateUiImage(
                overlayRoot,
                "Panel",
                Vector2.zero,
                new Color(0f, 0f, 0f, 0.65f));
            panelRect.anchorMin = Vector2.zero;
            panelRect.anchorMax = Vector2.one;
            panelRect.offsetMin = Vector2.zero;
            panelRect.offsetMax = Vector2.zero;

            RectTransform titleRect = CreateUiRect(overlayRoot, "Title", new Vector2(0f, 48f), new Vector2(640f, 96f));
            Text titleText = titleRect.gameObject.AddComponent<Text>();
            titleText.text = "Game Over";
            titleText.alignment = TextAnchor.MiddleCenter;
            titleText.fontSize = 56;
            titleText.color = Color.white;
            titleText.raycastTarget = false;
            titleText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

            RectTransform buttonRect = CreateUiRect(overlayRoot, "RestartButton", new Vector2(0f, -72f), new Vector2(320f, 72f));
            Image buttonImage = buttonRect.gameObject.AddComponent<Image>();
            buttonImage.color = new Color(0.2f, 0.55f, 0.95f, 0.95f);

            Button restartButton = buttonRect.gameObject.AddComponent<Button>();
            restartButton.targetGraphic = buttonImage;

            RectTransform buttonLabelRect = CreateUiRect(buttonRect, "Label", Vector2.zero, Vector2.zero);
            buttonLabelRect.anchorMin = Vector2.zero;
            buttonLabelRect.anchorMax = Vector2.one;
            buttonLabelRect.offsetMin = Vector2.zero;
            buttonLabelRect.offsetMax = Vector2.zero;

            Text buttonLabel = buttonLabelRect.gameObject.AddComponent<Text>();
            buttonLabel.text = "Restart";
            buttonLabel.alignment = TextAnchor.MiddleCenter;
            buttonLabel.fontSize = 32;
            buttonLabel.color = Color.white;
            buttonLabel.raycastTarget = false;
            buttonLabel.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

            GameOverOverlay overlay = overlayRoot.gameObject.AddComponent<GameOverOverlay>();
            SerializedObject overlaySerialized = new SerializedObject(overlay);
            overlaySerialized.FindProperty("_restartButton").objectReferenceValue = restartButton;
            overlaySerialized.ApplyModifiedPropertiesWithoutUndo();
            overlayRoot.gameObject.SetActive(false);

            GameHud gameHud = gameHudHost.gameObject.AddComponent<GameHud>();
            SerializedObject gameHudSerialized = new SerializedObject(gameHud);
            gameHudSerialized.FindProperty("_livesHud").objectReferenceValue = livesHud;
            gameHudSerialized.FindProperty("_gameOverOverlay").objectReferenceValue = overlay;
            gameHudSerialized.ApplyModifiedPropertiesWithoutUndo();

            return gameHud;
        }

        public static CoinsHud CreateCoinsHud(Canvas canvas)
        {
            RectTransform hudRoot = CreateUiRect(
                canvas.transform,
                "CoinsHud",
                new Vector2(-CoinHudMargin - CoinHudSize * 0.5f, -CoinHudMargin - CoinHudSize * 0.5f),
                new Vector2(CoinHudSize * 2f, CoinHudSize));
            hudRoot.anchorMin = new Vector2(1f, 1f);
            hudRoot.anchorMax = new Vector2(1f, 1f);
            hudRoot.pivot = new Vector2(1f, 1f);

            RectTransform iconRect = CreateUiImage(
                hudRoot,
                "CoinIcon",
                new Vector2(CoinHudSize, CoinHudSize),
                new Color(0.95f, 0.85f, 0.15f, 0.95f));
            iconRect.anchoredPosition = new Vector2(-CoinHudSize * 0.5f, 0f);

            RectTransform countRect = CreateUiRect(
                hudRoot,
                "Count",
                new Vector2(-CoinHudSize - 8f, 0f),
                new Vector2(48f, CoinHudSize));
            countRect.anchorMin = new Vector2(1f, 0.5f);
            countRect.anchorMax = new Vector2(1f, 0.5f);
            countRect.pivot = new Vector2(1f, 0.5f);

            Text countText = countRect.gameObject.AddComponent<Text>();
            countText.text = "0";
            countText.alignment = TextAnchor.MiddleRight;
            countText.fontSize = 32;
            countText.color = Color.white;
            countText.raycastTarget = false;
            countText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

            CoinsHud coinsHud = hudRoot.gameObject.AddComponent<CoinsHud>();
            SerializedObject coinsSerialized = new SerializedObject(coinsHud);
            coinsSerialized.FindProperty("_countText").objectReferenceValue = countText;
            coinsSerialized.ApplyModifiedPropertiesWithoutUndo();

            return coinsHud;
        }

        public static GameOverOverlay CreateCourseWinOverlay(Canvas canvas)
        {
            RectTransform overlayRoot = CreateUiRect(canvas.transform, "CourseWinOverlay", Vector2.zero, Vector2.zero);
            overlayRoot.anchorMin = Vector2.zero;
            overlayRoot.anchorMax = Vector2.one;
            overlayRoot.offsetMin = Vector2.zero;
            overlayRoot.offsetMax = Vector2.zero;

            RectTransform panelRect = CreateUiImage(
                overlayRoot,
                "Panel",
                Vector2.zero,
                new Color(0f, 0f, 0f, 0.65f));
            panelRect.anchorMin = Vector2.zero;
            panelRect.anchorMax = Vector2.one;
            panelRect.offsetMin = Vector2.zero;
            panelRect.offsetMax = Vector2.zero;

            RectTransform titleRect = CreateUiRect(overlayRoot, "Title", new Vector2(0f, 48f), new Vector2(720f, 96f));
            Text titleText = titleRect.gameObject.AddComponent<Text>();
            titleText.text = "Course Complete!";
            titleText.alignment = TextAnchor.MiddleCenter;
            titleText.fontSize = 56;
            titleText.color = Color.white;
            titleText.raycastTarget = false;
            titleText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

            RectTransform buttonRect = CreateUiRect(overlayRoot, "RestartButton", new Vector2(0f, -72f), new Vector2(320f, 72f));
            Image buttonImage = buttonRect.gameObject.AddComponent<Image>();
            buttonImage.color = new Color(0.2f, 0.75f, 0.35f, 0.95f);

            Button restartButton = buttonRect.gameObject.AddComponent<Button>();
            restartButton.targetGraphic = buttonImage;

            RectTransform buttonLabelRect = CreateUiRect(buttonRect, "Label", Vector2.zero, Vector2.zero);
            buttonLabelRect.anchorMin = Vector2.zero;
            buttonLabelRect.anchorMax = Vector2.one;
            buttonLabelRect.offsetMin = Vector2.zero;
            buttonLabelRect.offsetMax = Vector2.zero;

            Text buttonLabel = buttonLabelRect.gameObject.AddComponent<Text>();
            buttonLabel.text = "Play Again";
            buttonLabel.alignment = TextAnchor.MiddleCenter;
            buttonLabel.fontSize = 32;
            buttonLabel.color = Color.white;
            buttonLabel.raycastTarget = false;
            buttonLabel.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

            GameOverOverlay overlay = overlayRoot.gameObject.AddComponent<GameOverOverlay>();
            SerializedObject overlaySerialized = new SerializedObject(overlay);
            overlaySerialized.FindProperty("_restartButton").objectReferenceValue = restartButton;
            overlaySerialized.ApplyModifiedPropertiesWithoutUndo();
            overlayRoot.gameObject.SetActive(false);

            return overlay;
        }

        public static GameObject CreateStaticPlatform(Transform parent, string name, Vector3 position, Vector3 scale)
        {
            return CreatePlatformFromPrefab(parent, name, position, scale);
        }

        public static GameObject CreatePlatformFromPrefab(
            Transform parent,
            string name,
            Vector3 position,
            Vector3 scale,
            bool moving = false,
            Vector3? endOffset = null)
        {
            GameObject platform = InstantiatePrefab(PlatformMovingPrefabPath, position, parent);
            if (platform == null)
            {
                return null;
            }

            platform.name = name;
            platform.transform.localScale = scale;
            SetPlatformMovement(platform, moving, endOffset);
            return platform;
        }

        private static void SetPlatformMovement(GameObject platform, bool moving, Vector3? endOffset = null)
        {
            MovingPlatformBehaviour movingBehaviour = platform.GetComponent<MovingPlatformBehaviour>();
            if (movingBehaviour == null)
            {
                return;
            }

            SerializedObject movingSerialized = new SerializedObject(movingBehaviour);
            movingSerialized.FindProperty("_speed").floatValue = moving ? movingSerialized.FindProperty("_speed").floatValue : 0f;
            if (endOffset.HasValue)
            {
                movingSerialized.FindProperty("_endOffset").vector3Value = endOffset.Value;
            }

            movingSerialized.ApplyModifiedPropertiesWithoutUndo();
        }

        public static GameObject InstantiatePrefab(string prefabPath, Vector3 position, Transform parent = null)
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
            if (prefab == null)
            {
                Debug.LogError($"Missing prefab at {prefabPath}");
                return null;
            }

            GameObject instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab, parent);
            instance.transform.position = position;
            return instance;
        }

        public static StompableEnemy CreateStompableEnemy(Vector3 position, Transform parent = null)
        {
            GameObject enemyObject = InstantiatePrefab(StompableEnemyPrefabPath, position, parent);
            return enemyObject != null ? enemyObject.GetComponent<StompableEnemy>() : null;
        }

        public static void CreateCoin(Vector3 position, Transform parent = null)
        {
            GameObject coinObject = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            coinObject.name = "Coin";
            coinObject.transform.SetParent(parent, false);
            coinObject.transform.position = position;
            coinObject.transform.localScale = Vector3.one * 0.6f;

            SphereCollider collider = coinObject.GetComponent<SphereCollider>();
            collider.isTrigger = true;

            Renderer renderer = coinObject.GetComponent<Renderer>();
            renderer.material.color = new Color(0.95f, 0.85f, 0.15f);

            coinObject.AddComponent<CoinPickup>();
        }

        public static GoalTrigger CreateGoalFlag(Vector3 platformTopPosition, Transform parent = null)
        {
            GameObject goalRoot = new GameObject("GoalFlag");
            goalRoot.transform.SetParent(parent, false);
            goalRoot.transform.position = platformTopPosition;

            GameObject pole = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            pole.name = "Pole";
            pole.transform.SetParent(goalRoot.transform, false);
            pole.transform.localPosition = new Vector3(0f, 1.5f, 0f);
            pole.transform.localScale = new Vector3(0.15f, 1.5f, 0.15f);
            Object.DestroyImmediate(pole.GetComponent<CapsuleCollider>());

            GameObject flag = GameObject.CreatePrimitive(PrimitiveType.Cube);
            flag.name = "Flag";
            flag.transform.SetParent(goalRoot.transform, false);
            flag.transform.localPosition = new Vector3(0.6f, 2.4f, 0f);
            flag.transform.localScale = new Vector3(1.2f, 0.6f, 0.05f);
            flag.GetComponent<Renderer>().sharedMaterial.color = new Color(0.2f, 0.85f, 0.35f);
            Object.DestroyImmediate(flag.GetComponent<BoxCollider>());

            GameObject triggerObject = new GameObject("GoalTrigger");
            triggerObject.transform.SetParent(goalRoot.transform, false);
            triggerObject.transform.localPosition = new Vector3(0f, 1.5f, 0f);

            BoxCollider triggerCollider = triggerObject.AddComponent<BoxCollider>();
            triggerCollider.isTrigger = true;
            triggerCollider.size = new Vector3(2f, 3f, 2f);

            return triggerObject.AddComponent<GoalTrigger>();
        }


        public static void CreateDeathPlane(Vector3 center, Vector3 size)
        {
            GameObject deathPlane = new GameObject("DeathPlane");
            deathPlane.transform.position = center;

            BoxCollider collider = deathPlane.AddComponent<BoxCollider>();
            collider.isTrigger = true;
            collider.size = size;

            deathPlane.AddComponent<KillZoneTrigger>();
        }

        public static CheckpointTrigger CreateCheckpointTrigger(
            Vector3 position,
            Vector3 size,
            Transform checkpointTransform = null,
            Transform checkpointsRoot = null)
        {
            Transform root = checkpointsRoot != null ? checkpointsRoot : GetOrCreateCheckpointsRoot();

            GameObject checkpointTriggerObject = new GameObject("CheckpointTrigger");
            checkpointTriggerObject.transform.SetParent(root, false);
            checkpointTriggerObject.transform.position = position;

            BoxCollider collider = checkpointTriggerObject.AddComponent<BoxCollider>();
            collider.isTrigger = true;
            collider.size = size;

            CheckpointTrigger trigger = checkpointTriggerObject.AddComponent<CheckpointTrigger>();
            if (checkpointTransform != null)
            {
                SerializedObject triggerSerialized = new SerializedObject(trigger);
                triggerSerialized.FindProperty("_checkpointTransform").objectReferenceValue = checkpointTransform;
                triggerSerialized.ApplyModifiedPropertiesWithoutUndo();
            }

            return trigger;
        }

        public static void WireLivesSystem(
            PlayerController playerController,
            Transform cameraTransform,
            Vector3 spawnPosition,
            MobileTouchInput mobileTouchInput)
        {
            GameSession gameSession = Object.FindAnyObjectByType<GameSession>();
            if (gameSession == null)
            {
                gameSession = CreateGameSession(playerController, spawnPosition);
            }
            else
            {
                EnsureCheckpointsManager(gameSession, playerController.transform, spawnPosition);
            }

            GameHud gameHud = Object.FindAnyObjectByType<GameHud>();

            Canvas canvas = mobileTouchInput.GetComponent<Canvas>();
            if (canvas != null && gameHud == null)
            {
                Transform gameHudHost = mobileTouchInput.transform.parent != null
                    ? mobileTouchInput.transform.parent
                    : mobileTouchInput.transform;
                gameHud = CreateLivesUi(canvas, gameHudHost);
            }

            FollowCameraController followCamera = cameraTransform.GetComponent<FollowCameraController>();
            GoalTrigger goalTrigger = Object.FindAnyObjectByType<GoalTrigger>();

            GameBootstrap bootstrap = Object.FindAnyObjectByType<GameBootstrap>();
            if (bootstrap == null)
            {
                CreateBootstrap(playerController, mobileTouchInput, gameSession, gameHud, followCamera, goalTrigger);
            }
            else
            {
                WireBootstrap(bootstrap, playerController, mobileTouchInput, gameSession, gameHud, followCamera, goalTrigger);
            }
        }

        private static void EnsureCheckpointsManager(GameSession gameSession, Transform playerTransform, Vector3 spawnPosition)
        {
            CheckpointsManager checkpointsManager = gameSession.GetComponent<CheckpointsManager>();
            if (checkpointsManager == null)
            {
                checkpointsManager = gameSession.gameObject.AddComponent<CheckpointsManager>();
            }

            SerializedObject checkpointsSerialized = new SerializedObject(checkpointsManager);
            SerializedProperty defaultCheckpoint = checkpointsSerialized.FindProperty("_defaultCheckpoint");
            if (defaultCheckpoint.objectReferenceValue == null)
            {
                Transform checkpointsRoot = GetOrCreateCheckpointsRoot();
                defaultCheckpoint.objectReferenceValue = CreateCheckpointMarker(
                    "Checkpoint_Spawn",
                    spawnPosition,
                    checkpointsRoot);
            }

            if (checkpointsSerialized.FindProperty("_playerFallback").objectReferenceValue == null)
            {
                checkpointsSerialized.FindProperty("_playerFallback").objectReferenceValue = playerTransform;
            }

            checkpointsSerialized.ApplyModifiedPropertiesWithoutUndo();
        }

        [MenuItem("MarioTest/Wire Lives System To Open Scene")]
        public static void WireLivesSystemToOpenScene()
        {
            PlayerController playerController = Object.FindAnyObjectByType<PlayerController>();
            if (playerController == null)
            {
                Debug.LogError("No PlayerController found in the open scene.");
                return;
            }

            UnityEngine.Camera mainCamera = UnityEngine.Camera.main;
            if (mainCamera == null)
            {
                Debug.LogError("No Main Camera found in the open scene.");
                return;
            }

            MobileTouchInput mobileTouchInput = Object.FindAnyObjectByType<MobileTouchInput>();
            if (mobileTouchInput == null)
            {
                mobileTouchInput = CreateGameHud().TouchInput;
            }

            WireFollowCamera(mainCamera.transform, playerController.transform);
            WireLivesSystem(
                playerController,
                mainCamera.transform,
                playerController.transform.position,
                mobileTouchInput);

            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
            Debug.Log("Lives system wired to open scene.");
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
