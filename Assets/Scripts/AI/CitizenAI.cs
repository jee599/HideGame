using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class CitizenAI : MonoBehaviour
{
    private static readonly List<CitizenAI> ActiveCitizens = new List<CitizenAI>();
    private static readonly int AnimationStateHash = Animator.StringToHash("State");
    private static readonly int SpeedHash = Animator.StringToHash("Speed");

    [SerializeField] private float socialRadius = 4f;
    [SerializeField] private float phoneCooldownSeconds = 12f;
    [SerializeField] private float socialCooldownSeconds = 15f;
    [SerializeField] private float phoneCheckMinSeconds = 2f;
    [SerializeField] private float phoneCheckMaxSeconds = 5f;
    [SerializeField] private float conversationDurationSeconds = 3f;

    public ArchetypeData Archetype { get; private set; }
    public PersonalityProfile Personality { get; private set; }
    public ScheduleTable.ScheduleOption CurrentScheduleOption { get; private set; }
    public DestinationPoint CurrentDestination { get; private set; }
    public NavMeshAgent Agent { get; private set; }
    public Animator Animator { get; private set; }
    public CharacterVariation CharacterVariation { get; private set; }
    public bool IsBusy => _busyTimer > 0f;
    public bool CanSocialize => Time.time >= _lastSocialStartedAt + socialCooldownSeconds && !IsBusy;
    public bool CanCheckPhone => Time.time >= _lastPhoneStartedAt + phoneCooldownSeconds && !IsBusy;
    public IReadOnlyList<Relationship> Relationships => _relationships;

    private readonly List<Relationship> _relationships = new List<Relationship>();

    private BehaviorTree _behaviorTree;
    private Renderer[] _renderers;
    private float _nextTreeTickAt;
    private float _busyTimer;
    private float _eventWaitTimer;
    private float _lastPhoneStartedAt = -999f;
    private float _lastSocialStartedAt = -999f;
    private int _currentScheduleSlotIndex = -1;
    private string _activeEventId = string.Empty;
    private CitizenAnimationState _requestedAnimationState = CitizenAnimationState.Idle;

    private void Awake()
    {
        Agent = GetComponent<NavMeshAgent>();
        Animator = GetComponentInChildren<Animator>();
        CharacterVariation = GetComponent<CharacterVariation>();
        _renderers = GetComponentsInChildren<Renderer>(true);
    }

    private void OnEnable()
    {
        if (!ActiveCitizens.Contains(this))
        {
            ActiveCitizens.Add(this);
        }
    }

    private void OnDisable()
    {
        ActiveCitizens.Remove(this);
    }

    private void Update()
    {
        TickBusyTimer();
        UpdateAnimator();

        if (Archetype == null || Personality == null)
        {
            return;
        }

        if (GameManager.Instance != null && !GameManager.Instance.IsPlaying)
        {
            return;
        }

        var tickInterval = GetLodTickInterval();
        if (tickInterval > 0f && Time.time < _nextTreeTickAt)
        {
            return;
        }

        _behaviorTree?.Tick(this);
        _nextTreeTickAt = Time.time + tickInterval;
    }

    public void Initialize(ArchetypeData archetype, PersonalityProfile personality)
    {
        Archetype = archetype;
        Personality = personality;
        _currentScheduleSlotIndex = -1;
        _activeEventId = string.Empty;
        _eventWaitTimer = 0f;
        _busyTimer = 0f;
        _requestedAnimationState = CitizenAnimationState.Idle;

        if (Agent != null)
        {
            Agent.speed = Mathf.Max(1f, personality.walkSpeed * 2f);
            Agent.angularSpeed = 720f;
            Agent.acceleration = 12f;
        }

        CharacterVariation?.Randomize();
        _behaviorTree = BehaviorTreeBuilder.Build(this);
    }

    public static IReadOnlyList<CitizenAI> GetAllCitizens()
    {
        return ActiveCitizens;
    }

    public void AddRelationship(CitizenAI other, RelationshipType type)
    {
        if (other == null || other == this)
        {
            return;
        }

        for (var i = 0; i < _relationships.Count; i++)
        {
            if (_relationships[i].Other == other)
            {
                return;
            }
        }

        _relationships.Add(new Relationship { Other = other, Type = type });
    }

    public void ClearRelationships()
    {
        _relationships.Clear();
    }

    public bool TryGetNearbyRelatedCitizen(out CitizenAI relatedCitizen)
    {
        relatedCitizen = null;
        var bestDistance = float.MaxValue;

        for (var i = 0; i < _relationships.Count; i++)
        {
            if (_relationships[i].Type == RelationshipType.Conflict)
            {
                continue;
            }

            var other = _relationships[i].Other;
            if (other == null)
            {
                continue;
            }

            var distance = Vector3.Distance(transform.position, other.transform.position);
            if (distance <= socialRadius && distance < bestDistance)
            {
                relatedCitizen = other;
                bestDistance = distance;
            }
        }

        return relatedCitizen != null;
    }

    public bool TryGetActiveEventReaction(out EventReaction reaction)
    {
        reaction = default;
        return EventManager.Instance != null && EventManager.Instance.TryGetReactionForCitizen(this, out reaction);
    }

    public BTStatus TickEventReaction(EventReaction reaction)
    {
        if (!string.Equals(_activeEventId, reaction.eventId, StringComparison.Ordinal))
        {
            _activeEventId = reaction.eventId;
            _eventWaitTimer = 0f;
        }

        if (!string.IsNullOrEmpty(reaction.destinationTag))
        {
            var destination = DestinationPoint.GetClosest(reaction.destinationTag, transform.position);
            if (destination != null)
            {
                CurrentDestination = destination;
                ApplyTransportMode(reaction.hurry ? TransportMode.Fast : TransportMode.Walk);
                SetDestination(destination.GetBestPoint(transform.position));

                if (!HasReachedDestination())
                {
                    SetAnimationState(CitizenAnimationState.Walk);
                    return BTStatus.Running;
                }
            }
        }

        if (_eventWaitTimer <= 0f)
        {
            _eventWaitTimer = Mathf.Max(0.5f, reaction.waitSeconds);
        }

        _eventWaitTimer = Mathf.Max(0f, _eventWaitTimer - Time.deltaTime);
        SetAnimationState(reaction.animationState);

        if (_eventWaitTimer > 0f)
        {
            return BTStatus.Running;
        }

        _activeEventId = string.Empty;
        return BTStatus.Success;
    }

    public bool RefreshSchedule()
    {
        if (Archetype == null || Archetype.scheduleTable == null || TimeManager.Instance == null)
        {
            return false;
        }

        var slotIndex = Archetype.scheduleTable.GetCurrentSlotIndex(TimeManager.Instance.CurrentGameHour);
        if (slotIndex < 0)
        {
            return false;
        }

        if (slotIndex != _currentScheduleSlotIndex || CurrentDestination == null)
        {
            _currentScheduleSlotIndex = slotIndex;
            CurrentScheduleOption = Archetype.scheduleTable.GetActionAtSlot(slotIndex);
            CurrentDestination = DestinationPoint.GetClosest(CurrentScheduleOption.destinationTag, transform.position);
        }

        return CurrentDestination != null;
    }

    public bool MoveToScheduleDestination()
    {
        if (CurrentDestination == null)
        {
            return false;
        }

        ApplyTransportMode(CurrentScheduleOption.transport);
        SetDestination(CurrentDestination.GetBestPoint(transform.position));
        SetAnimationState(CitizenAnimationState.Walk);
        return true;
    }

    public bool HasReachedDestination()
    {
        if (Agent == null)
        {
            return CurrentDestination != null && Vector3.Distance(transform.position, CurrentDestination.transform.position) <= 1.2f;
        }

        if (Agent.pathPending)
        {
            return false;
        }

        return Agent.remainingDistance <= Agent.stoppingDistance + 0.25f;
    }

    public float GetWaitDuration()
    {
        if (CurrentScheduleOption.waitSeconds > 0f)
        {
            return CurrentScheduleOption.waitSeconds;
        }

        return Personality != null ? Personality.patience : 3f;
    }

    public void BeginWait(float duration, CitizenAnimationState animationState)
    {
        _busyTimer = Mathf.Max(_busyTimer, duration);
        if (Agent != null && Agent.isOnNavMesh)
        {
            Agent.ResetPath();
        }

        SetAnimationState(animationState);
    }

    public void BeginPhoneCheck()
    {
        _lastPhoneStartedAt = Time.time;
        BeginWait(UnityEngine.Random.Range(phoneCheckMinSeconds, phoneCheckMaxSeconds), CitizenAnimationState.Phone);
    }

    public void BeginSocialInteraction()
    {
        _lastSocialStartedAt = Time.time;
        BeginWait(conversationDurationSeconds, CitizenAnimationState.Talk);
    }

    public void SetAnimationState(CitizenAnimationState state)
    {
        if (!HasAnimatorController())
        {
            _requestedAnimationState = state;
            return;
        }

        _requestedAnimationState = state;
        ApplyAnimatorState();
    }

    private void ApplyTransportMode(TransportMode transportMode)
    {
        if (Agent == null || Personality == null)
        {
            return;
        }

        var multiplier = transportMode switch
        {
            TransportMode.Bus => 1.15f,
            TransportMode.Fast => 1.35f,
            _ => 1f
        };

        Agent.speed = Mathf.Max(1f, Personality.walkSpeed * 2f * multiplier);
    }

    private void SetDestination(Vector3 destination)
    {
        if (Agent == null || !Agent.isOnNavMesh)
        {
            return;
        }

        if (!Agent.hasPath || Vector3.Distance(Agent.destination, destination) > 1f)
        {
            Agent.SetDestination(destination);
        }
    }

    private void TickBusyTimer()
    {
        if (_busyTimer > 0f)
        {
            _busyTimer = Mathf.Max(0f, _busyTimer - Time.deltaTime);
        }
    }

    private void UpdateAnimator()
    {
        if (!HasAnimatorController() || Agent == null)
        {
            return;
        }

        Animator.SetFloat(SpeedHash, Agent.velocity.magnitude);
        ApplyAnimatorState();
    }

    private void ApplyAnimatorState()
    {
        if (!HasAnimatorController())
        {
            return;
        }

        var state = _requestedAnimationState;
        if (state == CitizenAnimationState.Walk && Agent != null)
        {
            var normalizedVelocity = Agent.speed > 0.01f ? Agent.velocity.magnitude / Agent.speed : 0f;
            if (normalizedVelocity >= 0.85f && Agent.speed >= 3.6f)
            {
                state = CitizenAnimationState.Run;
            }
        }

        Animator.SetInteger(AnimationStateHash, (int)state);
    }

    private bool HasAnimatorController()
    {
        if (Animator == null)
        {
            Animator = GetComponentInChildren<Animator>();
        }

        return Animator != null && Animator.runtimeAnimatorController != null;
    }

    private float GetLodTickInterval()
    {
        var sceneCamera = Camera.main;
        if (sceneCamera == null)
        {
            return 0.1f;
        }

        var distance = Vector3.Distance(sceneCamera.transform.position, transform.position);
        var visible = false;
        for (var i = 0; i < _renderers.Length; i++)
        {
            if (_renderers[i] != null && _renderers[i].isVisible)
            {
                visible = true;
                break;
            }
        }

        if (!visible)
        {
            return 1f;
        }

        if (distance <= 30f)
        {
            return 0f;
        }

        if (distance <= 60f)
        {
            return 0.2f;
        }

        return 0.5f;
    }
}
