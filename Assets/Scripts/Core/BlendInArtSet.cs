using System;
using UnityEngine;

[CreateAssetMenu(menuName = "BlendIn/Art Set")]
public class BlendInArtSet : ScriptableObject
{
    [Header("Character Visuals")]
    public GameObject citizenVisualPrefab;
    public GameObject playerVisualPrefab;
    public GameObject hunterVisualPrefab;
    public bool tintImportedCharacterRenderers;

    [Header("Environment Visuals")]
    public GameObject[] buildingVisualPrefabs;
    public GameObject[] roadVisualPrefabs;
    public GameObject[] sidewalkVisualPrefabs;
    public GameObject[] parkVisualPrefabs;
    public GameObject[] streetPropPrefabs;
    public GameObject plazaVisualPrefab;
    public GameObject busStopVisualPrefab;

    [Header("Shared Materials")]
    public Material citizenMaterial;
    public Material playerMaterial;
    public Material hunterMaterial;
    public Material groundMaterial;
    public Material buildingMaterial;
    public Material roadMaterial;
}
