using UnityEngine;

[CreateAssetMenu(menuName = "BlendIn/PersonalityProfile")]
public class PersonalityProfile : ScriptableObject
{
    [Range(0.7f, 1.3f)] public float walkSpeed = 1f;
    [Range(3f, 15f)] public float patience = 8f;
    [Range(0f, 1f)] public float sociability = 0.3f;
    [Range(0f, 1f)] public float curiosity = 0.2f;
    [Range(0f, 0.3f)] public float routineBreak = 0.1f;
    [Range(0f, 0.5f)] public float phoneAddiction = 0.1f;
    [Range(0f, 1f)] public float awareness = 0.3f;

    public static PersonalityProfile GenerateRandom(ArchetypeData archetype)
    {
        var profile = CreateInstance<PersonalityProfile>();
        if (archetype == null || archetype.personalityRanges == null)
        {
            return profile;
        }

        var ranges = archetype.personalityRanges;
        profile.walkSpeed = ranges.walkSpeed.Sample(profile.walkSpeed);
        profile.patience = ranges.patience.Sample(profile.patience);
        profile.sociability = Mathf.Clamp01(ranges.sociability.Sample(profile.sociability));
        profile.curiosity = Mathf.Clamp01(ranges.curiosity.Sample(profile.curiosity));
        profile.routineBreak = Mathf.Clamp(ranges.routineBreak.Sample(profile.routineBreak), 0f, 0.3f);
        profile.phoneAddiction = Mathf.Clamp(ranges.phoneAddiction.Sample(profile.phoneAddiction), 0f, 0.5f);
        profile.awareness = Mathf.Clamp01(ranges.awareness.Sample(profile.awareness));
        return profile;
    }
}
