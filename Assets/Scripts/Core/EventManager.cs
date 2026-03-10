using System.Collections.Generic;
using UnityEngine;

public class EventManager : MonoBehaviour
{
    private struct ScheduledEvent
    {
        public GameEvent EventAsset;
        public float StartTimeRealSeconds;
    }

    public static EventManager Instance { get; private set; }

    public GameEvent[] availableEvents;
    [Range(1, 7)] public int eventsPerGame = 3;
    [Min(10f)] public float minimumSpacingSeconds = 60f;

    public GameEvent ActiveEvent { get; private set; }

    public event System.Action<GameEvent> EventStarted;
    public event System.Action<GameEvent> EventEnded;

    private readonly List<ScheduledEvent> _scheduledEvents = new List<ScheduledEvent>();

    private PlayerController _player;
    private SuspicionSystem _suspicion;
    private float _activeEventEndTime;
    private bool _sessionPrepared;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    private void Start()
    {
        CacheSceneReferences();
        PrepareSession();
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }

    private void Update()
    {
        if (GameManager.Instance != null && !GameManager.Instance.IsPlaying)
        {
            return;
        }

        if (_player == null || _suspicion == null)
        {
            CacheSceneReferences();
        }

        if (TimeManager.Instance == null)
        {
            return;
        }

        if (!_sessionPrepared)
        {
            PrepareSession();
        }

        if (ActiveEvent != null)
        {
            ActiveEvent.OnEventTick(_player, _suspicion);
            if (TimeManager.Instance.ElapsedRealSeconds >= _activeEventEndTime)
            {
                EndActiveEvent();
            }

            return;
        }

        if (_scheduledEvents.Count == 0)
        {
            return;
        }

        if (TimeManager.Instance.ElapsedRealSeconds >= _scheduledEvents[0].StartTimeRealSeconds)
        {
            StartEvent(_scheduledEvents[0]);
            _scheduledEvents.RemoveAt(0);
        }
    }

    public void ResetSession()
    {
        _scheduledEvents.Clear();
        if (ActiveEvent != null)
        {
            ActiveEvent.OnEventEnd();
        }

        ActiveEvent = null;
        _activeEventEndTime = 0f;
        _sessionPrepared = false;
        CacheSceneReferences();
        PrepareSession();
    }

    public bool TryGetReactionForCitizen(CitizenAI citizen, out EventReaction reaction)
    {
        reaction = default;
        return ActiveEvent != null && ActiveEvent.TryGetCitizenReaction(citizen, out reaction);
    }

    private void CacheSceneReferences()
    {
        _player = FindFirstObjectByType<PlayerController>();
        _suspicion = FindFirstObjectByType<SuspicionSystem>();
    }

    private void PrepareSession()
    {
        _scheduledEvents.Clear();
        if (availableEvents == null || availableEvents.Length == 0)
        {
            _sessionPrepared = true;
            return;
        }

        var eventCount = Mathf.Min(eventsPerGame, availableEvents.Length);
        var totalDuration = TimeManager.Instance != null ? TimeManager.Instance.realDurationSeconds : 180f;
        var choices = new List<GameEvent>(availableEvents);
        var startTimes = new List<float>(eventCount);

        for (var i = 0; i < eventCount && choices.Count > 0; i++)
        {
            var pickIndex = Random.Range(0, choices.Count);
            var pickedEvent = choices[pickIndex];
            choices.RemoveAt(pickIndex);

            var baseSlice = totalDuration / (eventCount + 1);
            var idealStart = baseSlice * (i + 1);
            var startTime = Mathf.Clamp(
                idealStart + Random.Range(-15f, 15f),
                10f,
                Mathf.Max(10f, totalDuration - Mathf.Max(5f, pickedEvent != null ? pickedEvent.duration : 5f)));

            if (startTimes.Count > 0)
            {
                startTime = Mathf.Max(startTime, startTimes[startTimes.Count - 1] + minimumSpacingSeconds);
            }

            if (startTime >= totalDuration)
            {
                break;
            }

            startTimes.Add(startTime);
            _scheduledEvents.Add(new ScheduledEvent
            {
                EventAsset = pickedEvent,
                StartTimeRealSeconds = startTime
            });
        }

        _scheduledEvents.Sort((left, right) => left.StartTimeRealSeconds.CompareTo(right.StartTimeRealSeconds));
        _sessionPrepared = true;
    }

    private void StartEvent(ScheduledEvent scheduledEvent)
    {
        if (scheduledEvent.EventAsset == null || TimeManager.Instance == null)
        {
            return;
        }

        ActiveEvent = scheduledEvent.EventAsset;
        _activeEventEndTime = TimeManager.Instance.ElapsedRealSeconds + ActiveEvent.duration;
        ActiveEvent.OnEventStart();
        EventStarted?.Invoke(ActiveEvent);
    }

    private void EndActiveEvent()
    {
        if (ActiveEvent == null)
        {
            return;
        }

        var finishedEvent = ActiveEvent;
        ActiveEvent.OnEventEnd();
        ActiveEvent = null;
        _activeEventEndTime = 0f;
        EventEnded?.Invoke(finishedEvent);
    }
}
