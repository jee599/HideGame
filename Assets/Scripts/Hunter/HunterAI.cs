using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public enum HunterState
{
    Patrol,
    Investigate,
    Chase,
    Lockdown
}

public interface IHunterState
{
    void Enter(HunterAI hunter);
    void Tick(HunterAI hunter);
    void Exit(HunterAI hunter);
}

[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(DetectionSystem))]
public class HunterAI : MonoBehaviour
{
    private static readonly List<HunterAI> ActiveHunters = new List<HunterAI>();

    public HunterState currentState = HunterState.Patrol;
    public HunterConfig config;
    public Transform[] patrolRoute;

    public NavMeshAgent Agent { get; private set; }
    public DetectionSystem Detection { get; private set; }
    public Animator Animator { get; private set; }
    public PlayerController Player { get; private set; }
    public SuspicionSystem Suspicion { get; private set; }
    public float StateElapsedTime { get; private set; }
    public float LockdownRemainingTime { get; private set; }
    public Vector3 LastKnownPlayerPosition { get; private set; }
    public float LostSightTime { get; private set; }
    public float CrowdHiddenTime { get; private set; }

    public event Action<HunterAI, HunterState> StateChanged;

    private readonly PatrolState _patrolState = new PatrolState();
    private readonly InvestigateState _investigateState = new InvestigateState();
    private readonly ChaseState _chaseState = new ChaseState();
    private readonly LockdownState _lockdownState = new LockdownState();

    private IHunterState _stateImpl;
    private int _patrolIndex;
    private bool _hasRandomPatrolDestination;
    private Vector3 _randomPatrolDestination;

    private void Awake()
    {
        Agent = GetComponent<NavMeshAgent>();
        Detection = GetComponent<DetectionSystem>();
        Animator = GetComponentInChildren<Animator>();
    }

    private void OnEnable()
    {
        if (!ActiveHunters.Contains(this))
        {
            ActiveHunters.Add(this);
        }
    }

    private void OnDisable()
    {
        ActiveHunters.Remove(this);
    }

    private void Start()
    {
        Player = FindFirstObjectByType<PlayerController>();
        Suspicion = FindFirstObjectByType<SuspicionSystem>();
        ChangeState(HunterState.Patrol);
    }

    private void Update()
    {
        if (GameManager.Instance != null && !GameManager.Instance.IsPlaying)
        {
            return;
        }

        if (Player == null)
        {
            Player = FindFirstObjectByType<PlayerController>();
        }

        if (Suspicion == null)
        {
            Suspicion = FindFirstObjectByType<SuspicionSystem>();
        }

        StateElapsedTime += Time.deltaTime;
        if (currentState == HunterState.Lockdown && config != null)
        {
            LockdownRemainingTime = Mathf.Max(0f, config.lockdownDuration - StateElapsedTime);
        }

        var seesPlayer = Player != null && SeesPlayer();
        if (seesPlayer)
        {
            LastKnownPlayerPosition = Player.transform.position;
            LostSightTime = 0f;
            CrowdHiddenTime = 0f;
        }
        else
        {
            LostSightTime += Time.deltaTime;
            CrowdHiddenTime = Player != null && Player.IsInCrowd ? CrowdHiddenTime + Time.deltaTime : 0f;
        }

        if (IsPrimaryHunter() && Player != null)
        {
            Player.SetHunterEyeContact(AnyHunterMakingEyeContact());
        }

        ApplyObservationPressure(seesPlayer);
        UpdateAnimator();
        _stateImpl?.Tick(this);
    }

    public static bool AnyHunterSeeingPlayer()
    {
        for (var i = 0; i < ActiveHunters.Count; i++)
        {
            if (ActiveHunters[i] != null && ActiveHunters[i].SeesPlayer())
            {
                return true;
            }
        }

        return false;
    }

    public static bool AnyHunterMakingEyeContact()
    {
        for (var i = 0; i < ActiveHunters.Count; i++)
        {
            var hunter = ActiveHunters[i];
            if (hunter == null || hunter.Player == null || !hunter.SeesPlayer())
            {
                continue;
            }

            if (hunter.Player.IsFacingPosition(hunter.transform.position))
            {
                return true;
            }
        }

        return false;
    }

    public static void NotifyPlayerDisguised(bool witnessed, string outfitId)
    {
        for (var i = 0; i < ActiveHunters.Count; i++)
        {
            var hunter = ActiveHunters[i];
            if (hunter != null)
            {
                hunter.HandlePlayerDisguised(witnessed, outfitId);
            }
        }
    }

    public static HunterAI GetPrimaryHunter()
    {
        return ActiveHunters.Count > 0 ? ActiveHunters[0] : null;
    }

    public float GetCurrentViewRange()
    {
        if (config == null)
        {
            return 15f;
        }

        return config.type == HunterType.CCTV ? Mathf.Max(config.viewRange, config.cctvRange) : config.viewRange;
    }

    public bool SeesPlayer()
    {
        return Detection != null && Detection.CanSeePlayer(Player);
    }

    public float DistanceToPlayer()
    {
        return Detection != null ? Detection.DistanceToPlayer(Player) : float.MaxValue;
    }

    public void MoveAlongPatrol()
    {
        var patrolStyle = config != null ? config.patrolStyle : HunterPatrolStyle.FixedRoute;
        switch (patrolStyle)
        {
            case HunterPatrolStyle.RandomZone:
                MoveRandomPatrol();
                break;
            case HunterPatrolStyle.StayAndWatch:
                if (Agent != null && Agent.isOnNavMesh)
                {
                    Agent.ResetPath();
                }
                break;
            default:
                MoveSequentialPatrol();
                break;
        }
    }

    public void MoveToPlayer(float stoppingDistance)
    {
        if (Player == null)
        {
            return;
        }

        Agent.stoppingDistance = stoppingDistance;
        SetDestination(Player.transform.position);
    }

    public void MoveToLastKnownPosition(float stoppingDistance)
    {
        Agent.stoppingDistance = stoppingDistance;
        SetDestination(LastKnownPlayerPosition);
    }

    public bool HasReachedDestination()
    {
        return !Agent.pathPending && Agent.remainingDistance <= Agent.stoppingDistance + 0.25f;
    }

    public bool HasLostPlayerLongEnough(float seconds)
    {
        return LostSightTime >= Mathf.Max(0f, seconds);
    }

    public bool IsPlayerHiddenInCrowd()
    {
        return Player != null && Player.IsInCrowd && !SeesPlayer();
    }

    public bool IsPlayerHiddenInCrowdLongEnough()
    {
        return config != null ? CrowdHiddenTime >= config.crowdLoseTime : CrowdHiddenTime >= 3f;
    }

    public void ChangeState(HunterState newState)
    {
        _stateImpl?.Exit(this);
        currentState = newState;
        StateElapsedTime = 0f;
        LockdownRemainingTime = config != null ? config.lockdownDuration : 0f;
        _hasRandomPatrolDestination = false;

        _stateImpl = newState switch
        {
            HunterState.Investigate => _investigateState,
            HunterState.Chase => _chaseState,
            HunterState.Lockdown => _lockdownState,
            _ => _patrolState
        };

        _stateImpl.Enter(this);
        StateChanged?.Invoke(this, currentState);
    }

    public void ApplySpeed(float speed)
    {
        if (Agent != null)
        {
            Agent.speed = speed;
        }
    }

    public void HandlePlayerDisguised(bool witnessed, string outfitId)
    {
        if (witnessed)
        {
            return;
        }

        if (currentState == HunterState.Chase || currentState == HunterState.Lockdown)
        {
            ChangeState(HunterState.Investigate);
        }
    }

    private void MoveSequentialPatrol()
    {
        if (patrolRoute == null || patrolRoute.Length == 0)
        {
            return;
        }

        var target = patrolRoute[_patrolIndex];
        if (target == null)
        {
            return;
        }

        SetDestination(target.position);
        if (!Agent.pathPending && Agent.remainingDistance <= Agent.stoppingDistance + 0.2f)
        {
            _patrolIndex = (_patrolIndex + 1) % patrolRoute.Length;
        }
    }

    private void MoveRandomPatrol()
    {
        if (!_hasRandomPatrolDestination || HasReachedDestination())
        {
            if (TryGetRandomPatrolDestination(out var patrolDestination))
            {
                _randomPatrolDestination = patrolDestination;
                _hasRandomPatrolDestination = true;
            }
        }

        if (_hasRandomPatrolDestination)
        {
            SetDestination(_randomPatrolDestination);
        }
    }

    private void SetDestination(Vector3 destination)
    {
        if (Agent != null && Agent.isOnNavMesh)
        {
            Agent.SetDestination(destination);
        }
    }

    private bool TryGetRandomPatrolDestination(out Vector3 destination)
    {
        if (patrolRoute != null && patrolRoute.Length > 0)
        {
            for (var attempt = 0; attempt < patrolRoute.Length; attempt++)
            {
                var target = patrolRoute[UnityEngine.Random.Range(0, patrolRoute.Length)];
                if (target != null)
                {
                    destination = target.position;
                    return true;
                }
            }
        }

        for (var attempt = 0; attempt < 8; attempt++)
        {
            var radius = config != null ? Mathf.Max(8f, config.viewRange) : 15f;
            var candidate = transform.position + new Vector3(UnityEngine.Random.Range(-radius, radius), 0f, UnityEngine.Random.Range(-radius, radius));
            if (NavMesh.SamplePosition(candidate, out var hit, radius, NavMesh.AllAreas))
            {
                destination = hit.position;
                return true;
            }
        }

        destination = transform.position;
        return false;
    }

    private void ApplyObservationPressure(bool seesPlayer)
    {
        if (!seesPlayer || Suspicion == null || Player == null)
        {
            return;
        }

        var pressure = currentState switch
        {
            HunterState.Investigate => 4.5f,
            HunterState.Chase => 8f,
            HunterState.Lockdown => 12f,
            _ => 1.5f
        };

        if (!Player.IsInCrowd)
        {
            pressure += 4f;
        }

        if (!Player.IsSheltered)
        {
            pressure += 2f;
        }

        if (string.IsNullOrEmpty(Player.CurrentZoneTag))
        {
            pressure += 3f;
        }

        if (DistanceToPlayer() <= 6f)
        {
            pressure += 4f;
        }

        if (Player.IsLoitering && !Player.IsInCrowd)
        {
            pressure += 3f;
        }

        if (Player.IsInCrowd && Player.IsActingNatural)
        {
            pressure = Mathf.Max(0f, pressure - 5f);
        }

        if (pressure > 0f)
        {
            Suspicion.AddContinuousPenalty(pressure);
        }
    }

    private void UpdateAnimator()
    {
        if (Animator == null)
        {
            Animator = GetComponentInChildren<Animator>();
        }

        if (Animator == null || Animator.runtimeAnimatorController == null || Agent == null)
        {
            return;
        }

        var state = Agent.velocity.magnitude <= 0.1f
            ? CitizenAnimationState.Idle
            : currentState == HunterState.Chase || currentState == HunterState.Lockdown
                ? CitizenAnimationState.Run
                : CitizenAnimationState.Walk;
        CharacterAnimatorDriver.ApplyLocomotion(
            Animator,
            transform,
            Agent.velocity,
            Mathf.Max(Agent.speed, 0.01f),
            state,
            state == CitizenAnimationState.Run);
    }

    private bool IsPrimaryHunter()
    {
        return ActiveHunters.Count > 0 && ActiveHunters[0] == this;
    }
}
