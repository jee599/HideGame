#if UNITY_EDITOR
using System.Linq;
using TMPro;
using Unity.AI.Navigation;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public static partial class BlendInBootstrapper
{
    private static void CreateScenes(BlendInBootstrapAssets assets)
    {
        CreateMainMenuScene();
        CreateResultScene();
        CreateGameScene(assets);
        ApplyBuildSettings();
    }

    private static void CreateMainMenuScene()
    {
        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

        CreateDirectionalLight();
        CreateStaticCamera(new Vector3(0f, 14f, -18f), new Vector3(18f, 0f, 0f));
        CreatePrimitiveGround(new Vector3(0f, 0f, 0f), Vector3.one * 3f, Color.gray);

        var canvas = CreateCanvas("MainMenuCanvas");
        var title = CreateTextElement("Title", canvas.transform, "BLEND IN", 52, TextAlignmentOptions.Center);
        Stretch((RectTransform)title.transform, new Vector2(0.5f, 0.75f), new Vector2(0.5f, 0.75f), Vector2.zero, new Vector2(600f, 100f));

        var body = CreateTextElement("Body", canvas.transform, "Use Blend In/Bootstrap Prototype to rebuild the sample scenes.", 24, TextAlignmentOptions.Center);
        Stretch((RectTransform)body.transform, new Vector2(0.5f, 0.62f), new Vector2(0.5f, 0.62f), Vector2.zero, new Vector2(900f, 80f));

        EditorSceneManager.SaveScene(scene, SceneRoot + "/MainMenu.unity");
    }

    private static void CreateResultScene()
    {
        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

        CreateDirectionalLight();
        CreateStaticCamera(new Vector3(0f, 12f, -16f), new Vector3(20f, 0f, 0f));
        CreatePrimitiveGround(new Vector3(0f, 0f, 0f), Vector3.one * 2f, Color.gray);

        var canvas = CreateCanvas("ResultCanvas");
        var title = CreateTextElement("Title", canvas.transform, "Result Scene Placeholder", 42, TextAlignmentOptions.Center);
        Stretch((RectTransform)title.transform, new Vector2(0.5f, 0.70f), new Vector2(0.5f, 0.70f), Vector2.zero, new Vector2(900f, 80f));

        var body = CreateTextElement("Body", canvas.transform, "GameOverUI currently displays inside GameScene. Use this scene later for flow polish.", 22, TextAlignmentOptions.Center);
        Stretch((RectTransform)body.transform, new Vector2(0.5f, 0.58f), new Vector2(0.5f, 0.58f), Vector2.zero, new Vector2(1000f, 80f));

        EditorSceneManager.SaveScene(scene, SceneRoot + "/ResultScene.unity");
    }

    private static void CreateGameScene(BlendInBootstrapAssets assets)
    {
        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

        var environmentRoot = new GameObject("Environment").transform;
        var markerRoot = new GameObject("WorldMarkers").transform;
        var routeRoot = new GameObject("HunterRoute").transform;

        CreateDirectionalLight();
        CreateGrayboxMap(assets, environmentRoot, markerRoot);
        BuildSceneNavMesh(environmentRoot.gameObject);

        var player = (GameObject)PrefabUtility.InstantiatePrefab(assets.PlayerPrefab);
        player.name = "Player";
        player.transform.position = new Vector3(-52f, 0.05f, 56f);

        var hunter = (GameObject)PrefabUtility.InstantiatePrefab(assets.HunterPrefab);
        hunter.name = "Hunter";
        hunter.transform.position = new Vector3(0f, 0.05f, 12f);

        var camera = CreateFollowCamera(player.transform);
        var playerController = player.GetComponent<PlayerController>();
        playerController.cameraPivot = camera.transform;

        var systemsRoot = new GameObject("_Systems").transform;
        var gameManager = new GameObject("GameManager").AddComponent<GameManager>();
        gameManager.transform.SetParent(systemsRoot);
        var timeManager = new GameObject("TimeManager").AddComponent<TimeManager>();
        timeManager.transform.SetParent(systemsRoot);
        var scoreManager = new GameObject("ScoreManager").AddComponent<ScoreManager>();
        scoreManager.transform.SetParent(systemsRoot);
        var eventManager = new GameObject("EventManager").AddComponent<EventManager>();
        eventManager.transform.SetParent(systemsRoot);
        var missionManager = new GameObject("MissionManager").AddComponent<MissionManager>();
        missionManager.transform.SetParent(systemsRoot);
        var relationshipManager = new GameObject("RelationshipManager").AddComponent<RelationshipManager>();
        relationshipManager.transform.SetParent(systemsRoot);
        var citizenSpawner = new GameObject("CitizenSpawner").AddComponent<CitizenSpawner>();
        citizenSpawner.transform.SetParent(systemsRoot);

        eventManager.availableEvents = assets.Events;
        missionManager.missionPool = assets.Missions;
        citizenSpawner.citizenPrefab = assets.CitizenPrefab;
        citizenSpawner.archetypes = assets.Archetypes;

        var routePoints = CreateHunterRoute(routeRoot);
        var hunterAi = hunter.GetComponent<HunterAI>();
        hunterAi.patrolRoute = routePoints;
        hunterAi.config = assets.HunterConfig;

        BuildGameplayUI(player, missionManager);
        EditorSceneManager.MarkSceneDirty(scene);

        EditorSceneManager.SaveScene(scene, SceneRoot + "/GameScene.unity");
    }

    private static void CreateGrayboxMap(BlendInBootstrapAssets assets, Transform environmentRoot, Transform markerRoot)
    {
        CreatePrimitiveGround(Vector3.zero, Vector3.one * 20f, new Color(0.36f, 0.56f, 0.32f), assets.GroundMaterial).transform.SetParent(environmentRoot);

        CreateRoad(environmentRoot, assets.ArtSet, "MainRoad", new Vector3(0f, 0.02f, 14f), new Vector3(18f, 0.05f, 2f), assets.RoadMaterial);
        CreateRoad(environmentRoot, assets.ArtSet, "EastRoad", new Vector3(22f, 0.02f, -10f), new Vector3(2f, 0.05f, 18f), assets.RoadMaterial);
        CreateRoad(environmentRoot, assets.ArtSet, "WestRoad", new Vector3(-22f, 0.02f, -4f), new Vector3(2f, 0.05f, 14f), assets.RoadMaterial);

        CreateBuilding(environmentRoot, assets.ArtSet, "Apartment_A", new Vector3(-60f, 6f, 56f), new Vector3(14f, 12f, 14f), assets.BuildingMaterial);
        CreateBuilding(environmentRoot, assets.ArtSet, "Apartment_B", new Vector3(-42f, 5f, 58f), new Vector3(12f, 10f, 12f), assets.BuildingMaterial);
        CreateBuilding(environmentRoot, assets.ArtSet, "Apartment_C", new Vector3(-24f, 6f, 56f), new Vector3(14f, 12f, 14f), assets.BuildingMaterial);
        CreateBuilding(environmentRoot, assets.ArtSet, "School", new Vector3(60f, 8f, 56f), new Vector3(18f, 16f, 18f), assets.BuildingMaterial);
        CreateBuilding(environmentRoot, assets.ArtSet, "Office", new Vector3(52f, 7f, 30f), new Vector3(20f, 14f, 16f), assets.BuildingMaterial);
        CreateBuilding(environmentRoot, assets.ArtSet, "Cafe", new Vector3(-56f, 4f, 16f), new Vector3(12f, 8f, 10f), assets.BuildingMaterial);
        CreateBuilding(environmentRoot, assets.ArtSet, "Convenience", new Vector3(-40f, 4f, 16f), new Vector3(10f, 8f, 10f), assets.BuildingMaterial);
        CreateBuilding(environmentRoot, assets.ArtSet, "Restaurant", new Vector3(-22f, 4f, 16f), new Vector3(14f, 8f, 12f), assets.BuildingMaterial);
        CreateBuilding(environmentRoot, assets.ArtSet, "Shop", new Vector3(-46f, 4f, 0f), new Vector3(12f, 8f, 10f), assets.BuildingMaterial);

        CreatePlaza(environmentRoot, assets);
        CreatePark(environmentRoot, assets);
        CreateStreetProps(environmentRoot, assets.ArtSet);

        CreateDestination(markerRoot, "Home_01", "Home", new Vector3(-60f, 0f, 48f));
        CreateDestination(markerRoot, "Home_02", "Home", new Vector3(-42f, 0f, 48f));
        CreateDestination(markerRoot, "Home_03", "Home", new Vector3(-24f, 0f, 48f));
        CreateDestination(markerRoot, "School", "School", new Vector3(60f, 0f, 44f), crowdZone: true);
        CreateDestination(markerRoot, "Office", "Office", new Vector3(52f, 0f, 18f), crowdZone: true);
        CreateDestination(markerRoot, "Cafe", "Cafe", new Vector3(-56f, 0f, 8f), shelter: true, crowdZone: true);
        CreateDestination(markerRoot, "Convenience", "Convenience", new Vector3(-40f, 0f, 8f), shelter: true);
        CreateDestination(markerRoot, "Restaurant", "Restaurant", new Vector3(-22f, 0f, 8f), shelter: true, crowdZone: true);
        CreateDestination(markerRoot, "Shop", "Shop", new Vector3(-46f, 0f, -8f), shelter: true);
        CreateDestination(markerRoot, "Plaza", "Plaza", new Vector3(0f, 0f, 0f), crowdZone: true);
        CreateDestination(markerRoot, "Performance", "Performance", new Vector3(8f, 0f, 0f), crowdZone: true);
        CreateDestination(markerRoot, "Park", "Park", new Vector3(44f, 0f, -46f), crowdZone: true);
        CreateDestination(markerRoot, "Bench", "Bench", new Vector3(52f, 0f, -44f), crowdZone: true, createSitPoint: true);
        CreateDestination(markerRoot, "BusStop", "BusStop", new Vector3(74f, 0f, 16f), crowdZone: true);
        CreateDestination(markerRoot, "Shelter", "Shelter", new Vector3(-52f, 0f, 10f), shelter: true);
        CreateDestination(markerRoot, "Vendor", "Vendor", new Vector3(-8f, 0f, 8f), crowdZone: true);
        CreateDestination(markerRoot, "Patrol", "Patrol", new Vector3(18f, 0f, 22f));
        CreateDestination(markerRoot, "Accident", "Accident", new Vector3(18f, 0f, 14f), crowdZone: true);
        CreateDestination(markerRoot, "SideStreet", "SideStreet", new Vector3(-70f, 0f, 24f), shelter: true);
        CreateDestination(markerRoot, "Exit", "Exit", new Vector3(-76f, 0f, -62f));

        CreateSpawnZone(markerRoot, "Home_Spawn_A", "Home", new Vector3(-60f, 0f, 62f), new Vector3(10f, 1f, 10f));
        CreateSpawnZone(markerRoot, "Home_Spawn_B", "Home", new Vector3(-42f, 0f, 62f), new Vector3(10f, 1f, 10f));
        CreateSpawnZone(markerRoot, "Home_Spawn_C", "Home", new Vector3(-24f, 0f, 62f), new Vector3(10f, 1f, 10f));
        CreateSpawnZone(markerRoot, "Restaurant_Spawn", "Restaurant", new Vector3(-20f, 0f, 20f), new Vector3(8f, 1f, 8f));
        CreateSpawnZone(markerRoot, "Shop_Spawn", "Shop", new Vector3(-46f, 0f, 2f), new Vector3(8f, 1f, 8f));
        CreateSpawnZone(markerRoot, "Park_Spawn", "Park", new Vector3(34f, 0f, -54f), new Vector3(10f, 1f, 10f));
        CreateSpawnZone(markerRoot, "Patrol_Spawn", "Patrol", new Vector3(22f, 0f, 18f), new Vector3(6f, 1f, 6f));
        CreateSpawnZone(markerRoot, "Vendor_Spawn", "Vendor", new Vector3(-10f, 0f, 4f), new Vector3(6f, 1f, 6f));

        CreateMissionTrigger(markerRoot, "CafeTrigger", "Cafe", new Vector3(-56f, 1f, 8f), new Vector3(10f, 2f, 10f), true);
        CreateMissionTrigger(markerRoot, "BusStopTrigger", "BusStop", new Vector3(74f, 1f, 16f), new Vector3(8f, 2f, 8f), false);
        CreateMissionTrigger(markerRoot, "BenchTrigger", "Bench", new Vector3(52f, 1f, -44f), new Vector3(8f, 2f, 8f), false);
    }

    private static Transform[] CreateHunterRoute(Transform parent)
    {
        var positions = new[]
        {
            new Vector3(-18f, 0f, 10f),
            new Vector3(14f, 0f, 12f),
            new Vector3(38f, 0f, 20f),
            new Vector3(64f, 0f, 16f),
            new Vector3(28f, 0f, -8f),
            new Vector3(-6f, 0f, -6f)
        };

        var points = new Transform[positions.Length];
        for (var i = 0; i < positions.Length; i++)
        {
            var point = new GameObject("PatrolPoint_" + i).transform;
            point.SetParent(parent);
            point.position = positions[i];
            points[i] = point;
        }

        return points;
    }

    private static Camera CreateFollowCamera(Transform target)
    {
        var cameraObject = new GameObject("Main Camera", typeof(Camera), typeof(AudioListener), typeof(CameraFollow));
        cameraObject.tag = "MainCamera";
        var camera = cameraObject.GetComponent<Camera>();
        camera.backgroundColor = new Color(0.66f, 0.82f, 0.94f);
        camera.clearFlags = CameraClearFlags.SolidColor;

        var follow = cameraObject.GetComponent<CameraFollow>();
        follow.target = target;
        follow.offset = new Vector3(0f, 16f, -14f);
        follow.positionLerp = 5f;
        follow.rotationLerp = 6f;
        return camera;
    }

    private static void BuildGameplayUI(GameObject player, MissionManager missionManager)
    {
        var playerDisguise = player.GetComponent<PlayerDisguise>();
        var suspicion = player.GetComponent<SuspicionSystem>();

        CreateEventSystem();
        var canvas = CreateCanvas("HUD");

        var timerRoot = new GameObject("TimerUI", typeof(RectTransform), typeof(TimerUI));
        timerRoot.transform.SetParent(canvas.transform, false);
        var timerText = CreateTextElement("Label", timerRoot.transform, "Timer 03:00", 28, TextAlignmentOptions.Center);
        Stretch((RectTransform)timerRoot.transform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(120f, -40f), new Vector2(220f, 50f));
        Stretch((RectTransform)timerText.transform, new Vector2(0f, 0f), new Vector2(1f, 1f), Vector2.zero, Vector2.zero);
        timerRoot.GetComponent<TimerUI>().timerLabel = timerText;

        var missionRoot = new GameObject("MissionUI", typeof(RectTransform), typeof(MissionUI));
        missionRoot.transform.SetParent(canvas.transform, false);
        Stretch((RectTransform)missionRoot.transform, new Vector2(0f, 0f), new Vector2(0f, 0f), new Vector2(180f, 140f), new Vector2(320f, 90f));
        var missionBg = CreateImageElement("Background", missionRoot.transform, new Color(0f, 0f, 0f, 0.35f));
        Stretch((RectTransform)missionBg.transform, new Vector2(0f, 0f), new Vector2(1f, 1f), Vector2.zero, Vector2.zero);
        var missionText = CreateTextElement("Label", missionRoot.transform, "Mission: --", 22, TextAlignmentOptions.TopLeft);
        Stretch((RectTransform)missionText.transform, new Vector2(0f, 0f), new Vector2(1f, 1f), new Vector2(14f, -10f), new Vector2(-28f, -28f));
        var missionProgressBg = CreateImageElement("ProgressBg", missionRoot.transform, new Color(1f, 1f, 1f, 0.12f));
        Stretch((RectTransform)missionProgressBg.transform, new Vector2(0f, 0f), new Vector2(1f, 0f), new Vector2(0f, 12f), new Vector2(-24f, 12f));
        var missionProgressFill = CreateImageElement("ProgressFill", missionProgressBg.transform, new Color(0.25f, 0.80f, 0.50f, 0.95f));
        missionProgressFill.type = Image.Type.Filled;
        missionProgressFill.fillMethod = Image.FillMethod.Horizontal;
        Stretch((RectTransform)missionProgressFill.transform, new Vector2(0f, 0f), new Vector2(1f, 1f), Vector2.zero, Vector2.zero);
        var missionUi = missionRoot.GetComponent<MissionUI>();
        missionUi.missionManager = missionManager;
        missionUi.missionLabel = missionText;
        missionUi.progressFill = missionProgressFill;

        var suspicionRoot = new GameObject("SuspicionUI", typeof(RectTransform), typeof(SuspicionMeterUI));
        suspicionRoot.transform.SetParent(canvas.transform, false);
        Stretch((RectTransform)suspicionRoot.transform, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 112f), new Vector2(360f, 52f));
        var suspicionBg = CreateImageElement("Background", suspicionRoot.transform, new Color(0f, 0f, 0f, 0.35f));
        Stretch((RectTransform)suspicionBg.transform, new Vector2(0f, 0f), new Vector2(1f, 1f), Vector2.zero, Vector2.zero);
        var suspicionFill = CreateImageElement("Fill", suspicionBg.transform, new Color(0.90f, 0.80f, 0.10f, 0.95f));
        suspicionFill.type = Image.Type.Filled;
        suspicionFill.fillMethod = Image.FillMethod.Horizontal;
        suspicionFill.fillAmount = 0f;
        Stretch((RectTransform)suspicionFill.transform, new Vector2(0f, 0f), new Vector2(1f, 1f), Vector2.zero, Vector2.zero);
        var suspicionLabel = CreateTextElement("Value", suspicionRoot.transform, "Suspicion 0", 22, TextAlignmentOptions.Center);
        Stretch((RectTransform)suspicionLabel.transform, new Vector2(0f, 0f), new Vector2(1f, 1f), Vector2.zero, Vector2.zero);
        var suspicionUi = suspicionRoot.GetComponent<SuspicionMeterUI>();
        suspicionUi.suspicionSystem = suspicion;
        suspicionUi.fillImage = suspicionFill;
        suspicionUi.valueLabel = suspicionLabel;
        suspicionUi.fillGradient = CreateSuspicionGradient();

        var disguiseButton = CreateButtonElement("DisguiseButton", canvas.transform, new Color(0.12f, 0.22f, 0.32f, 0.85f));
        Stretch((RectTransform)disguiseButton.transform, new Vector2(1f, 0f), new Vector2(1f, 0f), new Vector2(-100f, 170f), new Vector2(150f, 72f));
        var disguiseLabel = CreateTextElement("Label", disguiseButton.transform, "Disguise", 22, TextAlignmentOptions.Center);
        Stretch((RectTransform)disguiseLabel.transform, new Vector2(0f, 0f), new Vector2(1f, 1f), new Vector2(0f, 14f), new Vector2(-12f, -22f));
        var disguiseProgress = CreateImageElement("Progress", disguiseButton.transform, new Color(0.20f, 0.75f, 0.45f, 0.9f));
        disguiseProgress.type = Image.Type.Filled;
        disguiseProgress.fillMethod = Image.FillMethod.Horizontal;
        Stretch((RectTransform)disguiseProgress.transform, new Vector2(0f, 0f), new Vector2(1f, 0f), new Vector2(0f, 8f), new Vector2(-20f, 10f));
        var disguiseCharges = CreateTextElement("Charges", disguiseButton.transform, "Disguise x3", 18, TextAlignmentOptions.Center);
        Stretch((RectTransform)disguiseCharges.transform, new Vector2(0f, 0f), new Vector2(1f, 1f), new Vector2(0f, -14f), new Vector2(-12f, -24f));
        var disguiseUiGo = new GameObject("DisguiseUI", typeof(RectTransform), typeof(DisguiseUI));
        disguiseUiGo.transform.SetParent(canvas.transform, false);
        var disguiseUi = disguiseUiGo.GetComponent<DisguiseUI>();
        disguiseUi.playerDisguise = playerDisguise;
        disguiseUi.disguiseButton = disguiseButton;
        disguiseUi.progressFill = disguiseProgress;
        disguiseUi.chargesLabel = disguiseCharges;

        var minimapButton = CreateButtonElement("MinimapButton", canvas.transform, new Color(0.05f, 0.08f, 0.10f, 0.75f));
        Stretch((RectTransform)minimapButton.transform, new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(-110f, -72f), new Vector2(190f, 190f));
        var minimapText = CreateTextElement("Label", minimapButton.transform, "MINIMAP", 20, TextAlignmentOptions.Center);
        Stretch((RectTransform)minimapText.transform, new Vector2(0f, 0f), new Vector2(1f, 1f), Vector2.zero, Vector2.zero);
        var minimapUiGo = new GameObject("MinimapUI", typeof(RectTransform), typeof(MinimapUI));
        minimapUiGo.transform.SetParent(canvas.transform, false);
        var minimapUi = minimapUiGo.GetComponent<MinimapUI>();
        minimapUi.minimapRoot = (RectTransform)minimapButton.transform;
        minimapUi.toggleButton = minimapButton;

        var gameOverOverlay = CreateImageElement("GameOverOverlay", canvas.transform, new Color(0f, 0f, 0f, 0.7f));
        Stretch((RectTransform)gameOverOverlay.transform, new Vector2(0f, 0f), new Vector2(1f, 1f), Vector2.zero, Vector2.zero);
        var gameOverTitle = CreateTextElement("Title", gameOverOverlay.transform, "Caught", 46, TextAlignmentOptions.Center);
        Stretch((RectTransform)gameOverTitle.transform, new Vector2(0.5f, 0.65f), new Vector2(0.5f, 0.65f), Vector2.zero, new Vector2(700f, 80f));
        var gameOverScore = CreateTextElement("Score", gameOverOverlay.transform, "Score 0", 30, TextAlignmentOptions.Center);
        Stretch((RectTransform)gameOverScore.transform, new Vector2(0.5f, 0.54f), new Vector2(0.5f, 0.54f), Vector2.zero, new Vector2(500f, 60f));
        var gameOverSummary = CreateTextElement("Summary", gameOverOverlay.transform, "Summary", 22, TextAlignmentOptions.Center);
        Stretch((RectTransform)gameOverSummary.transform, new Vector2(0.5f, 0.46f), new Vector2(0.5f, 0.46f), Vector2.zero, new Vector2(800f, 60f));
        var gameOverUiGo = new GameObject("GameOverUI", typeof(RectTransform), typeof(GameOverUI));
        gameOverUiGo.transform.SetParent(canvas.transform, false);
        var gameOverUi = gameOverUiGo.GetComponent<GameOverUI>();
        gameOverUi.root = gameOverOverlay.gameObject;
        gameOverUi.titleLabel = gameOverTitle;
        gameOverUi.scoreLabel = gameOverScore;
        gameOverUi.summaryLabel = gameOverSummary;
        gameOverOverlay.gameObject.SetActive(false);

        CreateJoystick(canvas.transform);
    }

    private static Canvas CreateCanvas(string name)
    {
        var canvasGo = new GameObject(name, typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        var canvas = canvasGo.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.pixelPerfect = false;

        var scaler = canvasGo.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.matchWidthOrHeight = 0.5f;
        return canvas;
    }

    private static void CreateEventSystem()
    {
        var eventSystem = Object.FindFirstObjectByType<EventSystem>();
        if (eventSystem == null)
        {
            eventSystem = new GameObject("EventSystem", typeof(EventSystem)).GetComponent<EventSystem>();
        }

        var standaloneModule = eventSystem.GetComponent<StandaloneInputModule>();
        if (standaloneModule != null)
        {
            Object.DestroyImmediate(standaloneModule);
        }

        if (eventSystem.GetComponent<InputSystemUIInputModule>() == null)
        {
            eventSystem.gameObject.AddComponent<InputSystemUIInputModule>();
        }
    }

    private static void CreateJoystick(Transform canvas)
    {
        var joystickRoot = new GameObject("JoystickUI", typeof(RectTransform), typeof(JoystickUI));
        joystickRoot.transform.SetParent(canvas, false);
        Stretch((RectTransform)joystickRoot.transform, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
        var background = CreateImageElement("Background", joystickRoot.transform, new Color(0f, 0f, 0f, 0.28f));
        background.raycastTarget = true;
        Stretch((RectTransform)background.transform, new Vector2(0f, 0f), new Vector2(0f, 0f), new Vector2(140f, 140f), new Vector2(180f, 180f));
        var handle = CreateImageElement("Handle", background.transform, new Color(1f, 1f, 1f, 0.75f));
        handle.raycastTarget = false;
        Stretch((RectTransform)handle.transform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(72f, 72f));

        var joystick = joystickRoot.GetComponent<JoystickUI>();
        joystick.background = (RectTransform)background.transform;
        joystick.handle = (RectTransform)handle.transform;
        joystick.maxRadius = 70f;
    }

    private static Gradient CreateSuspicionGradient()
    {
        var gradient = new Gradient();
        gradient.SetKeys(
            new[]
            {
                new GradientColorKey(new Color(0.25f, 0.85f, 0.45f), 0f),
                new GradientColorKey(new Color(0.95f, 0.80f, 0.15f), 0.5f),
                new GradientColorKey(new Color(0.95f, 0.25f, 0.20f), 1f)
            },
            new[]
            {
                new GradientAlphaKey(1f, 0f),
                new GradientAlphaKey(1f, 1f)
            });
        return gradient;
    }

    private static void CreateDirectionalLight()
    {
        var lightObject = new GameObject("Directional Light", typeof(Light));
        var light = lightObject.GetComponent<Light>();
        light.type = LightType.Directional;
        light.intensity = 1.15f;
        lightObject.transform.rotation = Quaternion.Euler(48f, -32f, 0f);
    }

    private static Camera CreateStaticCamera(Vector3 position, Vector3 rotation)
    {
        var cameraObject = new GameObject("Main Camera", typeof(Camera), typeof(AudioListener));
        cameraObject.tag = "MainCamera";
        cameraObject.transform.position = position;
        cameraObject.transform.rotation = Quaternion.Euler(rotation);
        return cameraObject.GetComponent<Camera>();
    }

    private static GameObject CreatePrimitiveGround(Vector3 position, Vector3 scale, Color color, Material material = null)
    {
        var ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
        ground.name = "Ground";
        ground.transform.position = position;
        ground.transform.localScale = scale;
        var renderer = ground.GetComponent<Renderer>();
        if (material != null)
        {
            renderer.sharedMaterial = material;
        }
        else if (renderer != null)
        {
            renderer.sharedMaterial.color = color;
        }

        return ground;
    }

    private static void CreateRoad(Transform parent, BlendInArtSet artSet, string name, Vector3 position, Vector3 scale, Material material)
    {
        var road = GameObject.CreatePrimitive(PrimitiveType.Cube);
        road.name = name;
        road.transform.SetParent(parent);
        road.transform.position = position;
        road.transform.localScale = scale;
        var renderer = road.GetComponent<Renderer>();
        if (renderer != null)
        {
            renderer.sharedMaterial = material != null
                ? material
                : CreateOrUpdateMaterial(MaterialRoot + "/Road.mat", new Color(0.22f, 0.23f, 0.25f));
        }

        var visualPrefab = PickRoadPrefabForName(name, artSet);
        if (visualPrefab != null)
        {
            if (renderer != null)
            {
                renderer.enabled = false;
            }

            AttachFittedVisual(road.transform, visualPrefab, new Vector3(scale.x, 0.3f, scale.z), 0.98f);
        }
    }

    private static void CreateBuilding(Transform parent, BlendInArtSet artSet, string name, Vector3 position, Vector3 scale, Material material)
    {
        var building = GameObject.CreatePrimitive(PrimitiveType.Cube);
        building.name = name;
        building.transform.SetParent(parent);
        building.transform.position = position;
        building.transform.localScale = scale;
        var renderer = building.GetComponent<Renderer>();
        if (renderer != null && material != null)
        {
            renderer.sharedMaterial = material;
        }

        var visualPrefab = PickBuildingPrefabForName(name, artSet);
        if (visualPrefab != null)
        {
            if (renderer != null)
            {
                renderer.enabled = false;
            }

            AttachFittedVisual(building.transform, visualPrefab, GetBuildingVisualTargetSize(name, scale), GetBuildingVisualFill(name));
        }
    }

    private static void CreatePlaza(Transform parent, BlendInBootstrapAssets assets)
    {
        var plaza = GameObject.CreatePrimitive(PrimitiveType.Cube);
        plaza.name = "Plaza";
        plaza.transform.SetParent(parent);
        plaza.transform.position = new Vector3(0f, 0.1f, 0f);
        plaza.transform.localScale = new Vector3(28f, 0.2f, 28f);
        var renderer = plaza.GetComponent<Renderer>();
        var hasPlazaVisuals = assets.ArtSet != null
            && (assets.ArtSet.plazaVisualPrefab != null
                || (assets.ArtSet.sidewalkVisualPrefabs != null && assets.ArtSet.sidewalkVisualPrefabs.Length > 0));
        if (renderer != null)
        {
            renderer.sharedMaterial = assets.GroundMaterial != null ? assets.GroundMaterial : assets.BuildingMaterial;
            renderer.enabled = !hasPlazaVisuals;
        }

        if (assets.ArtSet != null && assets.ArtSet.plazaVisualPrefab != null)
        {
            AttachFittedVisual(plaza.transform, assets.ArtSet.plazaVisualPrefab, new Vector3(14f, 4f, 14f), 0.95f);
        }

        if (assets.ArtSet != null && assets.ArtSet.sidewalkVisualPrefabs != null && assets.ArtSet.sidewalkVisualPrefabs.Length > 0)
        {
            PlacePropStrip(plaza.transform, assets.ArtSet.sidewalkVisualPrefabs, new Vector3(-10f, 0.15f, 0f), new Vector3(10f, 0f, 0f), 3, new Vector3(8f, 0.6f, 8f), 0.85f);
            PlacePropStrip(plaza.transform, assets.ArtSet.sidewalkVisualPrefabs, new Vector3(10f, 0.15f, 0f), new Vector3(-10f, 0f, 0f), 3, new Vector3(8f, 0.6f, 8f), 0.85f);
            PlacePropStrip(plaza.transform, assets.ArtSet.sidewalkVisualPrefabs, new Vector3(0f, 0.15f, -10f), new Vector3(0f, 0f, 10f), 3, new Vector3(8f, 0.6f, 8f), 0.85f);
            PlacePropStrip(plaza.transform, assets.ArtSet.sidewalkVisualPrefabs, new Vector3(0f, 0.15f, 10f), new Vector3(0f, 0f, -10f), 3, new Vector3(8f, 0.6f, 8f), 0.85f);
        }
    }

    private static void CreatePark(Transform parent, BlendInBootstrapAssets assets)
    {
        var park = GameObject.CreatePrimitive(PrimitiveType.Cube);
        park.name = "Park";
        park.transform.SetParent(parent);
        park.transform.position = new Vector3(44f, 0.08f, -46f);
        park.transform.localScale = new Vector3(32f, 0.16f, 26f);
        var renderer = park.GetComponent<Renderer>();
        if (renderer != null)
        {
            renderer.sharedMaterial = assets.GroundMaterial;
        }

        if (assets.ArtSet != null && assets.ArtSet.parkVisualPrefabs != null && assets.ArtSet.parkVisualPrefabs.Length > 0)
        {
            PlacePropStrip(park.transform, assets.ArtSet.parkVisualPrefabs, new Vector3(-10f, 0.1f, -6f), new Vector3(20f, 0f, 0f), 4, new Vector3(4f, 8f, 4f), 0.9f);
            PlacePropStrip(park.transform, assets.ArtSet.parkVisualPrefabs, new Vector3(-12f, 0.1f, 8f), new Vector3(24f, 0f, -10f), 5, new Vector3(4f, 8f, 4f), 0.9f);
        }
    }

    private static void CreateDestination(Transform parent, string name, string zoneTag, Vector3 position, bool shelter = false, bool crowdZone = false, bool createSitPoint = false)
    {
        var root = new GameObject(name);
        root.transform.SetParent(parent);
        root.transform.position = position;

        var point = root.AddComponent<DestinationPoint>();
        point.zoneTag = zoneTag;
        point.countsAsShelter = shelter;
        point.countsAsCrowdZone = crowdZone;

        var marker = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        marker.name = "Marker";
        marker.transform.SetParent(root.transform);
        marker.transform.localPosition = new Vector3(0f, 0.4f, 0f);
        marker.transform.localScale = Vector3.one * 0.8f;
        var markerCollider = marker.GetComponent<Collider>();
        if (markerCollider != null)
        {
            Object.DestroyImmediate(markerCollider);
        }
        var markerRenderer = marker.GetComponent<Renderer>();
        if (markerRenderer != null)
        {
            markerRenderer.enabled = false;
        }

        var standPoints = new Transform[4];
        for (var i = 0; i < standPoints.Length; i++)
        {
            var stand = new GameObject("Stand_" + i).transform;
            stand.SetParent(root.transform);
            var angle = Mathf.Deg2Rad * (90f * i);
            stand.localPosition = new Vector3(Mathf.Cos(angle) * 1.5f, 0f, Mathf.Sin(angle) * 1.5f);
            standPoints[i] = stand;
        }

        point.standPoints = standPoints;

        if (createSitPoint)
        {
            var sit = new GameObject("Sit_0").transform;
            sit.SetParent(root.transform);
            sit.localPosition = new Vector3(0f, 0f, 0f);
            point.sitPoints = new[] { sit };
        }
    }

    private static void CreateSpawnZone(Transform parent, string name, string zoneTag, Vector3 position, Vector3 size)
    {
        var root = new GameObject(name);
        root.transform.SetParent(parent);
        root.transform.position = position;
        var zone = root.AddComponent<SpawnZone>();
        zone.zoneTag = zoneTag;
        zone.size = size;
    }

    private static void CreateMissionTrigger(Transform parent, string name, string zoneTag, Vector3 position, Vector3 size, bool sheltered)
    {
        var trigger = new GameObject(name, typeof(BoxCollider), typeof(MissionTrigger));
        trigger.transform.SetParent(parent);
        trigger.transform.position = position;
        var collider = trigger.GetComponent<BoxCollider>();
        collider.isTrigger = true;
        collider.size = size;
        var missionTrigger = trigger.GetComponent<MissionTrigger>();
        missionTrigger.zoneTag = zoneTag;
        missionTrigger.countsAsShelter = sheltered;
    }

    private static void BuildSceneNavMesh(GameObject environmentRoot)
    {
        if (environmentRoot == null)
        {
            return;
        }

        var surface = environmentRoot.GetComponent<NavMeshSurface>();
        if (surface == null)
        {
            surface = environmentRoot.AddComponent<NavMeshSurface>();
        }

        surface.collectObjects = CollectObjects.All;
        surface.useGeometry = NavMeshCollectGeometry.PhysicsColliders;
        surface.layerMask = ~0;
        surface.overrideTileSize = false;
        surface.overrideVoxelSize = false;
        surface.BuildNavMesh();
    }

    private static void CreateStreetProps(Transform parent, BlendInArtSet artSet)
    {
        if (artSet == null)
        {
            return;
        }

        if (artSet.busStopVisualPrefab != null)
        {
            PlacePropInstance(parent, artSet.busStopVisualPrefab, new Vector3(72f, 0f, 14f), new Vector3(6f, 5f, 4f), 0.95f);
        }

        if (artSet.streetPropPrefabs != null && artSet.streetPropPrefabs.Length > 0)
        {
            PlacePropStrip(parent, artSet.streetPropPrefabs, new Vector3(-36f, 0f, 6f), new Vector3(24f, 0f, 0f), 4, new Vector3(2f, 4f, 2f), 0.85f);
            PlacePropStrip(parent, artSet.streetPropPrefabs, new Vector3(10f, 0f, 16f), new Vector3(0f, 0f, -24f), 4, new Vector3(2f, 4f, 2f), 0.85f);
        }
    }

    private static GameObject PickPrefabForName(string seed, GameObject[] prefabs)
    {
        if (prefabs == null || prefabs.Length == 0)
        {
            return null;
        }

        var index = Mathf.Abs(seed.GetHashCode()) % prefabs.Length;
        return prefabs[index];
    }

    private static GameObject PickBuildingPrefabForName(string seed, BlendInArtSet artSet)
    {
        var prefabs = artSet != null ? artSet.buildingVisualPrefabs : null;
        if (prefabs == null || prefabs.Length == 0)
        {
            return null;
        }

        var loweredSeed = string.IsNullOrEmpty(seed) ? string.Empty : seed.ToLowerInvariant();
        var filtered = prefabs;

        if (ContainsAny(loweredSeed, "cafe", "convenience", "restaurant", "shop"))
        {
            filtered = FilterPrefabsByAssetPath(prefabs, "building_1", "building_2", "building_3", "building_4", "terrace");
        }
        else if (ContainsAny(loweredSeed, "apartment", "home"))
        {
            filtered = FilterPrefabsByAssetPath(prefabs, "building_4", "building_5", "building_6", "building_7", "slope", "terrace");
        }
        else if (ContainsAny(loweredSeed, "office", "school"))
        {
            filtered = FilterPrefabsByAssetPath(prefabs, "building_7", "building_8", "building_9", "slope", "terrace");
        }

        if (filtered.Length == 0)
        {
            filtered = FilterOutPrefabsByAssetPath(prefabs, "grid", "twistedtower");
        }

        if (filtered.Length == 0)
        {
            filtered = prefabs;
        }

        var index = Mathf.Abs(seed.GetHashCode()) % filtered.Length;
        return filtered[index];
    }

    private static GameObject PickRoadPrefabForName(string seed, BlendInArtSet artSet)
    {
        var prefabs = artSet != null ? artSet.roadVisualPrefabs : null;
        if (prefabs == null || prefabs.Length == 0)
        {
            return null;
        }

        var loweredSeed = string.IsNullOrEmpty(seed) ? string.Empty : seed.ToLowerInvariant();
        GameObject[] filtered;
        if (ContainsAny(loweredSeed, "main"))
        {
            filtered = FilterPrefabsByAssetPath(prefabs, "crossroad", "road_2", "road_3", "road_001", "road_003");
        }
        else if (ContainsAny(loweredSeed, "east", "west"))
        {
            filtered = FilterPrefabsByAssetPath(prefabs, "road_1", "road_2", "road_001", "road_003", "road_013");
        }
        else
        {
            filtered = FilterOutPrefabsByAssetPath(prefabs, "railroad", "roadworks", "stopline", "crosswalk");
        }

        if (filtered.Length == 0)
        {
            filtered = FilterOutPrefabsByAssetPath(prefabs, "railroad", "roadworks");
        }

        if (filtered.Length == 0)
        {
            filtered = prefabs;
        }

        var index = Mathf.Abs(seed.GetHashCode()) % filtered.Length;
        return filtered[index];
    }

    private static void PlacePropStrip(Transform parent, GameObject[] prefabs, Vector3 startLocalPosition, Vector3 deltaLocalPosition, int count, Vector3 targetSize, float fill)
    {
        if (prefabs == null || prefabs.Length == 0 || count <= 0)
        {
            return;
        }

        for (var i = 0; i < count; i++)
        {
            var localPosition = startLocalPosition + deltaLocalPosition * (count == 1 ? 0f : i / (float)(count - 1));
            var prefab = prefabs[i % prefabs.Length];
            PlacePropInstance(parent, prefab, parent.TransformPoint(localPosition), targetSize, fill);
        }
    }

    private static void PlacePropInstance(Transform parent, GameObject prefab, Vector3 worldPosition, Vector3 targetSize, float fill)
    {
        if (parent == null || prefab == null)
        {
            return;
        }

        var instance = PrefabUtility.InstantiatePrefab(prefab) as GameObject;
        if (instance == null)
        {
            instance = Object.Instantiate(prefab);
        }

        instance.transform.SetParent(parent, true);
        instance.transform.position = worldPosition;
        instance.transform.rotation = Quaternion.identity;
        FitInstanceToSize(instance, targetSize, fill, worldPosition.y);
    }

    private static void AttachFittedVisual(Transform parent, GameObject prefab, Vector3 targetSize, float fill)
    {
        if (parent == null || prefab == null)
        {
            return;
        }

        var instance = PrefabUtility.InstantiatePrefab(prefab) as GameObject;
        if (instance == null)
        {
            instance = Object.Instantiate(prefab);
        }

        instance.transform.SetParent(parent, true);
        instance.transform.rotation = Quaternion.identity;
        FitInstanceToSize(instance, targetSize, fill, parent.position.y - (targetSize.y * 0.5f));
    }

    private static void FitInstanceToSize(GameObject instance, Vector3 targetSize, float fill, float groundY)
    {
        if (instance == null)
        {
            return;
        }

        instance.transform.localScale = Vector3.one;
        var renderers = instance.GetComponentsInChildren<Renderer>(true);
        if (renderers.Length == 0)
        {
            return;
        }

        var bounds = renderers[0].bounds;
        for (var i = 1; i < renderers.Length; i++)
        {
            bounds.Encapsulate(renderers[i].bounds);
        }

        var size = bounds.size;
        if (size.x <= 0.001f || size.y <= 0.001f || size.z <= 0.001f)
        {
            return;
        }

        var scaleFactor = Mathf.Min(targetSize.x / size.x, targetSize.y / size.y, targetSize.z / size.z) * fill;
        instance.transform.localScale = Vector3.one * scaleFactor;

        bounds = renderers[0].bounds;
        for (var i = 1; i < renderers.Length; i++)
        {
            bounds.Encapsulate(renderers[i].bounds);
        }

        var centerTarget = new Vector3(instance.transform.position.x, groundY, instance.transform.position.z);
        var offset = new Vector3(centerTarget.x - bounds.center.x, groundY - bounds.min.y, centerTarget.z - bounds.center.z);
        instance.transform.position += offset;
    }

    private static Vector3 GetBuildingVisualTargetSize(string name, Vector3 baseScale)
    {
        var target = baseScale;
        var loweredName = string.IsNullOrEmpty(name) ? string.Empty : name.ToLowerInvariant();

        if (ContainsAny(loweredName, "cafe", "convenience", "restaurant", "shop"))
        {
            target.y *= 0.58f;
            target.x *= 0.92f;
            target.z *= 0.92f;
        }
        else if (ContainsAny(loweredName, "apartment", "home"))
        {
            target.y *= 0.82f;
        }
        else if (ContainsAny(loweredName, "office", "school"))
        {
            target.y *= 0.95f;
        }

        return target;
    }

    private static float GetBuildingVisualFill(string name)
    {
        var loweredName = string.IsNullOrEmpty(name) ? string.Empty : name.ToLowerInvariant();
        if (ContainsAny(loweredName, "cafe", "convenience", "restaurant", "shop"))
        {
            return 0.82f;
        }

        if (ContainsAny(loweredName, "apartment", "home"))
        {
            return 0.88f;
        }

        return 0.92f;
    }

    private static GameObject[] FilterPrefabsByAssetPath(GameObject[] prefabs, params string[] tokens)
    {
        if (prefabs == null || prefabs.Length == 0)
        {
            return System.Array.Empty<GameObject>();
        }

        return prefabs
            .Where(prefab => prefab != null && ContainsAny(AssetDatabase.GetAssetPath(prefab).ToLowerInvariant(), tokens))
            .ToArray();
    }

    private static GameObject[] FilterOutPrefabsByAssetPath(GameObject[] prefabs, params string[] tokens)
    {
        if (prefabs == null || prefabs.Length == 0)
        {
            return System.Array.Empty<GameObject>();
        }

        return prefabs
            .Where(prefab => prefab != null && !ContainsAny(AssetDatabase.GetAssetPath(prefab).ToLowerInvariant(), tokens))
            .ToArray();
    }

    private static bool ContainsAny(string value, params string[] tokens)
    {
        if (string.IsNullOrEmpty(value) || tokens == null)
        {
            return false;
        }

        for (var i = 0; i < tokens.Length; i++)
        {
            if (value.Contains(tokens[i]))
            {
                return true;
            }
        }

        return false;
    }

    private static void ApplyBuildSettings()
    {
        EditorBuildSettings.scenes = new[]
        {
            new EditorBuildSettingsScene(SceneRoot + "/MainMenu.unity", true),
            new EditorBuildSettingsScene(SceneRoot + "/GameScene.unity", true),
            new EditorBuildSettingsScene(SceneRoot + "/ResultScene.unity", true)
        };
    }
}
#endif
