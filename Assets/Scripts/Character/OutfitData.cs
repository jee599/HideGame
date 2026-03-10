using UnityEngine;

[CreateAssetMenu(menuName = "BlendIn/OutfitData")]
public class OutfitData : ScriptableObject
{
    public string outfitId = "Outfit";
    public int headIndex = -1;
    public int hairIndex = -1;
    public int bodyIndex = -1;
    public int legIndex = -1;
    public int accessoryIndex = -1;
    public bool useExplicitColor;
    public Color tintColor = Color.white;
    public Color[] paletteOverride;
}
