using System;
using UnityEngine;

public enum CitizenBehaviorPreset
{
    OfficeWorker,
    Student,
    Shopkeeper,
    Elder,
    DeliveryDriver,
    Walker,
    Couple,
    Guard,
    StreetVendor
}

[Serializable]
public struct FloatRange
{
    public float min;
    public float max;

    public float Sample(float fallback)
    {
        if (max <= min)
        {
            return min <= 0f ? fallback : min;
        }

        return UnityEngine.Random.Range(min, max);
    }
}

[Serializable]
public class PersonalityRanges
{
    public FloatRange walkSpeed = new FloatRange { min = 0.9f, max = 1.1f };
    public FloatRange patience = new FloatRange { min = 5f, max = 10f };
    public FloatRange sociability = new FloatRange { min = 0.1f, max = 0.5f };
    public FloatRange curiosity = new FloatRange { min = 0.1f, max = 0.3f };
    public FloatRange routineBreak = new FloatRange { min = 0f, max = 0.15f };
    public FloatRange phoneAddiction = new FloatRange { min = 0f, max = 0.15f };
    public FloatRange awareness = new FloatRange { min = 0.2f, max = 0.5f };
}

[CreateAssetMenu(menuName = "BlendIn/Archetype")]
public class ArchetypeData : ScriptableObject
{
    public string archetypeId = "OfficeWorker";
    [Min(1)] public int count = 1;
    public string spawnZoneTag = "Home";
    public CitizenBehaviorPreset behaviorPreset = CitizenBehaviorPreset.OfficeWorker;
    public ScheduleTable scheduleTable;
    public PersonalityRanges personalityRanges = new PersonalityRanges();
}
