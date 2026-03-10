#if UNITY_EDITOR
using System.Collections.Generic;
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
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public static class PrototypeValidator
{
    private const string MainMenuScenePath = "Assets/_Project/Scenes/MainMenu.unity";
    private const string GameScenePath = "Assets/_Project/Scenes/GameScene.unity";
    private const string ResultScenePath = "Assets/_Project/Scenes/ResultScene.unity";
    private const string ArtSetPath = "Assets/_Project/Data/Art/PrototypeArtSet.asset";
    private const string PlayerPrefabPath = "Assets/_Project/Prefabs/Player.prefab";
    private const string CitizenPrefabPath = "Assets/_Project/Prefabs/Citizen.prefab";
    private const string HunterPrefabPath = "Assets/_Project/Prefabs/Hunter.prefab";

    [MenuItem("Blend In/Codex/Validate Prototype")]
    public static void ValidatePrototypeMenu()
    {
        var issues = ValidatePrototype();
        if (issues.Count == 0)
        {
            Debug.Log("PrototypeValidator: validation passed.");
            return;
        }

        throw new System.InvalidOperationException("Prototype validation failed:\n- " + string.Join("\n- ", issues));
    }

    public static List<string> ValidatePrototype()
    {
        var issues = new List<string>();

        ValidateArtSet(issues);
        ValidatePrefab(PlayerPrefabPath, issues);
        ValidatePrefab(CitizenPrefabPath, issues);
        ValidatePrefab(HunterPrefabPath, issues);
        ValidateGameScene(issues);
        ValidateMenuScene(issues);
        ValidateResultScene(issues);

        return issues;
    }

    private static void ValidateArtSet(List<string> issues)
    {
        var artSet = AssetDatabase.LoadAssetAtPath<BlendInArtSet>(ArtSetPath);
        if (artSet == null)
        {
            issues.Add("PrototypeArtSet.asset missing");
            return;
        }

        if (artSet.citizenVisualPrefab == null || artSet.playerVisualPrefab == null || artSet.hunterVisualPrefab == null)
        {
            issues.Add("character visual prefabs not fully assigned");
        }

        if (artSet.buildingVisualPrefabs == null || artSet.buildingVisualPrefabs.Length < 6)
        {
            issues.Add("building visual prefab set is too small");
        }

        if (artSet.roadVisualPrefabs == null || artSet.roadVisualPrefabs.Length == 0)
        {
            issues.Add("road visual prefabs missing");
        }

        if (artSet.streetPropPrefabs == null || artSet.streetPropPrefabs.Length == 0)
        {
            issues.Add("street prop prefabs missing");
        }

        if (artSet.parkVisualPrefabs == null || artSet.parkVisualPrefabs.Length == 0)
        {
            issues.Add("park visual prefabs missing");
        }
    }

    private static void ValidatePrefab(string path, List<string> issues)
    {
        var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
        if (prefab == null)
        {
            issues.Add($"prefab missing: {path}");
            return;
        }

        if (path.EndsWith("Player.prefab"))
        {
            if (prefab.GetComponent<PlayerController>() == null) issues.Add("Player prefab missing PlayerController");
            if (prefab.GetComponent<SuspicionSystem>() == null) issues.Add("Player prefab missing SuspicionSystem");
            if (prefab.GetComponent<PlayerDisguise>() == null) issues.Add("Player prefab missing PlayerDisguise");
        }
        else if (path.EndsWith("Citizen.prefab"))
        {
            if (prefab.GetComponent<CitizenAI>() == null) issues.Add("Citizen prefab missing CitizenAI");
        }
        else if (path.EndsWith("Hunter.prefab"))
        {
            if (prefab.GetComponent<HunterAI>() == null) issues.Add("Hunter prefab missing HunterAI");
            var detectionCount = prefab.GetComponents<DetectionSystem>().Length;
            if (detectionCount != 1) issues.Add($"Hunter prefab has {detectionCount} DetectionSystem components");
        }

        if (prefab.GetComponentInChildren<Animator>(true) == null)
        {
            issues.Add($"{path} has no Animator");
        }
    }

    private static void ValidateGameScene(List<string> issues)
    {
        var scene = EditorSceneManager.OpenScene(GameScenePath, OpenSceneMode.Single);
        if (!scene.IsValid())
        {
            issues.Add("GameScene failed to open");
            return;
        }

        ValidateObject<GameManager>("GameManager", issues);
        ValidateObject<TimeManager>("TimeManager", issues);
        ValidateObject<ScoreManager>("ScoreManager", issues);
        ValidateObject<EventManager>("EventManager", issues);
        ValidateObject<MissionManager>("MissionManager", issues);
        ValidateObject<RelationshipManager>("RelationshipManager", issues);
        ValidateObject<CitizenSpawner>("CitizenSpawner", issues);
        ValidateObject<GameAudioDirector>("GameAudioDirector", issues);
        ValidateObject<PrototypeVfxDirector>("PrototypeVfxDirector", issues);

        var player = Object.FindFirstObjectByType<PlayerController>();
        if (player == null)
        {
            issues.Add("scene missing Player");
        }

        var hunter = Object.FindFirstObjectByType<HunterAI>();
        if (hunter == null)
        {
            issues.Add("scene missing Hunter");
        }
        else
        {
            if (hunter.patrolRoute == null || hunter.patrolRoute.Length < 3)
            {
                issues.Add("Hunter patrol route not assigned");
            }

            if (hunter.GetComponentInChildren<HunterVisionVisualizer>(true) == null)
            {
                issues.Add("Hunter vision visualizer missing");
            }
        }

        var eventSystem = Object.FindFirstObjectByType<EventSystem>();
        if (eventSystem == null)
        {
            issues.Add("EventSystem missing");
        }
        else
        {
            if (eventSystem.GetComponent<InputSystemUIInputModule>() == null)
            {
                issues.Add("EventSystem missing InputSystemUIInputModule");
            }

            if (eventSystem.GetComponent<StandaloneInputModule>() != null)
            {
                issues.Add("EventSystem still has StandaloneInputModule");
            }
        }

        var timerUi = Object.FindFirstObjectByType<TimerUI>(FindObjectsInactive.Include);
        if (timerUi == null || timerUi.timerLabel == null || timerUi.scoreLabel == null || timerUi.hunterLabel == null || timerUi.eventLabel == null || timerUi.guideLabel == null)
        {
            issues.Add("TimerUI is incomplete");
        }

        var missionUi = Object.FindFirstObjectByType<MissionUI>(FindObjectsInactive.Include);
        if (missionUi == null || missionUi.missionManager == null || missionUi.missionLabel == null || missionUi.progressFill == null)
        {
            issues.Add("MissionUI is incomplete");
        }

        var disguiseUi = Object.FindFirstObjectByType<DisguiseUI>(FindObjectsInactive.Include);
        if (disguiseUi == null || disguiseUi.disguiseButton == null || disguiseUi.progressFill == null || disguiseUi.chargesLabel == null)
        {
            issues.Add("DisguiseUI is incomplete");
        }

        var minimapUi = Object.FindFirstObjectByType<MinimapUI>(FindObjectsInactive.Include);
        if (minimapUi == null || minimapUi.toggleButton == null || minimapUi.minimapRoot == null || minimapUi.mapFrame == null || minimapUi.mapImage == null)
        {
            issues.Add("MinimapUI is incomplete");
        }

        if (Object.FindFirstObjectByType<MinimapCameraFollow>(FindObjectsInactive.Include) == null)
        {
            issues.Add("Minimap camera missing");
        }

        var gameOverUi = Object.FindFirstObjectByType<GameOverUI>(FindObjectsInactive.Include);
        if (gameOverUi == null || gameOverUi.root == null)
        {
            issues.Add("GameOverUI is incomplete");
        }

        var navMeshSurface = Object.FindFirstObjectByType<NavMeshSurface>();
        if (navMeshSurface == null)
        {
            issues.Add("NavMeshSurface missing");
        }

        var triangulation = NavMesh.CalculateTriangulation();
        if (triangulation.vertices == null || triangulation.vertices.Length == 0)
        {
            issues.Add("NavMesh triangulation is empty");
        }

        if (Object.FindObjectsByType<DestinationPoint>(FindObjectsInactive.Include, FindObjectsSortMode.None).Length < 10)
        {
            issues.Add("not enough DestinationPoints in scene");
        }

        if (Object.FindObjectsByType<MissionTrigger>(FindObjectsInactive.Include, FindObjectsSortMode.None).Length < 3)
        {
            issues.Add("mission triggers missing");
        }

        if (Object.FindFirstObjectByType<Volume>(FindObjectsInactive.Include) == null)
        {
            issues.Add("Global Volume missing");
        }
    }

    private static void ValidateMenuScene(List<string> issues)
    {
        var scene = EditorSceneManager.OpenScene(MainMenuScenePath, OpenSceneMode.Single);
        if (!scene.IsValid())
        {
            issues.Add("MainMenu failed to open");
            return;
        }

        var controller = Object.FindFirstObjectByType<MainMenuController>(FindObjectsInactive.Include);
        if (controller == null)
        {
            issues.Add("MainMenuController missing");
        }

        if (Object.FindFirstObjectByType<GameAudioDirector>() == null)
        {
            issues.Add("MainMenu GameAudioDirector missing");
        }

        if (Object.FindObjectsByType<Button>(FindObjectsInactive.Include, FindObjectsSortMode.None).Length < 2)
        {
            issues.Add("MainMenu buttons missing");
        }

        if (Object.FindObjectsByType<TextMeshProUGUI>(FindObjectsInactive.Include, FindObjectsSortMode.None).Length < 3)
        {
            issues.Add("MainMenu text layout is incomplete");
        }

        ValidateEventSystem(issues, "MainMenu");
    }

    private static void ValidateResultScene(List<string> issues)
    {
        var scene = EditorSceneManager.OpenScene(ResultScenePath, OpenSceneMode.Single);
        if (!scene.IsValid())
        {
            issues.Add("ResultScene failed to open");
            return;
        }

        var controller = Object.FindFirstObjectByType<ResultSceneController>(FindObjectsInactive.Include);
        if (controller == null)
        {
            issues.Add("ResultSceneController missing");
        }

        if (Object.FindFirstObjectByType<GameAudioDirector>() == null)
        {
            issues.Add("ResultScene GameAudioDirector missing");
        }

        if (Object.FindObjectsByType<Button>(FindObjectsInactive.Include, FindObjectsSortMode.None).Length < 2)
        {
            issues.Add("ResultScene buttons missing");
        }

        if (Object.FindObjectsByType<TextMeshProUGUI>(FindObjectsInactive.Include, FindObjectsSortMode.None).Length < 4)
        {
            issues.Add("ResultScene text layout is incomplete");
        }

        ValidateEventSystem(issues, "ResultScene");
    }

    private static void ValidateEventSystem(List<string> issues, string sceneLabel)
    {
        var eventSystem = Object.FindFirstObjectByType<EventSystem>();
        if (eventSystem == null)
        {
            issues.Add($"{sceneLabel} EventSystem missing");
            return;
        }

        if (eventSystem.GetComponent<InputSystemUIInputModule>() == null)
        {
            issues.Add($"{sceneLabel} EventSystem missing InputSystemUIInputModule");
        }

        if (eventSystem.GetComponent<StandaloneInputModule>() != null)
        {
            issues.Add($"{sceneLabel} EventSystem still has StandaloneInputModule");
        }
    }

    private static void ValidateObject<T>(string label, List<string> issues) where T : Object
    {
        if (Object.FindFirstObjectByType<T>() == null)
        {
            issues.Add($"{label} missing");
        }
    }
}
#endif
