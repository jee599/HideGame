using UnityEngine;

public enum MissionType
{
    CoffeeOrder,
    BusWait,
    BenchSit
}

[CreateAssetMenu(menuName = "BlendIn/MissionData")]
public class MissionData : ScriptableObject
{
    public string missionId = "Mission";
    [TextArea] public string description = "Visit the target zone.";
    public MissionType missionType = MissionType.CoffeeOrder;
    public string targetZoneTag = "Cafe";
    [Min(1f)] public float requiredWaitSeconds = 3f;
    [Min(0)] public int scoreReward = 100;
}
