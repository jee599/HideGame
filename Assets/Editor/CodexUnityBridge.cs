#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Animations;
using UnityEditor.Build.Reporting;
using UnityEngine;
using TMPro;

public static class CodexUnityBridge
{
    private const string ProjectScenePath = "Assets/_Project/Scenes/GameScene.unity";
    private const string ArtSetPath = "Assets/_Project/Data/Art/PrototypeArtSet.asset";
    private const string DefaultBuildRoot = "Builds";

    [MenuItem("Blend In/Codex/Rebuild Prototype")]
    public static void RebuildPrototype()
    {
        BlendInBootstrapper.BootstrapPrototype();
        Debug.Log("CodexUnityBridge: prototype rebuilt.");
    }

    [MenuItem("Blend In/Codex/Auto Bind Art Set")]
    public static void AutoBindArtSet()
    {
        BlendInBootstrapper.BootstrapPrototype();

        var artSet = AssetDatabase.LoadAssetAtPath<BlendInArtSet>(ArtSetPath);
        if (artSet == null)
        {
            throw new InvalidOperationException("PrototypeArtSet.asset was not created.");
        }

        var changes = new List<string>();
        AutoConfigureMixamoImports(changes);
        AutoConfigureCharacterMeshImports(changes);
        AutoAssignVisualPrefabs(artSet, changes);
        AutoAssignEnvironmentPrefabs(artSet, changes);
        AutoAssignMaterials(artSet, changes);
        AutoUpgradeImportedMaterials(changes);

        EditorUtility.SetDirty(artSet);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        if (changes.Count == 0)
        {
            Debug.Log("CodexUnityBridge: no imported art candidates were found. PrototypeArtSet remains in fallback mode.");
            return;
        }

        Debug.Log("CodexUnityBridge: art set updated.\n" + string.Join("\n", changes));
    }

    [MenuItem("Blend In/Codex/Import TMP Essentials")]
    public static void ImportTmpEssentials()
    {
        if (AssetDatabase.FindAssets("t:TMP_Settings").Length > 0)
        {
            Debug.Log("CodexUnityBridge: TMP essentials already available.");
            return;
        }

        TMP_PackageResourceImporter.ImportResources(importEssentials: true, importExamples: false, interactive: false);
        AssetDatabase.Refresh();
        Debug.Log("CodexUnityBridge: TMP essentials imported.");
    }

    [MenuItem("Blend In/Codex/Rebuild + Auto Bind")]
    public static void RebuildAndAutoBind()
    {
        ImportTmpEssentials();
        RebuildPrototype();
        AutoBindArtSet();
        RebuildPrototype();
        Debug.Log("CodexUnityBridge: rebuild + auto bind complete.");
    }

    [MenuItem("Blend In/Codex/Build Current Target")]
    public static void BuildCurrentTarget()
    {
        RebuildAndAutoBind();

        var enabledScenes = EditorBuildSettings.scenes
            .Where(scene => scene.enabled)
            .Select(scene => scene.path)
            .ToArray();

        if (enabledScenes.Length == 0)
        {
            throw new InvalidOperationException("No enabled scenes were found in EditorBuildSettings.");
        }

        var outputPath = GetBuildOutputPath();
        Directory.CreateDirectory(Path.GetDirectoryName(outputPath) ?? DefaultBuildRoot);

        var buildOptions = GetBuildOptionsFromEnvironment();
        var report = BuildPipeline.BuildPlayer(enabledScenes, outputPath, EditorUserBuildSettings.activeBuildTarget, buildOptions);

        if (report.summary.result != BuildResult.Succeeded)
        {
            throw new InvalidOperationException(
                $"Build failed for {EditorUserBuildSettings.activeBuildTarget}: {report.summary.result}. Output: {outputPath}");
        }

        Debug.Log($"CodexUnityBridge: build succeeded. Output: {outputPath}");
    }

    public static void EnsurePrototypeReady()
    {
        RebuildAndAutoBind();
    }

    private static void AutoAssignVisualPrefabs(BlendInArtSet artSet, List<string> changes)
    {
        var prefabPaths = AssetDatabase.FindAssets("t:Prefab")
            .Select(AssetDatabase.GUIDToAssetPath)
            .Where(IsImportedArtAssetPath)
            .ToArray();

        var baseCharacterPrefab = LoadPreferredCharacterBasePrefab();
        artSet.citizenVisualPrefab = AssignIfFound(
            artSet.citizenVisualPrefab,
            FindBestCharacterPrefab(prefabPaths, baseCharacterPrefab, "citizen", "npc", "crowd"),
            "citizenVisualPrefab",
            changes);

        artSet.playerVisualPrefab = AssignIfFound(
            artSet.playerVisualPrefab,
            FindBestCharacterPrefab(prefabPaths, baseCharacterPrefab, "player", "hero", "main"),
            "playerVisualPrefab",
            changes);

        artSet.hunterVisualPrefab = AssignIfFound(
            artSet.hunterVisualPrefab,
            FindBestCharacterPrefab(prefabPaths, baseCharacterPrefab, "hunter", "guard", "police", "security"),
            "hunterVisualPrefab",
            changes);

        if (artSet.citizenVisualPrefab != null || artSet.playerVisualPrefab != null || artSet.hunterVisualPrefab != null)
        {
            artSet.tintImportedCharacterRenderers = false;
        }
    }

    private static void AutoAssignMaterials(BlendInArtSet artSet, List<string> changes)
    {
        var materialPaths = AssetDatabase.FindAssets("t:Material")
            .Select(AssetDatabase.GUIDToAssetPath)
            .Where(IsImportedArtAssetPath)
            .ToArray();

        artSet.citizenMaterial = AssignIfFound(
            artSet.citizenMaterial,
            FindBestAsset<Material>(materialPaths, "character", "citizen", "people"),
            "citizenMaterial",
            changes);

        artSet.playerMaterial = AssignIfFound(
            artSet.playerMaterial,
            FindBestAsset<Material>(materialPaths, "character", "player", "people"),
            "playerMaterial",
            changes);

        artSet.hunterMaterial = AssignIfFound(
            artSet.hunterMaterial,
            FindBestAsset<Material>(materialPaths, "guard", "hunter", "police", "character"),
            "hunterMaterial",
            changes);

        artSet.groundMaterial = AssignIfFound(
            artSet.groundMaterial,
            LoadFirstAsset<Material>(
                "Assets/ithappy/Cartoon_City_Free/Materials/Grass.mat",
                "Assets/ithappy/Cartoon_City_Free/Materials/Tile_1.mat")
            ?? FindBestAsset<Material>(materialPaths, "ground", "grass", "park", "terrain"),
            "groundMaterial",
            changes);

        artSet.buildingMaterial = AssignIfFound(
            artSet.buildingMaterial,
            LoadFirstAsset<Material>(
                "Assets/ithappy/Cartoon_City_Free/Materials/Color.mat",
                "Assets/ithappy/Cartoon_City_Free/Materials/Tile_1.mat")
            ?? FindBestAsset<Material>(materialPaths, "building", "wall", "city", "house"),
            "buildingMaterial",
            changes);

        artSet.roadMaterial = AssignIfFound(
            artSet.roadMaterial,
            LoadFirstAsset<Material>(
                "Assets/ithappy/Cartoon_City_Free/Materials/Roads.mat",
                "Assets/ithappy/Cartoon_City_Free/Materials/Asphalt_Dark_Gray.mat")
            ?? FindBestAsset<Material>(materialPaths, "road", "street", "asphalt", "pavement"),
            "roadMaterial",
            changes);
    }

    private static void AutoAssignEnvironmentPrefabs(BlendInArtSet artSet, List<string> changes)
    {
        artSet.buildingVisualPrefabs = AssignPrefabArray(
            artSet.buildingVisualPrefabs,
            LoadCuratedBuildingPrefabs(),
            "buildingVisualPrefabs",
            changes);

        artSet.roadVisualPrefabs = AssignPrefabArray(
            artSet.roadVisualPrefabs,
            LoadCuratedRoadPrefabs(),
            "roadVisualPrefabs",
            changes);

        artSet.sidewalkVisualPrefabs = AssignPrefabArray(
            artSet.sidewalkVisualPrefabs,
            LoadCuratedSidewalkPrefabs(),
            "sidewalkVisualPrefabs",
            changes);

        artSet.parkVisualPrefabs = AssignPrefabArray(
            artSet.parkVisualPrefabs,
            LoadCuratedParkPrefabs(),
            "parkVisualPrefabs",
            changes);

        artSet.streetPropPrefabs = AssignPrefabArray(
            artSet.streetPropPrefabs,
            LoadCuratedStreetProps(),
            "streetPropPrefabs",
            changes);

        artSet.plazaVisualPrefab = AssignIfFound(
            artSet.plazaVisualPrefab,
            LoadFirstPrefab(
                "Assets/ithappy/Cartoon_City_Free/Prefabs/Props/Fountain_03.prefab"),
            "plazaVisualPrefab",
            changes);

        artSet.busStopVisualPrefab = AssignIfFound(
            artSet.busStopVisualPrefab,
            LoadFirstPrefab(
                "Assets/ithappy/Cartoon_City_Free/Prefabs/Props/Bus_Stop_02.prefab"),
            "busStopVisualPrefab",
            changes);
    }

    private static void AutoUpgradeImportedMaterials(List<string> changes)
    {
        var materialGuids = AssetDatabase.FindAssets("t:Material", new[]
        {
            "Assets/Imported/Kenney",
            "Assets/ithappy"
        });

        if (materialGuids == null || materialGuids.Length == 0)
        {
            return;
        }

        var targetShader = Shader.Find("Universal Render Pipeline/Lit");
        if (targetShader == null)
        {
            return;
        }

        var changedCount = 0;
        for (var i = 0; i < materialGuids.Length; i++)
        {
            var path = AssetDatabase.GUIDToAssetPath(materialGuids[i]);
            var material = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (material == null || material.shader == targetShader)
            {
                continue;
            }

            var mainTexture = material.mainTexture;
            var baseColor = material.HasProperty("_BaseColor")
                ? material.GetColor("_BaseColor")
                : material.HasProperty("_Color")
                    ? material.color
                    : Color.white;

            material.shader = targetShader;
            if (mainTexture != null && material.HasProperty("_BaseMap"))
            {
                material.SetTexture("_BaseMap", mainTexture);
            }

            if (material.HasProperty("_BaseColor"))
            {
                material.SetColor("_BaseColor", baseColor);
            }

            EditorUtility.SetDirty(material);
            changedCount++;
        }

        if (changedCount > 0)
        {
            changes.Add($"imported materials upgraded to URP/Lit ({changedCount})");
        }
    }

    private static void ResetEnvironmentBindings(BlendInArtSet artSet, List<string> changes)
    {
        if (artSet == null)
        {
            return;
        }

        if ((artSet.buildingVisualPrefabs != null && artSet.buildingVisualPrefabs.Length > 0)
            || (artSet.roadVisualPrefabs != null && artSet.roadVisualPrefabs.Length > 0)
            || (artSet.sidewalkVisualPrefabs != null && artSet.sidewalkVisualPrefabs.Length > 0)
            || (artSet.parkVisualPrefabs != null && artSet.parkVisualPrefabs.Length > 0)
            || (artSet.streetPropPrefabs != null && artSet.streetPropPrefabs.Length > 0)
            || artSet.plazaVisualPrefab != null
            || artSet.busStopVisualPrefab != null)
        {
            changes.Add("environment visuals reset to stable prototype mode");
        }

        artSet.buildingVisualPrefabs = Array.Empty<GameObject>();
        artSet.roadVisualPrefabs = Array.Empty<GameObject>();
        artSet.sidewalkVisualPrefabs = Array.Empty<GameObject>();
        artSet.parkVisualPrefabs = Array.Empty<GameObject>();
        artSet.streetPropPrefabs = Array.Empty<GameObject>();
        artSet.plazaVisualPrefab = null;
        artSet.busStopVisualPrefab = null;
    }

    private static T AssignIfFound<T>(T currentValue, T candidate, string label, List<string> changes) where T : UnityEngine.Object
    {
        if (candidate == null || currentValue == candidate)
        {
            return currentValue;
        }

        changes.Add($"{label} -> {AssetDatabase.GetAssetPath(candidate)}");
        return candidate;
    }

    private static GameObject[] AssignPrefabArray(GameObject[] currentValue, GameObject[] candidate, string label, List<string> changes)
    {
        candidate ??= Array.Empty<GameObject>();
        var current = currentValue ?? Array.Empty<GameObject>();
        if (current.Length == candidate.Length && current.SequenceEqual(candidate))
        {
            return currentValue;
        }

        if (candidate.Length == 0)
        {
            return currentValue;
        }

        changes.Add($"{label} -> {candidate.Length} prefabs");
        return candidate;
    }

    private static GameObject FindBestCharacterPrefab(IEnumerable<string> prefabPaths, GameObject fallbackPrefab, params string[] keywords)
    {
        var candidates = prefabPaths
            .Select(path => new
            {
                Path = path,
                Prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path),
                Score = ScoreCharacterPrefab(path, keywords)
            })
            .Where(entry => entry.Prefab != null && entry.Score > 0)
            .OrderByDescending(entry => entry.Score)
            .ThenBy(entry => entry.Path.Length)
            .ToArray();

        foreach (var candidate in candidates)
        {
            if (!IsUsableCharacterPrefab(candidate.Prefab))
            {
                continue;
            }

            return candidate.Prefab;
        }

        return IsUsableCharacterPrefab(fallbackPrefab) ? fallbackPrefab : null;
    }

    private static T FindBestAsset<T>(IEnumerable<string> assetPaths, params string[] keywords) where T : UnityEngine.Object
    {
        return assetPaths
            .Select(path => new
            {
                Path = path,
                Asset = AssetDatabase.LoadAssetAtPath<T>(path),
                Score = ScorePath(path, keywords)
            })
            .Where(entry => entry.Asset != null && entry.Score > 0)
            .OrderByDescending(entry => entry.Score)
            .ThenBy(entry => entry.Path.Length)
            .Select(entry => entry.Asset)
            .FirstOrDefault();
    }

    private static void AutoConfigureMixamoImports(List<string> changes)
    {
        var modelPaths = AssetDatabase.FindAssets("t:Model", new[] { "Assets/Imported/Mixamo" })
            .Select(AssetDatabase.GUIDToAssetPath)
            .Where(path => path.EndsWith(".fbx", StringComparison.OrdinalIgnoreCase))
            .ToArray();

        for (var i = 0; i < modelPaths.Length; i++)
        {
            if (!ConfigureMixamoImporter(modelPaths[i]))
            {
                continue;
            }

            changes.Add("mixamo -> " + modelPaths[i]);
        }
    }

    private static void AutoConfigureCharacterMeshImports(List<string> changes)
    {
        var candidatePaths = new[]
        {
            "Assets/ithappy/Creative_Characters_FREE/Meshes/Base_Mesh.fbx",
            "Assets/ithappy/Creative_Characters_FREE/Animations/Animation_Mesh/Aminset_Basic.fbx"
        };

        for (var i = 0; i < candidatePaths.Length; i++)
        {
            if (!ConfigureCharacterModelImporter(candidatePaths[i]))
            {
                continue;
            }

            changes.Add("character rig -> " + candidatePaths[i]);
        }
    }

    private static bool ConfigureMixamoImporter(string path)
    {
        var importer = AssetImporter.GetAtPath(path) as ModelImporter;
        if (importer == null)
        {
            return false;
        }

        var changed = false;
        if (importer.animationType != ModelImporterAnimationType.Human)
        {
            importer.animationType = ModelImporterAnimationType.Human;
            changed = true;
        }

        if (importer.avatarSetup != ModelImporterAvatarSetup.CreateFromThisModel)
        {
            importer.avatarSetup = ModelImporterAvatarSetup.CreateFromThisModel;
            changed = true;
        }

        var defaultClips = importer.defaultClipAnimations;
        if (defaultClips != null && defaultClips.Length > 0)
        {
            var shouldLoop = ShouldLoopMixamoAnimation(path);
            var clips = new ModelImporterClipAnimation[defaultClips.Length];
            for (var i = 0; i < defaultClips.Length; i++)
            {
                clips[i] = defaultClips[i];
                if (clips[i].loopTime != shouldLoop)
                {
                    clips[i].loopTime = shouldLoop;
                    clips[i].loopPose = shouldLoop;
                    changed = true;
                }
            }

            importer.clipAnimations = clips;
        }

        if (!changed)
        {
            return false;
        }

        importer.SaveAndReimport();
        return true;
    }

    private static bool ConfigureCharacterModelImporter(string path)
    {
        var importer = AssetImporter.GetAtPath(path) as ModelImporter;
        if (importer == null)
        {
            return false;
        }

        var changed = false;
        if (importer.animationType != ModelImporterAnimationType.Human)
        {
            importer.animationType = ModelImporterAnimationType.Human;
            changed = true;
        }

        if (importer.avatarSetup != ModelImporterAvatarSetup.CreateFromThisModel)
        {
            importer.avatarSetup = ModelImporterAvatarSetup.CreateFromThisModel;
            changed = true;
        }

        if (importer.importAnimation)
        {
            importer.importAnimation = path.IndexOf("Animation_Mesh", StringComparison.OrdinalIgnoreCase) >= 0;
            changed = true;
        }

        if (!changed)
        {
            return false;
        }

        importer.SaveAndReimport();
        return true;
    }

    private static bool ShouldLoopMixamoAnimation(string path)
    {
        var loweredPath = Path.GetFileNameWithoutExtension(path).ToLowerInvariant();
        return loweredPath.Contains("idle")
            || loweredPath.Contains("walk")
            || loweredPath.Contains("run")
            || loweredPath.Contains("phone");
    }

    private static GameObject LoadPreferredCharacterBasePrefab()
    {
        const string preferredPath = "Assets/ithappy/Creative_Characters_FREE/Prefabs/Base_Mesh.prefab";
        var preferred = AssetDatabase.LoadAssetAtPath<GameObject>(preferredPath);
        if (preferred != null)
        {
            return preferred;
        }

        var controllerGuid = AssetDatabase.FindAssets("Base_Mesh t:Prefab")
            .Select(AssetDatabase.GUIDToAssetPath)
            .FirstOrDefault(path => path.EndsWith("Base_Mesh.prefab", StringComparison.OrdinalIgnoreCase));
        return string.IsNullOrEmpty(controllerGuid)
            ? null
            : AssetDatabase.LoadAssetAtPath<GameObject>(controllerGuid);
    }

    private static GameObject[] LoadPrefabsFromFolders(params string[] folders)
    {
        return folders
            .Where(path => AssetDatabase.IsValidFolder(path))
            .SelectMany(path => AssetDatabase.FindAssets("t:Prefab", new[] { path })
                .Select(AssetDatabase.GUIDToAssetPath))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(path => path, StringComparer.OrdinalIgnoreCase)
            .Select(path => AssetDatabase.LoadAssetAtPath<GameObject>(path))
            .Where(prefab => prefab != null)
            .ToArray();
    }

    private static GameObject[] LoadCuratedBuildingPrefabs()
    {
        return LoadAssetsFromPaths<GameObject>(
            "Assets/Imported/Kenney/CityKitSuburban/Models/FBX format/building-type-a.fbx",
            "Assets/Imported/Kenney/CityKitSuburban/Models/FBX format/building-type-b.fbx",
            "Assets/Imported/Kenney/CityKitSuburban/Models/FBX format/building-type-c.fbx",
            "Assets/Imported/Kenney/CityKitSuburban/Models/FBX format/building-type-d.fbx",
            "Assets/Imported/Kenney/CityKitSuburban/Models/FBX format/building-type-e.fbx",
            "Assets/Imported/Kenney/CityKitSuburban/Models/FBX format/building-type-f.fbx",
            "Assets/Imported/Kenney/CityKitSuburban/Models/FBX format/building-type-g.fbx",
            "Assets/Imported/Kenney/CityKitSuburban/Models/FBX format/building-type-h.fbx",
            "Assets/Imported/Kenney/CityKitSuburban/Models/FBX format/building-type-i.fbx",
            "Assets/Imported/Kenney/CityKitSuburban/Models/FBX format/building-type-j.fbx",
            "Assets/Imported/Kenney/CityKitSuburban/Models/FBX format/building-type-k.fbx",
            "Assets/Imported/Kenney/CityKitSuburban/Models/FBX format/building-type-l.fbx",
            "Assets/Imported/Kenney/CityKitSuburban/Models/FBX format/building-type-m.fbx",
            "Assets/Imported/Kenney/CityKitSuburban/Models/FBX format/building-type-n.fbx",
            "Assets/Imported/Kenney/CityKitSuburban/Models/FBX format/building-type-o.fbx",
            "Assets/Imported/Kenney/CityKitSuburban/Models/FBX format/building-type-p.fbx",
            "Assets/Imported/Kenney/CityKitSuburban/Models/FBX format/building-type-q.fbx",
            "Assets/Imported/Kenney/CityKitSuburban/Models/FBX format/building-type-r.fbx",
            "Assets/Imported/Kenney/CityKitSuburban/Models/FBX format/building-type-s.fbx",
            "Assets/Imported/Kenney/CityKitSuburban/Models/FBX format/building-type-t.fbx",
            "Assets/Imported/Kenney/CityKitSuburban/Models/FBX format/building-type-u.fbx");
    }

    private static GameObject[] LoadCuratedRoadPrefabs()
    {
        return LoadAssetsFromPaths<GameObject>(
            "Assets/Imported/Kenney/RetroUrbanKit/Models/FBX format/road-asphalt-straight.fbx",
            "Assets/Imported/Kenney/RetroUrbanKit/Models/FBX format/road-asphalt-side.fbx",
            "Assets/Imported/Kenney/RetroUrbanKit/Models/FBX format/road-asphalt-center.fbx",
            "Assets/Imported/Kenney/RetroUrbanKit/Models/FBX format/road-asphalt-pavement.fbx");
    }

    private static GameObject[] LoadCuratedSidewalkPrefabs()
    {
        return LoadAssetsFromPaths<GameObject>(
            "Assets/Imported/Kenney/RetroUrbanKit/Models/FBX format/road-asphalt-pavement.fbx",
            "Assets/Imported/Kenney/RetroUrbanKit/Models/FBX format/road-asphalt-side.fbx",
            "Assets/Imported/Kenney/CityKitSuburban/Models/FBX format/path-long.fbx",
            "Assets/Imported/Kenney/CityKitSuburban/Models/FBX format/path-short.fbx",
            "Assets/Imported/Kenney/CityKitSuburban/Models/FBX format/path-stones-long.fbx",
            "Assets/Imported/Kenney/CityKitSuburban/Models/FBX format/path-stones-short.fbx");
    }

    private static GameObject[] LoadCuratedParkPrefabs()
    {
        return LoadAssetsFromPaths<GameObject>(
            "Assets/Imported/Kenney/CityKitSuburban/Models/FBX format/tree-large.fbx",
            "Assets/Imported/Kenney/CityKitSuburban/Models/FBX format/tree-small.fbx",
            "Assets/Imported/Kenney/CityKitSuburban/Models/FBX format/planter.fbx",
            "Assets/Imported/Kenney/RetroUrbanKit/Models/FBX format/tree-park-large.fbx",
            "Assets/Imported/Kenney/RetroUrbanKit/Models/FBX format/tree-large.fbx",
            "Assets/Imported/Kenney/RetroUrbanKit/Models/FBX format/tree-small.fbx",
            "Assets/Imported/Kenney/RetroUrbanKit/Models/FBX format/tree-shrub.fbx");
    }

    private static GameObject[] LoadCuratedStreetProps()
    {
        return LoadAssetsFromPaths<GameObject>(
            "Assets/Imported/Kenney/RetroUrbanKit/Models/FBX format/detail-bench.fbx",
            "Assets/Imported/Kenney/RetroUrbanKit/Models/FBX format/detail-light-single.fbx",
            "Assets/Imported/Kenney/RetroUrbanKit/Models/FBX format/detail-light-double.fbx",
            "Assets/Imported/Kenney/RetroUrbanKit/Models/FBX format/detail-dumpster-closed.fbx",
            "Assets/Imported/Kenney/RetroUrbanKit/Models/FBX format/truck-grey.fbx",
            "Assets/Imported/Kenney/RetroUrbanKit/Models/FBX format/truck-green.fbx",
            "Assets/Imported/Kenney/CityKitSuburban/Models/FBX format/planter.fbx");
    }

    private static GameObject LoadFirstPrefab(params string[] preferredPaths)
    {
        for (var i = 0; i < preferredPaths.Length; i++)
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(preferredPaths[i]);
            if (prefab != null)
            {
                return prefab;
            }
        }

        return null;
    }

    private static T LoadFirstAsset<T>(params string[] preferredPaths) where T : UnityEngine.Object
    {
        for (var i = 0; i < preferredPaths.Length; i++)
        {
            var asset = AssetDatabase.LoadAssetAtPath<T>(preferredPaths[i]);
            if (asset != null)
            {
                return asset;
            }
        }

        return null;
    }

    private static T[] LoadAssetsFromPaths<T>(params string[] paths) where T : UnityEngine.Object
    {
        return paths
            .Select(AssetDatabase.LoadAssetAtPath<T>)
            .Where(asset => asset != null)
            .Distinct()
            .ToArray();
    }

    private static bool IsUsableCharacterPrefab(GameObject prefab)
    {
        if (prefab == null)
        {
            return false;
        }

        return prefab.GetComponentsInChildren<Renderer>(true).Length > 0
            && prefab.GetComponentsInChildren<SkinnedMeshRenderer>(true).Length > 0;
    }

    private static int ScoreCharacterPrefab(string path, IEnumerable<string> keywords)
    {
        if (string.IsNullOrEmpty(path))
        {
            return 0;
        }

        var loweredPath = path.ToLowerInvariant();
        var score = ScorePath(path, keywords);

        if (loweredPath.Contains("base_mesh"))
        {
            score += 120;
        }

        if (ContainsAny(loweredPath, "/body/", "/costumes/", "/outfit/", "/outwear/"))
        {
            score += 70;
        }

        if (ContainsAny(loweredPath, "body_", "costume_", "outfit_", "outwear_"))
        {
            score += 35;
        }

        if (ContainsAny(
                loweredPath,
                "/faces/",
                "emotion",
                "face accessories",
                "/glasses/",
                "/gloves/",
                "/hairstyle",
                "/hat",
                "/pants/",
                "/shorts/",
                "/shoes/",
                "/socks/",
                "mustache",
                "mascot",
                "pacifier",
                "clown"))
        {
            score -= 250;
        }

        var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
        if (prefab == null)
        {
            return 0;
        }

        if (prefab.GetComponentInChildren<Animator>(true) != null)
        {
            score += 60;
        }

        var skinnedMeshCount = prefab.GetComponentsInChildren<SkinnedMeshRenderer>(true).Length;
        if (skinnedMeshCount > 0)
        {
            score += 20 + (skinnedMeshCount * 3);
        }
        else
        {
            score -= 50;
        }

        var rendererCount = prefab.GetComponentsInChildren<Renderer>(true).Length;
        if (rendererCount >= 4)
        {
            score += 15;
        }
        else if (rendererCount <= 1)
        {
            score -= 25;
        }

        return score;
    }

    private static int ScorePath(string path, IEnumerable<string> keywords)
    {
        if (string.IsNullOrEmpty(path))
        {
            return 0;
        }

        var loweredPath = path.ToLowerInvariant();
        var score = 0;

        if (loweredPath.Contains("creative") || loweredPath.Contains("character"))
        {
            score += 5;
        }

        if (loweredPath.Contains("cartoon") || loweredPath.Contains("urban") || loweredPath.Contains("city"))
        {
            score += 3;
        }

        if (loweredPath.Contains("/materials/"))
        {
            score += 8;
        }

        foreach (var keyword in keywords)
        {
            if (loweredPath.Contains(keyword.ToLowerInvariant()))
            {
                score += 10;
            }
        }

        if (ContainsAny(loweredPath, "demo", "sample", "editor only", "/editor/"))
        {
            score -= 15;
        }

        if (loweredPath.Contains("_project"))
        {
            score = 0;
        }

        return score;
    }

    private static bool ContainsAny(string value, params string[] tokens)
    {
        for (var i = 0; i < tokens.Length; i++)
        {
            if (value.Contains(tokens[i], StringComparison.Ordinal))
            {
                return true;
            }
        }

        return false;
    }

    private static bool IsImportedArtAssetPath(string path)
    {
        if (string.IsNullOrEmpty(path))
        {
            return false;
        }

        if (!path.StartsWith("Assets/", StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        if (path.StartsWith("Assets/_Project/", StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        if (path.Contains("/Editor/", StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        return true;
    }

    private static string GetBuildOutputPath()
    {
        var customOutput = Environment.GetEnvironmentVariable("CODEX_UNITY_BUILD_OUTPUT");
        if (!string.IsNullOrWhiteSpace(customOutput))
        {
            return customOutput;
        }

        var buildTarget = EditorUserBuildSettings.activeBuildTarget;
        var extension = GetBuildExtension(buildTarget);
        var fileName = "HideGame" + extension;
        return Path.Combine(DefaultBuildRoot, buildTarget.ToString(), fileName);
    }

    private static string GetBuildExtension(BuildTarget buildTarget)
    {
        return buildTarget switch
        {
            BuildTarget.StandaloneWindows or BuildTarget.StandaloneWindows64 => ".exe",
            BuildTarget.Android => ".apk",
            _ => string.Empty
        };
    }

    private static BuildOptions GetBuildOptionsFromEnvironment()
    {
        var options = BuildOptions.None;
        if (string.Equals(Environment.GetEnvironmentVariable("CODEX_UNITY_DEVELOPMENT_BUILD"), "1", StringComparison.Ordinal))
        {
            options |= BuildOptions.Development;
        }

        if (string.Equals(Environment.GetEnvironmentVariable("CODEX_UNITY_AUTORUN_PLAYER"), "1", StringComparison.Ordinal))
        {
            options |= BuildOptions.AutoRunPlayer;
        }

        return options;
    }
}
#endif
