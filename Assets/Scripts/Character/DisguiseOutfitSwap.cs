using System;
using UnityEngine;

public class DisguiseOutfitSwap : MonoBehaviour
{
    public CharacterVariation targetVariation;
    public OutfitData[] disguiseOutfits;

    public string LastOutfitId { get; private set; }

    private void Awake()
    {
        if (targetVariation == null)
        {
            targetVariation = GetComponent<CharacterVariation>();
        }
    }

    public string ApplyRandomDisguise()
    {
        if (targetVariation == null)
        {
            return string.Empty;
        }

        if (disguiseOutfits != null && disguiseOutfits.Length > 0)
        {
            var outfit = disguiseOutfits[UnityEngine.Random.Range(0, disguiseOutfits.Length)];
            targetVariation.ApplyOutfit(outfit);
            LastOutfitId = outfit != null ? outfit.outfitId : string.Empty;
            return LastOutfitId;
        }

        targetVariation.Randomize();
        LastOutfitId = Guid.NewGuid().ToString("N");
        return LastOutfitId;
    }
}
