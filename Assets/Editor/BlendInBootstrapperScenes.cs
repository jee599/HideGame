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
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public static partial class BlendInBootstrapper
{
    private static void CreateScenes(BlendInBootstrapAssets assets)
    {
        CreateMainMenuScene(assets);
        CreateResultScene(assets);
        CreateGameScene(assets);
        ApplyBuildSettings();
    }

    private static void CreateMainMenuScene(BlendInBootstrapAssets assets)
    {
        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

        CreateDirectionalLight();
        CreateAtmosphere();
        var camera = CreateStaticCamera(new Vector3(-14f, 11f, -18f), new Vector3(26f, 32f, 0f));
        camera.backgroundColor = new Color(0.66f, 0.82f, 0.94f);
        camera.clearFlags = CameraClearFlags.SolidColor;
        camera.fieldOfView = 56f;
        CreateMenuBackdrop(assets);
        CreateEventSystem();
        var audioDirector = new GameObject("GameAudioDirector").AddComponent<GameAudioDirector>();
        ConfigureSharedAudio(audioDirector);

        var canvas = CreateCanvas("MainMenuCanvas");
        var panelColor = new Color(0.05f, 0.08f, 0.12f, 0.80f);
        var card = CreateImageElement("Card", canvas.transform, panelColor);
        Stretch((RectTransform)card.transform, new Vector2(0f, 0f), new Vector2(0f, 1f), new Vector2(48f, 0f), new Vector2(640f, -120f));

        var title = CreateTextElement("Title", canvas.transform, "BLEND IN", 52, TextAlignmentOptions.Center);
        Stretch((RectTransform)title.transform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(368f, -132f), new Vector2(520f, 82f));

        var subtitle = CreateTextElement("Subtitle", canvas.transform, "Move like the crowd, steal routines, and survive until nightfall.", 25, TextAlignmentOptions.TopLeft);
        Stretch((RectTransform)subtitle.transform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(368f, -210f), new Vector2(520f, 80f));

        var status = CreateTextElement("Status", canvas.transform, "3 minutes. 100 citizens. 1 hunter.", 24, TextAlignmentOptions.TopLeft);
        Stretch((RectTransform)status.transform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(368f, -274f), new Vector2(520f, 52f));

        var instructions = CreateTextElement(
            "Instructions",
            canvas.transform,
            "Watch the city first.\nBlend into busy zones.\nStop in mission areas to score.\nUse disguise sparingly when sightlines break.",
            22,
            TextAlignmentOptions.TopLeft);
        Stretch((RectTransform)instructions.transform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(368f, -386f), new Vector2(520f, 210f));

        var startButton = CreateMenuButton(canvas.transform, "StartButton", "Start Run", new Color(0.20f, 0.68f, 0.42f, 0.96f));
        Stretch((RectTransform)startButton.transform, new Vector2(0f, 0f), new Vector2(0f, 0f), new Vector2(220f, 132f), new Vector2(320f, 72f));
        var quitButton = CreateMenuButton(canvas.transform, "QuitButton", "Quit", new Color(0.18f, 0.24f, 0.32f, 0.96f));
        Stretch((RectTransform)quitButton.transform, new Vector2(0f, 0f), new Vector2(0f, 0f), new Vector2(220f, 44f), new Vector2(320f, 60f));

        var controllerGo = new GameObject("MainMenuController", typeof(RectTransform), typeof(MainMenuController));
        controllerGo.transform.SetParent(canvas.transform, false);
        var controller = controllerGo.GetComponent<MainMenuController>();
        controller.startButton = startButton;
        controller.quitButton = quitButton;
        controller.statusLabel = status;

        EditorSceneManager.SaveScene(scene, SceneRoot + "/MainMenu.unity");
    }

    private static void CreateResultScene(BlendInBootstrapAssets assets)
    {
        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

        CreateDirectionalLight();
        CreateAtmosphere();
        var camera = CreateStaticCamera(new Vector3(-12f, 10f, -17f), new Vector3(24f, 34f, 0f));
        camera.backgroundColor = new Color(0.66f, 0.82f, 0.94f);
        camera.clearFlags = CameraClearFlags.SolidColor;
        camera.fieldOfView = 58f;
        CreateMenuBackdrop(assets);
        CreateEventSystem();
        var audioDirector = new GameObject("GameAudioDirector").AddComponent<GameAudioDirector>();
        ConfigureSharedAudio(audioDirector);

        var canvas = CreateCanvas("ResultCanvas");
        var overlay = CreateImageElement("Overlay", canvas.transform, new Color(0.04f, 0.06f, 0.10f, 0.78f));
        Stretch((RectTransform)overlay.transform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(860f, 520f));
        var title = CreateTextElement("Title", overlay.transform, "Blend Successful", 46, TextAlignmentOptions.Center);
        Stretch((RectTransform)title.transform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -78f), new Vector2(700f, 72f));
        var score = CreateTextElement("Score", overlay.transform, "Score 0", 34, TextAlignmentOptions.Center);
        Stretch((RectTransform)score.transform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -148f), new Vector2(520f, 54f));
        var summary = CreateTextElement("Summary", overlay.transform, "You survived until 20:00.", 24, TextAlignmentOptions.Center);
        Stretch((RectTransform)summary.transform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, 18f), new Vector2(680f, 96f));
        var detail = CreateTextElement("Detail", overlay.transform, "Missions 0   Peak Suspicion 0   Disguises 0", 20, TextAlignmentOptions.Center);
        Stretch((RectTransform)detail.transform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, -44f), new Vector2(700f, 44f));
        var retryButton = CreateMenuButton(overlay.transform, "RetryButton", "Retry", new Color(0.20f, 0.68f, 0.42f, 0.96f));
        Stretch((RectTransform)retryButton.transform, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(-110f, 74f), new Vector2(220f, 62f));
        var menuButton = CreateMenuButton(overlay.transform, "MenuButton", "Main Menu", new Color(0.18f, 0.24f, 0.32f, 0.96f));
        Stretch((RectTransform)menuButton.transform, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(110f, 74f), new Vector2(220f, 62f));

        var controllerGo = new GameObject("ResultSceneController", typeof(RectTransform), typeof(ResultSceneController));
        controllerGo.transform.SetParent(canvas.transform, false);
        var controller = controllerGo.GetComponent<ResultSceneController>();
        controller.titleLabel = title;
        controller.scoreLabel = score;
        controller.summaryLabel = summary;
        controller.detailLabel = detail;
        controller.retryButton = retryButton;
        controller.menuButton = menuButton;

        EditorSceneManager.SaveScene(scene, SceneRoot + "/ResultScene.unity");
    }

    private static void CreateGameScene(BlendInBootstrapAssets assets)
    {
        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

        var environmentRoot = new GameObject("Environment").transform;
        var markerRoot = new GameObject("WorldMarkers").transform;
        var routeRoot = new GameObject("HunterRoute").transform;

        CreateDirectionalLight();
        CreateAtmosphere();
        CreateGrayboxMap(assets, environmentRoot, markerRoot);
        BuildSceneNavMesh(environmentRoot.gameObject);

        var player = (GameObject)PrefabUtility.InstantiatePrefab(assets.PlayerPrefab);
        player.name = "Player";
        player.transform.position = new Vector3(-34f, 0.05f, 18f);

        var hunter = (GameObject)PrefabUtility.InstantiatePrefab(assets.HunterPrefab);
        hunter.name = "Hunter";
        hunter.transform.position = new Vector3(-4f, 0.05f, 12f);
        BlendInBootstrapper.EnsureHunterPresentation(hunter, assets.HunterMaterial);

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
        var audioDirector = new GameObject("GameAudioDirector").AddComponent<GameAudioDirector>();
        audioDirector.transform.SetParent(systemsRoot);
        var vfxDirector = new GameObject("PrototypeVfxDirector").AddComponent<PrototypeVfxDirector>();
        vfxDirector.transform.SetParent(systemsRoot);
        var relationshipManager = new GameObject("RelationshipManager").AddComponent<RelationshipManager>();
        relationshipManager.transform.SetParent(systemsRoot);
        var citizenSpawner = new GameObject("CitizenSpawner").AddComponent<CitizenSpawner>();
        citizenSpawner.transform.SetParent(systemsRoot);

        eventManager.availableEvents = assets.Events;
        missionManager.missionPool = assets.Missions;
        citizenSpawner.citizenPrefab = assets.CitizenPrefab;
        citizenSpawner.archetypes = assets.Archetypes;
        ConfigureGameplayAudio(audioDirector);
        ConfigureGameplayVfx(vfxDirector);

        var routePoints = CreateHunterRoute(routeRoot);
        var hunterAi = hunter.GetComponent<HunterAI>();
        hunterAi.patrolRoute = routePoints;
        hunterAi.config = assets.HunterConfig;

        BuildGameplayUI(player, missionManager);
        EditorSceneManager.MarkSceneDirty(scene);

        EditorSceneManager.SaveScene(scene, SceneRoot + "/GameScene.unity");
    }

    private static void ConfigureSharedAudio(GameAudioDirector audioDirector)
    {
        if (audioDirector == null)
        {
            return;
        }

        audioDirector.uiClickClip = LoadAudioClip(
            "Assets/Imported/Kenney/UIAudio/Audio/click4.ogg",
            "Assets/Imported/Kenney/UIAudio/Audio/switch14.ogg");
        audioDirector.menuMusicClips = LoadAudioClipsFromFolder(
            "Assets/Imported/Kenney/MusicJingles/Audio/Pizzicato jingles",
            "Assets/Imported/Kenney/MusicJingles/Audio/8-Bit jingles");
        audioDirector.gameplayMusicClips = LoadAudioClipsFromFolder(
            "Assets/Imported/Kenney/MusicJingles/Audio/Steel jingles",
            "Assets/Imported/Kenney/MusicJingles/Audio/Sax jingles");
        audioDirector.resultMusicClips = LoadAudioClipsFromFolder(
            "Assets/Imported/Kenney/MusicJingles/Audio/Sax jingles",
            "Assets/Imported/Kenney/MusicJingles/Audio/Pizzicato jingles");
        audioDirector.musicVolume = 0.24f;
        audioDirector.musicGapRange = new Vector2(0.12f, 0.48f);
    }

    private static void ConfigureGameplayAudio(GameAudioDirector audioDirector)
    {
        if (audioDirector == null)
        {
            return;
        }

        ConfigureSharedAudio(audioDirector);
        audioDirector.missionCompleteClip = LoadAudioClip(
            "Assets/Imported/Kenney/UIAudio/Audio/switch21.ogg",
            "Assets/Imported/Kenney/UIAudio/Audio/switch11.ogg");
        audioDirector.hunterAlertClip = LoadAudioClip(
            "Assets/Imported/Kenney/UIAudio/Audio/switch8.ogg",
            "Assets/Imported/Kenney/UIAudio/Audio/switch5.ogg");
        audioDirector.lockdownClip = LoadAudioClip(
            "Assets/Imported/Kenney/UIAudio/Audio/switch30.ogg",
            "Assets/Imported/Kenney/UIAudio/Audio/switch31.ogg");
        audioDirector.gameOverClip = LoadAudioClip(
            "Assets/Imported/Kenney/UIAudio/Audio/switch35.ogg",
            "Assets/Imported/Kenney/UIAudio/Audio/switch26.ogg");
        audioDirector.disguiseClip = LoadAudioClip(
            "Assets/Imported/Kenney/UIAudio/Audio/switch18.ogg",
            "Assets/Imported/Kenney/UIAudio/Audio/switch10.ogg");
        audioDirector.footstepConcreteClip = LoadAudioClip(
            "Assets/Imported/Kenney/ImpactSounds/Audio/footstep_concrete_001.ogg",
            "Assets/Imported/Kenney/ImpactSounds/Audio/footstep_concrete_003.ogg");
        audioDirector.footstepGrassClip = LoadAudioClip(
            "Assets/Imported/Kenney/ImpactSounds/Audio/footstep_grass_001.ogg",
            "Assets/Imported/Kenney/ImpactSounds/Audio/footstep_grass_003.ogg");
        audioDirector.musicVolume = 0.18f;
        audioDirector.musicGapRange = new Vector2(0.08f, 0.22f);
    }

    private static void ConfigureGameplayVfx(PrototypeVfxDirector vfxDirector)
    {
        if (vfxDirector == null)
        {
            return;
        }

        vfxDirector.disguiseSmokeMaterial = CreateOrUpdateParticleMaterial(
            MaterialRoot + "/VFX_DisguiseSmoke.mat",
            "Assets/Imported/Kenney/ParticlePack/PNG (Transparent)/smoke_04.png",
            new Color(0.82f, 0.92f, 1f, 0.75f),
            false);
        vfxDirector.disguiseSparkMaterial = CreateOrUpdateParticleMaterial(
            MaterialRoot + "/VFX_DisguiseSpark.mat",
            "Assets/Imported/Kenney/ParticlePack/PNG (Transparent)/magic_03.png",
            new Color(0.18f, 1f, 0.72f, 1f),
            true);
        vfxDirector.missionCompleteMaterial = CreateOrUpdateParticleMaterial(
            MaterialRoot + "/VFX_MissionComplete.mat",
            "Assets/Imported/Kenney/ParticlePack/PNG (Transparent)/star_05.png",
            new Color(1f, 0.84f, 0.22f, 1f),
            true);
        vfxDirector.hunterAlertMaterial = CreateOrUpdateParticleMaterial(
            MaterialRoot + "/VFX_HunterAlert.mat",
            "Assets/Imported/Kenney/ParticlePack/PNG (Transparent)/spark_05.png",
            new Color(1f, 0.34f, 0.18f, 1f),
            true);
    }

    private static void CreateGrayboxMap(BlendInBootstrapAssets assets, Transform environmentRoot, Transform markerRoot)
    {
        var sidewalkMaterial = CreateOrUpdateMaterial(MaterialRoot + "/Sidewalk.mat", new Color(0.78f, 0.77f, 0.72f));
        var trimMaterial = CreateOrUpdateMaterial(MaterialRoot + "/BuildingTrim.mat", new Color(0.92f, 0.92f, 0.95f));
        var windowMaterial = CreateOrUpdateMaterial(MaterialRoot + "/Window.mat", new Color(0.18f, 0.31f, 0.42f));
        var accentWarmMaterial = CreateOrUpdateMaterial(MaterialRoot + "/AccentWarm.mat", new Color(0.93f, 0.56f, 0.24f));
        var accentCoolMaterial = CreateOrUpdateMaterial(MaterialRoot + "/AccentCool.mat", new Color(0.22f, 0.53f, 0.84f));
        var woodMaterial = CreateOrUpdateMaterial(MaterialRoot + "/Wood.mat", new Color(0.55f, 0.37f, 0.22f));
        var foliageMaterial = CreateOrUpdateMaterial(MaterialRoot + "/Foliage.mat", new Color(0.24f, 0.58f, 0.32f));
        var trunkMaterial = CreateOrUpdateMaterial(MaterialRoot + "/Trunk.mat", new Color(0.39f, 0.27f, 0.18f));
        var waterMaterial = CreateOrUpdateMaterial(MaterialRoot + "/Water.mat", new Color(0.22f, 0.61f, 0.78f));

        CreatePrimitiveGround(Vector3.zero, Vector3.one * 20f, new Color(0.36f, 0.56f, 0.32f), assets.GroundMaterial).transform.SetParent(environmentRoot);

        CreateRoad(environmentRoot, assets.ArtSet, "MainRoad", new Vector3(0f, 0.02f, 14f), new Vector3(18f, 0.05f, 2f), assets.RoadMaterial);
        CreateRoad(environmentRoot, assets.ArtSet, "EastRoad", new Vector3(22f, 0.02f, -10f), new Vector3(2f, 0.05f, 18f), assets.RoadMaterial);
        CreateRoad(environmentRoot, assets.ArtSet, "WestRoad", new Vector3(-22f, 0.02f, -4f), new Vector3(2f, 0.05f, 14f), assets.RoadMaterial);
        CreateSidewalk(environmentRoot, assets.ArtSet, "NorthWalk", new Vector3(0f, 0.03f, 17.2f), new Vector3(18f, 0.04f, 0.9f), sidewalkMaterial);
        CreateSidewalk(environmentRoot, assets.ArtSet, "SouthWalk", new Vector3(0f, 0.03f, 10.8f), new Vector3(18f, 0.04f, 0.9f), sidewalkMaterial);
        CreateSidewalk(environmentRoot, assets.ArtSet, "EastWalkA", new Vector3(25.2f, 0.03f, -10f), new Vector3(0.9f, 0.04f, 18f), sidewalkMaterial);
        CreateSidewalk(environmentRoot, assets.ArtSet, "EastWalkB", new Vector3(18.8f, 0.03f, -10f), new Vector3(0.9f, 0.04f, 18f), sidewalkMaterial);
        CreateSidewalk(environmentRoot, assets.ArtSet, "WestWalkA", new Vector3(-18.8f, 0.03f, -4f), new Vector3(0.9f, 0.04f, 14f), sidewalkMaterial);
        CreateSidewalk(environmentRoot, assets.ArtSet, "WestWalkB", new Vector3(-25.2f, 0.03f, -4f), new Vector3(0.9f, 0.04f, 14f), sidewalkMaterial);

        CreateBuilding(environmentRoot, assets.ArtSet, "Apartment_A", new Vector3(-60f, 6f, 56f), new Vector3(14f, 12f, 14f), assets.BuildingMaterial);
        CreateBuilding(environmentRoot, assets.ArtSet, "Apartment_B", new Vector3(-42f, 5f, 58f), new Vector3(12f, 10f, 12f), assets.BuildingMaterial);
        CreateBuilding(environmentRoot, assets.ArtSet, "Apartment_C", new Vector3(-24f, 6f, 56f), new Vector3(14f, 12f, 14f), assets.BuildingMaterial);
        CreateBuilding(environmentRoot, assets.ArtSet, "School", new Vector3(60f, 8f, 56f), new Vector3(18f, 16f, 18f), assets.BuildingMaterial);
        CreateBuilding(environmentRoot, assets.ArtSet, "Office", new Vector3(52f, 7f, 30f), new Vector3(20f, 14f, 16f), assets.BuildingMaterial);
        CreateBuilding(environmentRoot, assets.ArtSet, "Cafe", new Vector3(-56f, 4f, 16f), new Vector3(12f, 8f, 10f), assets.BuildingMaterial);
        CreateBuilding(environmentRoot, assets.ArtSet, "Convenience", new Vector3(-40f, 4f, 16f), new Vector3(10f, 8f, 10f), assets.BuildingMaterial);
        CreateBuilding(environmentRoot, assets.ArtSet, "Restaurant", new Vector3(-22f, 4f, 16f), new Vector3(14f, 8f, 12f), assets.BuildingMaterial);
        CreateBuilding(environmentRoot, assets.ArtSet, "Shop", new Vector3(-46f, 4f, 0f), new Vector3(12f, 8f, 10f), assets.BuildingMaterial);
        DecorateDistrict(environmentRoot, trimMaterial, windowMaterial, accentWarmMaterial, accentCoolMaterial, woodMaterial);

        CreatePlaza(environmentRoot, assets, sidewalkMaterial, trimMaterial, accentCoolMaterial, woodMaterial, foliageMaterial, trunkMaterial, waterMaterial);
        CreatePark(environmentRoot, assets, sidewalkMaterial, woodMaterial, foliageMaterial, trunkMaterial, waterMaterial);
        CreateStreetProps(environmentRoot, assets.ArtSet, trimMaterial, accentWarmMaterial, woodMaterial, foliageMaterial, trunkMaterial);

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
        camera.backgroundColor = new Color(0.70f, 0.84f, 0.96f);
        camera.clearFlags = CameraClearFlags.SolidColor;
        camera.fieldOfView = 50f;
        camera.nearClipPlane = 0.15f;
        camera.farClipPlane = 180f;

        var follow = cameraObject.GetComponent<CameraFollow>();
        follow.target = target;
        follow.offset = new Vector3(0f, 8.6f, -7.4f);
        follow.positionLerp = 6.5f;
        follow.rotationLerp = 7.5f;
        return camera;
    }

    private static void CreateAtmosphere()
    {
        RenderSettings.fog = true;
        RenderSettings.fogMode = FogMode.ExponentialSquared;
        RenderSettings.fogDensity = 0.0048f;
        RenderSettings.fogColor = new Color(0.80f, 0.88f, 0.94f);
        RenderSettings.ambientMode = AmbientMode.Flat;
        RenderSettings.ambientLight = new Color(0.84f, 0.88f, 0.92f);
        RenderSettings.subtractiveShadowColor = new Color(0.24f, 0.30f, 0.36f);

        var profile = CreateOrUpdateAsset<VolumeProfile>(ProjectRoot + "/Art/PrototypePostFX.asset", asset =>
        {
            asset.name = "PrototypePostFX";
        });

        var bloom = GetOrCreateVolumeComponent<Bloom>(profile);

        bloom.active = true;
        bloom.intensity.Override(0.26f);
        bloom.threshold.Override(0.88f);
        bloom.scatter.Override(0.72f);

        var colorAdjustments = GetOrCreateVolumeComponent<ColorAdjustments>(profile);

        colorAdjustments.active = true;
        colorAdjustments.postExposure.Override(0.14f);
        colorAdjustments.contrast.Override(12f);
        colorAdjustments.saturation.Override(10f);

        var vignette = GetOrCreateVolumeComponent<Vignette>(profile);

        vignette.active = true;
        vignette.intensity.Override(0.18f);
        vignette.smoothness.Override(0.78f);
        EditorUtility.SetDirty(profile);

        var volumeGo = new GameObject("Global Volume", typeof(Volume));
        var volume = volumeGo.GetComponent<Volume>();
        volume.isGlobal = true;
        volume.priority = 10f;
        volume.sharedProfile = profile;
    }

    private static void BuildGameplayUI(GameObject player, MissionManager missionManager)
    {
        var playerDisguise = player.GetComponent<PlayerDisguise>();
        var suspicion = player.GetComponent<SuspicionSystem>();
        var minimapTexture = CreateOrUpdateRenderTexture(ProjectRoot + "/Art/PrototypeMinimap.renderTexture", 768, 768);
        CreateMinimapCamera(player != null ? player.transform : null, minimapTexture);

        CreateEventSystem();
        var canvas = CreateCanvas("HUD");
        var panelColor = new Color(0.05f, 0.07f, 0.11f, 0.82f);
        var panelAltColor = new Color(0.09f, 0.13f, 0.18f, 0.92f);
        var accentColor = new Color(0.18f, 0.74f, 0.90f, 0.98f);
        var positiveColor = new Color(0.20f, 0.78f, 0.52f, 0.96f);

        var timerCard = CreateImageElement("TimerCard", canvas.transform, panelColor);
        Stretch((RectTransform)timerCard.transform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(34f, -28f), new Vector2(270f, 66f));
        var timerAccent = CreateImageElement("Accent", timerCard.transform, accentColor);
        Stretch((RectTransform)timerAccent.transform, new Vector2(0f, 1f), new Vector2(1f, 1f), Vector2.zero, new Vector2(0f, 6f));
        var timerHeader = CreateTextElement("Header", timerCard.transform, "SHIFT CLOCK", 14, TextAlignmentOptions.MidlineLeft);
        Stretch((RectTransform)timerHeader.transform, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(18f, -16f), new Vector2(-36f, 20f));
        var timerRoot = new GameObject("TimerUI", typeof(RectTransform), typeof(TimerUI));
        timerRoot.transform.SetParent(timerCard.transform, false);
        Stretch((RectTransform)timerRoot.transform, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
        var timerText = CreateTextElement("Label", timerRoot.transform, "08:00  |  03:00 left", 24, TextAlignmentOptions.BottomLeft);
        Stretch((RectTransform)timerText.transform, new Vector2(0f, 0f), new Vector2(1f, 1f), new Vector2(18f, 8f), new Vector2(-36f, -22f));
        var timerUi = timerRoot.GetComponent<TimerUI>();
        timerUi.timerLabel = timerText;

        var guideRoot = CreateImageElement("GuideCard", canvas.transform, new Color(0.05f, 0.08f, 0.11f, 0.72f));
        Stretch((RectTransform)guideRoot.transform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -28f), new Vector2(760f, 54f));
        var guideAccent = CreateImageElement("Accent", guideRoot.transform, new Color(1f, 1f, 1f, 0.06f));
        Stretch((RectTransform)guideAccent.transform, new Vector2(0f, 0f), new Vector2(0f, 1f), Vector2.zero, new Vector2(8f, 0f));
        var guideText = CreateTextElement("Guide", guideRoot.transform, "Survive until 20:00. Move like the crowd. Stop inside mission zones to score.", 18, TextAlignmentOptions.Center);
        Stretch((RectTransform)guideText.transform, new Vector2(0f, 0f), new Vector2(1f, 1f), new Vector2(22f, 0f), new Vector2(-44f, -8f));
        timerUi.guideLabel = guideText;

        var missionCard = CreateImageElement("MissionCard", canvas.transform, panelColor);
        Stretch((RectTransform)missionCard.transform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(34f, -114f), new Vector2(430f, 132f));
        var missionAccent = CreateImageElement("Accent", missionCard.transform, positiveColor);
        Stretch((RectTransform)missionAccent.transform, new Vector2(0f, 1f), new Vector2(1f, 1f), Vector2.zero, new Vector2(0f, 6f));
        var missionHeader = CreateTextElement("Header", missionCard.transform, "ACTIVE MISSION", 14, TextAlignmentOptions.MidlineLeft);
        Stretch((RectTransform)missionHeader.transform, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(18f, -16f), new Vector2(-36f, 20f));
        var missionRoot = new GameObject("MissionUI", typeof(RectTransform), typeof(MissionUI));
        missionRoot.transform.SetParent(missionCard.transform, false);
        Stretch((RectTransform)missionRoot.transform, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
        var missionText = CreateTextElement("Label", missionRoot.transform, "Mission: --", 24, TextAlignmentOptions.TopLeft);
        Stretch((RectTransform)missionText.transform, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(18f, -42f), new Vector2(-36f, 58f));
        var missionHint = CreateTextElement("Hint", missionRoot.transform, "Stay still in the target zone to complete the objective.", 16, TextAlignmentOptions.TopLeft);
        Stretch((RectTransform)missionHint.transform, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(18f, -88f), new Vector2(-36f, 34f));
        var missionProgressBg = CreateImageElement("ProgressBg", missionRoot.transform, new Color(1f, 1f, 1f, 0.10f));
        Stretch((RectTransform)missionProgressBg.transform, new Vector2(0f, 0f), new Vector2(1f, 0f), new Vector2(18f, 18f), new Vector2(-36f, 14f));
        var missionProgressFill = CreateImageElement("ProgressFill", missionProgressBg.transform, positiveColor);
        missionProgressFill.type = Image.Type.Filled;
        missionProgressFill.fillMethod = Image.FillMethod.Horizontal;
        Stretch((RectTransform)missionProgressFill.transform, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
        var missionUi = missionRoot.GetComponent<MissionUI>();
        missionUi.missionManager = missionManager;
        missionUi.missionLabel = missionText;
        missionUi.progressFill = missionProgressFill;

        var intelCard = CreateImageElement("IntelCard", canvas.transform, panelAltColor);
        Stretch((RectTransform)intelCard.transform, new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(-34f, -28f), new Vector2(376f, 300f));
        var intelAccent = CreateImageElement("Accent", intelCard.transform, new Color(0.94f, 0.35f, 0.24f, 0.96f));
        Stretch((RectTransform)intelAccent.transform, new Vector2(0f, 1f), new Vector2(1f, 1f), Vector2.zero, new Vector2(0f, 6f));
        var intelHeader = CreateTextElement("Header", intelCard.transform, "HUNTER INTEL", 14, TextAlignmentOptions.MidlineLeft);
        Stretch((RectTransform)intelHeader.transform, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(18f, -16f), new Vector2(-120f, 20f));
        var minimapToggle = CreateButtonElement("MinimapToggle", intelCard.transform, new Color(1f, 1f, 1f, 0.08f));
        Stretch((RectTransform)minimapToggle.transform, new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(-18f, -18f), new Vector2(104f, 26f));
        var minimapToggleLabel = CreateTextElement("Label", minimapToggle.transform, "EXPAND", 13, TextAlignmentOptions.Center);
        Stretch((RectTransform)minimapToggleLabel.transform, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
        var minimapFrame = CreateImageElement("MinimapFrame", intelCard.transform, new Color(0.02f, 0.03f, 0.05f, 0.92f));
        Stretch((RectTransform)minimapFrame.transform, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(18f, -44f), new Vector2(-36f, 148f));
        var minimapRawGo = new GameObject("Map", typeof(RectTransform), typeof(RawImage));
        minimapRawGo.transform.SetParent(minimapFrame.transform, false);
        var minimapRaw = minimapRawGo.GetComponent<RawImage>();
        minimapRaw.texture = minimapTexture;
        minimapRaw.color = Color.white;
        Stretch((RectTransform)minimapRaw.transform, new Vector2(0f, 0f), new Vector2(1f, 1f), new Vector2(6f, 6f), new Vector2(-12f, -12f));
        var minimapLabel = CreateTextElement("State", intelCard.transform, "MINIMAP", 14, TextAlignmentOptions.MidlineLeft);
        Stretch((RectTransform)minimapLabel.transform, new Vector2(0f, 0f), new Vector2(1f, 0f), new Vector2(18f, 100f), new Vector2(-36f, 20f));
        var scoreText = CreateTextElement("Score", intelCard.transform, "Score  0", 18, TextAlignmentOptions.TopLeft);
        Stretch((RectTransform)scoreText.transform, new Vector2(0f, 0f), new Vector2(1f, 0f), new Vector2(18f, 66f), new Vector2(-36f, 24f));
        var hunterText = CreateTextElement("Hunter", intelCard.transform, "Hunter  Patrolling", 18, TextAlignmentOptions.TopLeft);
        Stretch((RectTransform)hunterText.transform, new Vector2(0f, 0f), new Vector2(1f, 0f), new Vector2(18f, 40f), new Vector2(-36f, 24f));
        var eventText = CreateTextElement("Event", intelCard.transform, "Event  None", 17, TextAlignmentOptions.TopLeft);
        Stretch((RectTransform)eventText.transform, new Vector2(0f, 0f), new Vector2(1f, 0f), new Vector2(18f, 16f), new Vector2(-36f, 22f));
        timerUi.scoreLabel = scoreText;
        timerUi.hunterLabel = hunterText;
        timerUi.eventLabel = eventText;

        var minimapUiGo = new GameObject("MinimapUI", typeof(RectTransform), typeof(MinimapUI));
        minimapUiGo.transform.SetParent(canvas.transform, false);
        var minimapUi = minimapUiGo.GetComponent<MinimapUI>();
        minimapUi.minimapRoot = (RectTransform)intelCard.transform;
        minimapUi.mapFrame = (RectTransform)minimapFrame.transform;
        minimapUi.mapImage = minimapRaw;
        minimapUi.stateLabel = minimapLabel;
        minimapUi.toggleLabel = minimapToggleLabel;
        minimapUi.toggleButton = minimapToggle;
        minimapUi.collapsedSize = new Vector2(376f, 300f);
        minimapUi.expandedSize = new Vector2(500f, 392f);
        minimapUi.collapsedMapSize = new Vector2(340f, 148f);
        minimapUi.expandedMapSize = new Vector2(464f, 220f);

        var suspicionCard = CreateImageElement("SuspicionCard", canvas.transform, panelAltColor);
        Stretch((RectTransform)suspicionCard.transform, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 34f), new Vector2(390f, 72f));
        var suspicionAccent = CreateImageElement("Accent", suspicionCard.transform, new Color(0.96f, 0.76f, 0.16f, 0.98f));
        Stretch((RectTransform)suspicionAccent.transform, new Vector2(0f, 1f), new Vector2(1f, 1f), Vector2.zero, new Vector2(0f, 6f));
        var suspicionHeader = CreateTextElement("Header", suspicionCard.transform, "SUSPICION", 14, TextAlignmentOptions.MidlineLeft);
        Stretch((RectTransform)suspicionHeader.transform, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(18f, -16f), new Vector2(-36f, 20f));
        var suspicionRoot = new GameObject("SuspicionUI", typeof(RectTransform), typeof(SuspicionMeterUI));
        suspicionRoot.transform.SetParent(suspicionCard.transform, false);
        Stretch((RectTransform)suspicionRoot.transform, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
        var suspicionBarBg = CreateImageElement("BarBg", suspicionRoot.transform, new Color(1f, 1f, 1f, 0.10f));
        Stretch((RectTransform)suspicionBarBg.transform, new Vector2(0f, 0f), new Vector2(1f, 0f), new Vector2(18f, 16f), new Vector2(-36f, 16f));
        var suspicionFill = CreateImageElement("Fill", suspicionBarBg.transform, new Color(0.90f, 0.80f, 0.10f, 0.95f));
        suspicionFill.type = Image.Type.Filled;
        suspicionFill.fillMethod = Image.FillMethod.Horizontal;
        suspicionFill.fillAmount = 0f;
        Stretch((RectTransform)suspicionFill.transform, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
        var suspicionLabel = CreateTextElement("Value", suspicionRoot.transform, "CALM 0", 20, TextAlignmentOptions.TopLeft);
        Stretch((RectTransform)suspicionLabel.transform, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(18f, -40f), new Vector2(-36f, 24f));
        var suspicionUi = suspicionRoot.GetComponent<SuspicionMeterUI>();
        suspicionUi.suspicionSystem = suspicion;
        suspicionUi.fillImage = suspicionFill;
        suspicionUi.valueLabel = suspicionLabel;
        suspicionUi.fillGradient = CreateSuspicionGradient();

        var disguiseCard = CreateButtonElement("DisguiseButton", canvas.transform, panelAltColor);
        Stretch((RectTransform)disguiseCard.transform, new Vector2(1f, 0f), new Vector2(1f, 0f), new Vector2(-34f, 34f), new Vector2(210f, 94f));
        var disguiseAccent = CreateImageElement("Accent", disguiseCard.transform, positiveColor);
        Stretch((RectTransform)disguiseAccent.transform, new Vector2(0f, 1f), new Vector2(1f, 1f), Vector2.zero, new Vector2(0f, 6f));
        var disguiseTitle = CreateTextElement("Label", disguiseCard.transform, "DISGUISE", 24, TextAlignmentOptions.TopLeft);
        Stretch((RectTransform)disguiseTitle.transform, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(18f, -18f), new Vector2(-36f, 24f));
        var disguiseProgressBg = CreateImageElement("ProgressBg", disguiseCard.transform, new Color(1f, 1f, 1f, 0.10f));
        Stretch((RectTransform)disguiseProgressBg.transform, new Vector2(0f, 0f), new Vector2(1f, 0f), new Vector2(18f, 18f), new Vector2(-36f, 14f));
        var disguiseProgress = CreateImageElement("Progress", disguiseProgressBg.transform, positiveColor);
        disguiseProgress.type = Image.Type.Filled;
        disguiseProgress.fillMethod = Image.FillMethod.Horizontal;
        Stretch((RectTransform)disguiseProgress.transform, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
        var disguiseCharges = CreateTextElement("Charges", disguiseCard.transform, "Disguise x3", 18, TextAlignmentOptions.BottomLeft);
        Stretch((RectTransform)disguiseCharges.transform, new Vector2(0f, 0f), new Vector2(1f, 1f), new Vector2(18f, 0f), new Vector2(-36f, -30f));
        var disguiseUiGo = new GameObject("DisguiseUI", typeof(RectTransform), typeof(DisguiseUI));
        disguiseUiGo.transform.SetParent(canvas.transform, false);
        var disguiseUi = disguiseUiGo.GetComponent<DisguiseUI>();
        disguiseUi.playerDisguise = playerDisguise;
        disguiseUi.disguiseButton = disguiseCard;
        disguiseUi.progressFill = disguiseProgress;
        disguiseUi.chargesLabel = disguiseCharges;

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

    private static Camera CreateMinimapCamera(Transform target, RenderTexture renderTexture)
    {
        var cameraObject = new GameObject("MinimapCamera", typeof(Camera), typeof(MinimapCameraFollow));
        cameraObject.SetActive(true);
        var camera = cameraObject.GetComponent<Camera>();
        camera.orthographic = true;
        camera.orthographicSize = 34f;
        camera.clearFlags = CameraClearFlags.SolidColor;
        camera.backgroundColor = new Color(0.23f, 0.28f, 0.31f);
        camera.nearClipPlane = 0.3f;
        camera.farClipPlane = 220f;
        camera.allowHDR = false;
        camera.enabled = true;
        camera.targetTexture = renderTexture;
        camera.depth = -5f;

        var follow = cameraObject.GetComponent<MinimapCameraFollow>();
        follow.target = target;
        follow.height = 58f;
        follow.positionLerp = 12f;

        if (target != null)
        {
            cameraObject.transform.position = target.position + Vector3.up * follow.height;
        }

        cameraObject.transform.rotation = Quaternion.Euler(90f, 0f, 0f);
        return camera;
    }

    private static RenderTexture CreateOrUpdateRenderTexture(string path, int width, int height)
    {
        var renderTexture = AssetDatabase.LoadAssetAtPath<RenderTexture>(path);
        if (renderTexture == null)
        {
            renderTexture = new RenderTexture(width, height, 16)
            {
                name = "PrototypeMinimap"
            };
            AssetDatabase.CreateAsset(renderTexture, path);
        }

        renderTexture.width = width;
        renderTexture.height = height;
        renderTexture.depth = 16;
        renderTexture.wrapMode = TextureWrapMode.Clamp;
        renderTexture.filterMode = FilterMode.Bilinear;
        renderTexture.antiAliasing = 1;
        EditorUtility.SetDirty(renderTexture);
        return renderTexture;
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
        var background = CreateImageElement("Background", joystickRoot.transform, new Color(0.05f, 0.08f, 0.11f, 0.58f));
        background.raycastTarget = true;
        Stretch((RectTransform)background.transform, new Vector2(0f, 0f), new Vector2(0f, 0f), new Vector2(42f, 42f), new Vector2(170f, 170f));
        var ring = CreateImageElement("Ring", background.transform, new Color(1f, 1f, 1f, 0.06f));
        ring.raycastTarget = false;
        Stretch((RectTransform)ring.transform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(126f, 126f));
        var handle = CreateImageElement("Handle", background.transform, new Color(0.80f, 0.96f, 1f, 0.92f));
        handle.raycastTarget = false;
        Stretch((RectTransform)handle.transform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(66f, 66f));

        var joystick = joystickRoot.GetComponent<JoystickUI>();
        joystick.background = (RectTransform)background.transform;
        joystick.handle = (RectTransform)handle.transform;
        joystick.maxRadius = 58f;
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
        light.intensity = 1.28f;
        light.shadows = LightShadows.Soft;
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
        var roadVisual = PickRoadPrefabForName(name, artSet);
        if (renderer != null && roadVisual == null)
        {
            renderer.sharedMaterial = material != null
                ? material
                : CreateOrUpdateMaterial(MaterialRoot + "/Road.mat", new Color(0.22f, 0.23f, 0.25f));
        }

        if (roadVisual != null)
        {
            renderer.enabled = false;
            var rotation = scale.x >= scale.z ? Quaternion.identity : Quaternion.Euler(0f, 90f, 0f);
            AttachFittedVisual(road.transform, roadVisual, new Vector3(scale.x, 0.8f, scale.z), 0.96f, rotation);
            return;
        }

        CreateRoadLines(road.transform, scale);
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

            AttachFittedVisual(
                building.transform,
                visualPrefab,
                GetBuildingVisualTargetSize(name, scale),
                GetBuildingVisualFill(name),
                Quaternion.identity);
        }
    }

    private static void CreatePlaza(
        Transform parent,
        BlendInBootstrapAssets assets,
        Material sidewalkMaterial,
        Material trimMaterial,
        Material accentMaterial,
        Material woodMaterial,
        Material foliageMaterial,
        Material trunkMaterial,
        Material waterMaterial)
    {
        var plaza = GameObject.CreatePrimitive(PrimitiveType.Cube);
        plaza.name = "Plaza";
        plaza.transform.SetParent(parent);
        plaza.transform.position = new Vector3(0f, 0.1f, 0f);
        plaza.transform.localScale = new Vector3(28f, 0.2f, 28f);
        var renderer = plaza.GetComponent<Renderer>();
        if (renderer != null)
        {
            renderer.sharedMaterial = sidewalkMaterial != null ? sidewalkMaterial : assets.BuildingMaterial;
        }

        CreateDecorCube("PlazaInlay", plaza.transform, new Vector3(0f, 0.12f, 0f), new Vector3(18f, 0.02f, 18f), trimMaterial);
        CreateFountain(plaza.transform, new Vector3(0f, 0.15f, 0f), trimMaterial, waterMaterial, assets.ArtSet);
        CreateBench(plaza.transform, new Vector3(-8f, 0.15f, -5f), woodMaterial, trimMaterial, 90f, assets.ArtSet);
        CreateBench(plaza.transform, new Vector3(8f, 0.15f, -5f), woodMaterial, trimMaterial, -90f, assets.ArtSet);
        CreateBench(plaza.transform, new Vector3(-8f, 0.15f, 5f), woodMaterial, trimMaterial, 90f, assets.ArtSet);
        CreateBench(plaza.transform, new Vector3(8f, 0.15f, 5f), woodMaterial, trimMaterial, -90f, assets.ArtSet);
        CreateTree(plaza.transform, new Vector3(-11f, 0.15f, -11f), trunkMaterial, foliageMaterial, 1.25f, assets.ArtSet);
        CreateTree(plaza.transform, new Vector3(11f, 0.15f, -11f), trunkMaterial, foliageMaterial, 1.25f, assets.ArtSet);
        CreateTree(plaza.transform, new Vector3(-11f, 0.15f, 11f), trunkMaterial, foliageMaterial, 1.25f, assets.ArtSet);
        CreateTree(plaza.transform, new Vector3(11f, 0.15f, 11f), trunkMaterial, foliageMaterial, 1.25f, assets.ArtSet);
        CreateLamp(plaza.transform, new Vector3(-13f, 0.15f, 0f), accentMaterial, trimMaterial, assets.ArtSet);
        CreateLamp(plaza.transform, new Vector3(13f, 0.15f, 0f), accentMaterial, trimMaterial, assets.ArtSet);
    }

    private static void CreatePark(
        Transform parent,
        BlendInBootstrapAssets assets,
        Material sidewalkMaterial,
        Material woodMaterial,
        Material foliageMaterial,
        Material trunkMaterial,
        Material waterMaterial)
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

        CreateDecorCube("ParkPath_A", park.transform, new Vector3(0f, 0.08f, 0f), new Vector3(22f, 0.04f, 2.4f), sidewalkMaterial);
        CreateDecorCube("ParkPath_B", park.transform, new Vector3(6f, 0.08f, 0f), new Vector3(2.4f, 0.04f, 18f), sidewalkMaterial);
        CreateDecorCube("Pond", park.transform, new Vector3(10f, 0.1f, -6f), new Vector3(8f, 0.03f, 6f), waterMaterial);
        CreateBench(park.transform, new Vector3(-8f, 0.12f, 0f), woodMaterial, sidewalkMaterial, 180f, assets.ArtSet);
        CreateBench(park.transform, new Vector3(4f, 0.12f, 6f), woodMaterial, sidewalkMaterial, 90f, assets.ArtSet);
        CreateBench(park.transform, new Vector3(4f, 0.12f, -8f), woodMaterial, sidewalkMaterial, -90f, assets.ArtSet);
        CreateTree(park.transform, new Vector3(-12f, 0.12f, -8f), trunkMaterial, foliageMaterial, 1.35f, assets.ArtSet);
        CreateTree(park.transform, new Vector3(-4f, 0.12f, 8f), trunkMaterial, foliageMaterial, 1.1f, assets.ArtSet);
        CreateTree(park.transform, new Vector3(8f, 0.12f, -10f), trunkMaterial, foliageMaterial, 1.45f, assets.ArtSet);
        CreateTree(park.transform, new Vector3(12f, 0.12f, 8f), trunkMaterial, foliageMaterial, 1.2f, assets.ArtSet);
        CreateParkAccent(park.transform, new Vector3(-10f, 0.12f, 8f), new Vector3(2.6f, 1.8f, 2.6f), assets.ArtSet, "plant_bush", "tree-shrub");
        CreateParkAccent(park.transform, new Vector3(-1f, 0.12f, -9f), new Vector3(1.6f, 1.1f, 1.6f), assets.ArtSet, "rock_small", "rock");
        CreateParkAccent(park.transform, new Vector3(10f, 0.12f, 2f), new Vector3(1.4f, 1f, 1.4f), assets.ArtSet, "flower", "grass");
        CreateParkAccent(park.transform, new Vector3(14f, 0.12f, -4f), new Vector3(2f, 1.1f, 2f), assets.ArtSet, "grass_large", "grass");
    }

    private static void CreateSidewalk(Transform parent, BlendInArtSet artSet, string name, Vector3 position, Vector3 scale, Material material)
    {
        var sidewalk = GameObject.CreatePrimitive(PrimitiveType.Cube);
        sidewalk.name = name;
        sidewalk.transform.SetParent(parent);
        sidewalk.transform.position = position;
        sidewalk.transform.localScale = scale;
        var renderer = sidewalk.GetComponent<Renderer>();
        if (renderer != null)
        {
            renderer.sharedMaterial = material;
        }

        var visualPrefab = PickPrefabForName(name, artSet != null ? artSet.sidewalkVisualPrefabs : null);
        if (visualPrefab != null)
        {
            renderer.enabled = false;
            var rotation = scale.x >= scale.z ? Quaternion.identity : Quaternion.Euler(0f, 90f, 0f);
            AttachFittedVisual(sidewalk.transform, visualPrefab, new Vector3(scale.x, 0.45f, scale.z), 0.96f, rotation);
        }
    }

    private static void CreateRoadLines(Transform road, Vector3 roadScale)
    {
        var lineMaterial = CreateOrUpdateMaterial(MaterialRoot + "/RoadLine.mat", new Color(0.96f, 0.94f, 0.82f));
        var isHorizontal = roadScale.x > roadScale.z;
        var segmentCount = isHorizontal ? Mathf.Max(4, Mathf.RoundToInt(roadScale.x * 0.35f)) : Mathf.Max(4, Mathf.RoundToInt(roadScale.z * 0.35f));
        for (var i = 0; i < segmentCount; i++)
        {
            var t = segmentCount == 1 ? 0.5f : i / (float)(segmentCount - 1);
            var position = isHorizontal
                ? new Vector3(Mathf.Lerp(-roadScale.x * 0.42f, roadScale.x * 0.42f, t), roadScale.y * 0.5f + 0.01f, 0f)
                : new Vector3(0f, roadScale.y * 0.5f + 0.01f, Mathf.Lerp(-roadScale.z * 0.42f, roadScale.z * 0.42f, t));
            var scale = isHorizontal
                ? new Vector3(1.4f, 0.02f, 0.12f)
                : new Vector3(0.12f, 0.02f, 1.4f);
            CreateDecorCube("Lane_" + i, road, position, scale, lineMaterial);
        }
    }

    private static void DecorateDistrict(Transform parent, Material trimMaterial, Material windowMaterial, Material accentWarmMaterial, Material accentCoolMaterial, Material woodMaterial)
    {
        for (var i = 0; i < parent.childCount; i++)
        {
            var child = parent.GetChild(i);
            if (!child.name.StartsWith("Apartment")
                && !child.name.StartsWith("School")
                && !child.name.StartsWith("Office")
                && !child.name.StartsWith("Cafe")
                && !child.name.StartsWith("Convenience")
                && !child.name.StartsWith("Restaurant")
                && !child.name.StartsWith("Shop"))
            {
                continue;
            }

            var rootRenderer = child.GetComponent<Renderer>();
            if (rootRenderer != null && !rootRenderer.enabled)
            {
                continue;
            }

            DecorateBuildingFacade(child, trimMaterial, windowMaterial, child.name.Contains("Cafe") || child.name.Contains("Restaurant") ? accentWarmMaterial : accentCoolMaterial, woodMaterial);
        }
    }

    private static void DecorateBuildingFacade(Transform building, Material trimMaterial, Material windowMaterial, Material accentMaterial, Material woodMaterial)
    {
        var scale = building.localScale;
        CreateDecorCube("RoofCap", building, new Vector3(0f, scale.y * 0.5f + 0.18f, 0f), new Vector3(scale.x * 1.04f, 0.28f, scale.z * 1.04f), trimMaterial);
        CreateDecorCube("Entrance", building, new Vector3(0f, -scale.y * 0.34f, scale.z * 0.5f + 0.08f), new Vector3(scale.x * 0.22f, scale.y * 0.28f, 0.18f), woodMaterial);
        CreateDecorCube("Sign", building, new Vector3(0f, -scale.y * 0.08f, scale.z * 0.5f + 0.12f), new Vector3(scale.x * 0.48f, 0.28f, 0.14f), accentMaterial);

        var floorCount = Mathf.Clamp(Mathf.RoundToInt(scale.y / 3f), 2, 5);
        for (var floor = 0; floor < floorCount; floor++)
        {
            var y = -scale.y * 0.32f + 1.4f + floor * (scale.y * 0.18f);
            CreateDecorCube("WindowFront_" + floor, building, new Vector3(0f, y, scale.z * 0.5f + 0.05f), new Vector3(scale.x * 0.72f, 0.55f, 0.08f), windowMaterial);
            CreateDecorCube("WindowBack_" + floor, building, new Vector3(0f, y, -scale.z * 0.5f - 0.05f), new Vector3(scale.x * 0.72f, 0.55f, 0.08f), windowMaterial);
            CreateDecorCube("WindowLeft_" + floor, building, new Vector3(-scale.x * 0.5f - 0.05f, y, 0f), new Vector3(0.08f, 0.55f, scale.z * 0.62f), windowMaterial);
            CreateDecorCube("WindowRight_" + floor, building, new Vector3(scale.x * 0.5f + 0.05f, y, 0f), new Vector3(0.08f, 0.55f, scale.z * 0.62f), windowMaterial);
        }
    }

    private static GameObject CreateDecorCube(string name, Transform parent, Vector3 localPosition, Vector3 localScale, Material material)
    {
        var cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
        cube.name = name;
        cube.transform.SetParent(parent, false);
        cube.transform.localPosition = localPosition;
        cube.transform.localRotation = Quaternion.identity;
        cube.transform.localScale = localScale;

        var collider = cube.GetComponent<Collider>();
        if (collider != null)
        {
            Object.DestroyImmediate(collider);
        }

        var renderer = cube.GetComponent<Renderer>();
        if (renderer != null && material != null)
        {
            renderer.sharedMaterial = material;
        }

        return cube;
    }

    private static void CreateBench(Transform parent, Vector3 localPosition, Material woodMaterial, Material frameMaterial, float yRotation, BlendInArtSet artSet = null)
    {
        var benchPrefab = FindPrefabByTokens(artSet != null ? artSet.streetPropPrefabs : null, "bench");
        if (TryCreatePropVisual("Bench", parent, benchPrefab, localPosition, Quaternion.Euler(0f, yRotation, 0f), new Vector3(1.9f, 1.3f, 0.9f), 0.96f))
        {
            return;
        }

        var bench = new GameObject("Bench");
        bench.transform.SetParent(parent, false);
        bench.transform.localPosition = localPosition;
        bench.transform.localRotation = Quaternion.Euler(0f, yRotation, 0f);

        CreateDecorCube("Seat", bench.transform, new Vector3(0f, 0.32f, 0f), new Vector3(1.6f, 0.12f, 0.44f), woodMaterial);
        CreateDecorCube("Back", bench.transform, new Vector3(0f, 0.62f, -0.16f), new Vector3(1.6f, 0.52f, 0.12f), woodMaterial);
        CreateDecorCube("Leg_A", bench.transform, new Vector3(-0.62f, 0.16f, 0f), new Vector3(0.12f, 0.32f, 0.12f), frameMaterial);
        CreateDecorCube("Leg_B", bench.transform, new Vector3(0.62f, 0.16f, 0f), new Vector3(0.12f, 0.32f, 0.12f), frameMaterial);
    }

    private static void CreateTree(Transform parent, Vector3 localPosition, Material trunkMaterial, Material foliageMaterial, float scale, BlendInArtSet artSet = null)
    {
        var treePrefab = scale >= 1.2f
            ? FindPrefabByTokens(artSet != null ? artSet.parkVisualPrefabs : null, "tree-large", "tree-park-large")
            : FindPrefabByTokens(artSet != null ? artSet.parkVisualPrefabs : null, "tree-small", "tree-shrub");
        if (TryCreatePropVisual(
                "Tree",
                parent,
                treePrefab,
                localPosition,
                Quaternion.identity,
                new Vector3(2.8f * scale, 4.6f * scale, 2.8f * scale),
                0.96f))
        {
            return;
        }

        var tree = new GameObject("Tree");
        tree.transform.SetParent(parent, false);
        tree.transform.localPosition = localPosition;

        CreateDecorCube("Trunk", tree.transform, new Vector3(0f, 1.1f * scale, 0f), new Vector3(0.28f * scale, 2.2f * scale, 0.28f * scale), trunkMaterial);
        CreateDecorCube("Canopy", tree.transform, new Vector3(0f, 2.8f * scale, 0f), new Vector3(1.8f * scale, 1.6f * scale, 1.8f * scale), foliageMaterial);
    }

    private static void CreateLamp(Transform parent, Vector3 localPosition, Material poleMaterial, Material lightMaterial, BlendInArtSet artSet = null)
    {
        var lampPrefab = FindPrefabByTokens(artSet != null ? artSet.streetPropPrefabs : null, "light-double", "light-single", "light");
        if (TryCreatePropVisual("Lamp", parent, lampPrefab, localPosition, Quaternion.identity, new Vector3(1.4f, 4.2f, 1.4f), 0.98f))
        {
            return;
        }

        var lamp = new GameObject("Lamp");
        lamp.transform.SetParent(parent, false);
        lamp.transform.localPosition = localPosition;

        CreateDecorCube("Pole", lamp.transform, new Vector3(0f, 1.7f, 0f), new Vector3(0.14f, 3.4f, 0.14f), poleMaterial);
        CreateDecorCube("Head", lamp.transform, new Vector3(0f, 3.45f, 0f), new Vector3(0.48f, 0.22f, 0.48f), lightMaterial);
    }

    private static void CreateFountain(Transform parent, Vector3 localPosition, Material stoneMaterial, Material waterMaterial, BlendInArtSet artSet = null)
    {
        if (TryCreatePropVisual("Fountain", parent, artSet != null ? artSet.plazaVisualPrefab : null, localPosition, Quaternion.identity, new Vector3(6.2f, 3.4f, 6.2f), 0.94f))
        {
            return;
        }

        var fountain = new GameObject("Fountain");
        fountain.transform.SetParent(parent, false);
        fountain.transform.localPosition = localPosition;

        CreateDecorCube("Base", fountain.transform, new Vector3(0f, 0.16f, 0f), new Vector3(5.8f, 0.32f, 5.8f), stoneMaterial);
        CreateDecorCube("Water", fountain.transform, new Vector3(0f, 0.26f, 0f), new Vector3(4.8f, 0.08f, 4.8f), waterMaterial);
        CreateDecorCube("Column", fountain.transform, new Vector3(0f, 0.82f, 0f), new Vector3(0.6f, 1.2f, 0.6f), stoneMaterial);
        CreateDecorCube("Top", fountain.transform, new Vector3(0f, 1.62f, 0f), new Vector3(1.6f, 0.18f, 1.6f), stoneMaterial);
    }

    private static void CreateBusStop(Transform parent, Vector3 localPosition, Material frameMaterial, Material accentMaterial, Material benchMaterial, BlendInArtSet artSet = null)
    {
        if (TryCreatePropVisual("BusStopVisual", parent, artSet != null ? artSet.busStopVisualPrefab : null, localPosition, Quaternion.identity, new Vector3(4.6f, 3.2f, 2.8f), 0.96f))
        {
            return;
        }

        var busStop = new GameObject("BusStopVisual");
        busStop.transform.SetParent(parent, false);
        busStop.transform.localPosition = localPosition;

        CreateDecorCube("Roof", busStop.transform, new Vector3(0f, 2.2f, 0f), new Vector3(3.4f, 0.14f, 1.6f), accentMaterial);
        CreateDecorCube("Post_A", busStop.transform, new Vector3(-1.4f, 1f, -0.6f), new Vector3(0.12f, 2f, 0.12f), frameMaterial);
        CreateDecorCube("Post_B", busStop.transform, new Vector3(1.4f, 1f, -0.6f), new Vector3(0.12f, 2f, 0.12f), frameMaterial);
        CreateDecorCube("Post_C", busStop.transform, new Vector3(-1.4f, 1f, 0.6f), new Vector3(0.12f, 2f, 0.12f), frameMaterial);
        CreateDecorCube("Post_D", busStop.transform, new Vector3(1.4f, 1f, 0.6f), new Vector3(0.12f, 2f, 0.12f), frameMaterial);
        CreateBench(busStop.transform, new Vector3(0f, 0f, 0.12f), benchMaterial, frameMaterial, 180f);
    }

    private static void CreateStreetDetail(Transform parent, GameObject prefab, Vector3 localPosition, Vector3 targetSize, float fill, float yRotation = 0f)
    {
        TryCreatePropVisual("StreetDetail", parent, prefab, localPosition, Quaternion.Euler(0f, yRotation, 0f), targetSize, fill);
    }

    private static bool TryCreatePropVisual(
        string name,
        Transform parent,
        GameObject prefab,
        Vector3 localPosition,
        Quaternion localRotation,
        Vector3 targetSize,
        float fill)
    {
        if (parent == null || prefab == null)
        {
            return false;
        }

        var anchor = new GameObject(name);
        anchor.transform.SetParent(parent, false);
        anchor.transform.localPosition = localPosition;
        anchor.transform.localRotation = localRotation;
        AttachFittedVisual(anchor.transform, prefab, targetSize, fill, Quaternion.identity);
        return true;
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

    private static void CreateStreetProps(
        Transform parent,
        BlendInArtSet artSet,
        Material trimMaterial,
        Material accentMaterial,
        Material woodMaterial,
        Material foliageMaterial,
        Material trunkMaterial)
    {
        CreateBusStop(parent, new Vector3(72f, 0.02f, 14f), trimMaterial, accentMaterial, woodMaterial, artSet);
        CreateLamp(parent, new Vector3(-36f, 0.02f, 8f), accentMaterial, trimMaterial, artSet);
        CreateLamp(parent, new Vector3(-18f, 0.02f, 8f), accentMaterial, trimMaterial, artSet);
        CreateLamp(parent, new Vector3(10f, 0.02f, 20f), accentMaterial, trimMaterial, artSet);
        CreateLamp(parent, new Vector3(10f, 0.02f, -2f), accentMaterial, trimMaterial, artSet);
        CreateTree(parent, new Vector3(-8f, 0.02f, 8f), trunkMaterial, foliageMaterial, 0.95f, artSet);
        CreateTree(parent, new Vector3(26f, 0.02f, 14f), trunkMaterial, foliageMaterial, 0.95f, artSet);
        CreateStreetDetail(parent, FindPrefabByTokens(artSet != null ? artSet.streetPropPrefabs : null, "dumpster"), new Vector3(-28f, 0.02f, 11f), new Vector3(1.8f, 1.8f, 1.8f), 0.95f);
        CreateStreetDetail(parent, FindPrefabByTokens(artSet != null ? artSet.streetPropPrefabs : null, "truck"), new Vector3(42f, 0.02f, 14f), new Vector3(5.5f, 3.4f, 2.6f), 0.92f);
    }

    private static void CreateMenuBackdrop(BlendInBootstrapAssets assets)
    {
        var root = new GameObject("MenuBackdrop").transform;
        var sidewalkMaterial = CreateOrUpdateMaterial(MaterialRoot + "/MenuSidewalk.mat", new Color(0.76f, 0.77f, 0.73f));
        var trimMaterial = CreateOrUpdateMaterial(MaterialRoot + "/MenuTrim.mat", new Color(0.94f, 0.95f, 0.96f));
        var woodMaterial = CreateOrUpdateMaterial(MaterialRoot + "/MenuWood.mat", new Color(0.58f, 0.39f, 0.24f));
        var foliageMaterial = CreateOrUpdateMaterial(MaterialRoot + "/MenuFoliage.mat", new Color(0.24f, 0.56f, 0.33f));
        var trunkMaterial = CreateOrUpdateMaterial(MaterialRoot + "/MenuTrunk.mat", new Color(0.40f, 0.28f, 0.18f));
        var waterMaterial = CreateOrUpdateMaterial(MaterialRoot + "/MenuWater.mat", new Color(0.22f, 0.61f, 0.78f));

        CreatePrimitiveGround(new Vector3(0f, 0f, 0f), Vector3.one * 6f, new Color(0.36f, 0.56f, 0.32f), assets.GroundMaterial).transform.SetParent(root);
        CreateRoad(root, assets.ArtSet, "MenuRoad", new Vector3(12f, 0.02f, 6f), new Vector3(18f, 0.05f, 2f), assets.RoadMaterial);
        CreateSidewalk(root, assets.ArtSet, "MenuWalkA", new Vector3(12f, 0.03f, 9.1f), new Vector3(18f, 0.04f, 0.9f), sidewalkMaterial);
        CreateSidewalk(root, assets.ArtSet, "MenuWalkB", new Vector3(12f, 0.03f, 2.9f), new Vector3(18f, 0.04f, 0.9f), sidewalkMaterial);
        CreateBuilding(root, assets.ArtSet, "MenuOffice", new Vector3(18f, 6f, 28f), new Vector3(16f, 12f, 14f), assets.BuildingMaterial);
        CreateBuilding(root, assets.ArtSet, "MenuCafe", new Vector3(-2f, 4f, 24f), new Vector3(12f, 8f, 10f), assets.BuildingMaterial);
        CreateBuilding(root, assets.ArtSet, "MenuApartments", new Vector3(-18f, 6f, 30f), new Vector3(16f, 12f, 14f), assets.BuildingMaterial);
        CreateFountain(root, new Vector3(6f, 0.06f, 18f), trimMaterial, waterMaterial, assets.ArtSet);
        CreateBench(root, new Vector3(2f, 0.04f, 14f), woodMaterial, trimMaterial, 40f, assets.ArtSet);
        CreateBench(root, new Vector3(11f, 0.04f, 13f), woodMaterial, trimMaterial, -30f, assets.ArtSet);
        CreateTree(root, new Vector3(-6f, 0.04f, 13f), trunkMaterial, foliageMaterial, 1.2f, assets.ArtSet);
        CreateTree(root, new Vector3(18f, 0.04f, 14f), trunkMaterial, foliageMaterial, 1.1f, assets.ArtSet);
        CreateLamp(root, new Vector3(0f, 0.04f, 8f), trimMaterial, waterMaterial, assets.ArtSet);
        CreateLamp(root, new Vector3(18f, 0.04f, 8f), trimMaterial, waterMaterial, assets.ArtSet);
        CreateShowcaseCharacter("ShowcaseCitizen", assets.CitizenPrefab, new Vector3(6f, 0.05f, 16f), 160f);
        CreateShowcaseCharacter("ShowcaseHunter", assets.HunterPrefab, new Vector3(14f, 0.05f, 15.5f), -130f);
        CreateShowcaseCharacter("ShowcasePlayer", assets.PlayerPrefab, new Vector3(9f, 0.05f, 12f), -20f);
    }

    private static void CreateShowcaseCharacter(string name, GameObject prefab, Vector3 position, float yRotation)
    {
        if (prefab == null)
        {
            return;
        }

        var instance = PrefabUtility.InstantiatePrefab(prefab) as GameObject;
        if (instance == null)
        {
            instance = Object.Instantiate(prefab);
        }

        instance.name = name;
        instance.transform.position = position;
        instance.transform.rotation = Quaternion.Euler(0f, yRotation, 0f);

        var behaviours = instance.GetComponentsInChildren<Behaviour>(true);
        for (var i = 0; i < behaviours.Length; i++)
        {
            if (behaviours[i] == null || behaviours[i] is Animator)
            {
                continue;
            }

            behaviours[i].enabled = false;
        }

        var colliders = instance.GetComponentsInChildren<Collider>(true);
        for (var i = 0; i < colliders.Length; i++)
        {
            if (colliders[i] != null)
            {
                colliders[i].enabled = false;
            }
        }
    }

    private static void CreateParkAccent(Transform parent, Vector3 localPosition, Vector3 targetSize, BlendInArtSet artSet, params string[] tokens)
    {
        var prefab = FindPrefabByTokens(artSet != null ? artSet.parkVisualPrefabs : null, tokens);
        if (prefab == null)
        {
            return;
        }

        TryCreatePropVisual("ParkAccent", parent, prefab, localPosition, Quaternion.identity, targetSize, 0.92f);
    }

    private static Button CreateMenuButton(Transform parent, string name, string label, Color color)
    {
        var button = CreateButtonElement(name, parent, color);
        var labelText = CreateTextElement("Label", button.transform, label, 22, TextAlignmentOptions.Center);
        Stretch((RectTransform)labelText.transform, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
        return button;
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
            filtered = FilterPrefabsByAssetPath(prefabs, "building-type-a", "building-type-b", "building-type-c", "building-type-d", "building-type-e", "building-type-f");
        }
        else if (ContainsAny(loweredSeed, "apartment", "home"))
        {
            filtered = FilterPrefabsByAssetPath(prefabs, "building-type-g", "building-type-h", "building-type-i", "building-type-j", "building-type-k", "building-type-l", "building-type-m");
        }
        else if (ContainsAny(loweredSeed, "office", "school"))
        {
            filtered = FilterPrefabsByAssetPath(prefabs, "building-type-n", "building-type-o", "building-type-p", "building-type-q", "building-type-r", "building-type-s", "building-type-t", "building-type-u");
        }

        if (filtered.Length == 0)
        {
            filtered = prefabs;
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
            filtered = FilterPrefabsByAssetPath(prefabs, "road-asphalt-center", "road-asphalt-straight");
        }
        else if (ContainsAny(loweredSeed, "east", "west"))
        {
            filtered = FilterPrefabsByAssetPath(prefabs, "road-asphalt-straight", "road-asphalt-side");
        }
        else
        {
            filtered = FilterPrefabsByAssetPath(prefabs, "road-asphalt");
        }

        if (filtered.Length == 0)
        {
            filtered = prefabs;
        }

        if (filtered.Length == 0)
        {
            filtered = prefabs;
        }

        var index = Mathf.Abs(seed.GetHashCode()) % filtered.Length;
        return filtered[index];
    }

    private static GameObject FindPrefabByTokens(GameObject[] prefabs, params string[] tokens)
    {
        if (prefabs == null || prefabs.Length == 0)
        {
            return null;
        }

        var filtered = FilterPrefabsByAssetPath(prefabs, tokens);
        return filtered.Length > 0 ? filtered[0] : prefabs[0];
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

    private static void AttachFittedVisual(Transform parent, GameObject prefab, Vector3 targetSize, float fill, Quaternion localRotation)
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
        instance.transform.localRotation = localRotation;
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

    private static AudioClip LoadAudioClip(params string[] assetPaths)
    {
        for (var i = 0; i < assetPaths.Length; i++)
        {
            var clip = AssetDatabase.LoadAssetAtPath<AudioClip>(assetPaths[i]);
            if (clip != null)
            {
                return clip;
            }
        }

        return null;
    }

    private static AudioClip[] LoadAudioClipsFromFolder(params string[] folderPaths)
    {
        return folderPaths
            .Where(path => AssetDatabase.IsValidFolder(path))
            .SelectMany(path => AssetDatabase.FindAssets("t:AudioClip", new[] { path })
                .Select(AssetDatabase.GUIDToAssetPath)
                .OrderBy(assetPath => assetPath))
            .Select(AssetDatabase.LoadAssetAtPath<AudioClip>)
            .Where(clip => clip != null)
            .Distinct()
            .ToArray();
    }

    private static Material CreateOrUpdateParticleMaterial(string materialPath, string texturePath, Color tint, bool additive)
    {
        var shader = Shader.Find("Universal Render Pipeline/Particles/Unlit") ?? Shader.Find("Particles/Standard Unlit");
        if (shader == null)
        {
            return null;
        }

        var material = AssetDatabase.LoadAssetAtPath<Material>(materialPath);
        if (material == null)
        {
            material = new Material(shader);
            AssetDatabase.CreateAsset(material, materialPath);
        }
        else if (material.shader != shader)
        {
            material.shader = shader;
        }

        var texture = AssetDatabase.LoadAssetAtPath<Texture2D>(texturePath);
        if (texture != null)
        {
            if (material.HasProperty("_BaseMap"))
            {
                material.SetTexture("_BaseMap", texture);
            }

            if (material.HasProperty("_MainTex"))
            {
                material.SetTexture("_MainTex", texture);
            }
        }

        if (material.HasProperty("_BaseColor"))
        {
            material.SetColor("_BaseColor", tint);
        }

        if (material.HasProperty("_Color"))
        {
            material.SetColor("_Color", tint);
        }

        if (material.HasProperty("_Surface"))
        {
            material.SetFloat("_Surface", 1f);
        }

        if (material.HasProperty("_Blend"))
        {
            material.SetFloat("_Blend", additive ? 2f : 0f);
        }

        if (material.HasProperty("_AlphaClip"))
        {
            material.SetFloat("_AlphaClip", 0f);
        }

        material.SetOverrideTag("RenderType", "Transparent");
        material.enableInstancing = true;
        material.renderQueue = 3000;
        EditorUtility.SetDirty(material);
        return material;
    }

    private static T GetOrCreateVolumeComponent<T>(VolumeProfile profile) where T : VolumeComponent
    {
        if (profile == null)
        {
            return null;
        }

        var existing = profile.components.OfType<T>().FirstOrDefault();
        return existing ?? profile.Add<T>(true);
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
