using UnityEngine;

public enum HunterType
{
    Patrol,
    Intuitive,
    Ambush,
    CCTV,
    GuardDog
}

public enum HunterPatrolStyle
{
    FixedRoute,
    RandomZone,
    StayAndWatch,
    RoutePlusCamera,
    FastPatrol
}

[CreateAssetMenu(menuName = "BlendIn/HunterConfig")]
public class HunterConfig : ScriptableObject
{
    public HunterType type = HunterType.Patrol;
    public HunterPatrolStyle patrolStyle = HunterPatrolStyle.FixedRoute;
    public float patrolSpeed = 3f;
    public float investigateSpeed = 2.5f;
    public float chaseSpeed = 4.5f;
    [Range(10f, 360f)] public float viewAngle = 90f;
    public float viewRange = 15f;
    public float cctvRange = 30f;
    public float investigateDuration = 5f;
    public float lockdownDuration = 10f;
    public float crowdLoseTime = 3f;
    public float captureDistance = 1.75f;
}
