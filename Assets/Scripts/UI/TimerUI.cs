using TMPro;
using UnityEngine;

public class TimerUI : MonoBehaviour
{
    public TextMeshProUGUI timerLabel;
    public TextMeshProUGUI scoreLabel;
    public TextMeshProUGUI hunterLabel;
    public TextMeshProUGUI eventLabel;
    public TextMeshProUGUI guideLabel;

    private ScoreManager _scoreManager;
    private EventManager _eventManager;
    private bool _scoreSubscribed;
    private bool _eventSubscribed;

    private void OnEnable()
    {
        if (TimeManager.Instance != null)
        {
            TimeManager.Instance.TimeUpdated += HandleTimeUpdated;
            HandleTimeUpdated(TimeManager.Instance.CurrentGameHour);
        }

        TryBindSystems();
        RefreshDynamicLabels();
    }

    private void OnDisable()
    {
        if (TimeManager.Instance != null)
        {
            TimeManager.Instance.TimeUpdated -= HandleTimeUpdated;
        }

        UnbindSystems();
    }

    private void Update()
    {
        TryBindSystems();
        RefreshDynamicLabels();
    }

    private void HandleTimeUpdated(float currentHour)
    {
        if (timerLabel == null || TimeManager.Instance == null)
        {
            return;
        }

        var remaining = Mathf.Max(0f, TimeManager.Instance.realDurationSeconds - TimeManager.Instance.ElapsedRealSeconds);
        var minutes = Mathf.FloorToInt(remaining / 60f);
        var seconds = Mathf.FloorToInt(remaining % 60f);
        var displayHour = Mathf.FloorToInt(currentHour);
        var displayMinute = Mathf.FloorToInt((currentHour - displayHour) * 60f);
        if (displayMinute >= 60)
        {
            displayHour += 1;
            displayMinute = 0;
        }

        timerLabel.text = $"{displayHour:00}:{displayMinute:00}  |  {minutes:00}:{seconds:00} left";
    }

    private void TryBindSystems()
    {
        if (!_scoreSubscribed && ScoreManager.Instance != null)
        {
            _scoreManager = ScoreManager.Instance;
            _scoreManager.ScoreChanged += HandleScoreChanged;
            _scoreSubscribed = true;
            HandleScoreChanged(_scoreManager.CurrentScore);
        }

        if (!_eventSubscribed && EventManager.Instance != null)
        {
            _eventManager = EventManager.Instance;
            _eventManager.EventStarted += HandleEventChanged;
            _eventManager.EventEnded += HandleEventChanged;
            _eventSubscribed = true;
            RefreshEventLabel();
        }
    }

    private void UnbindSystems()
    {
        if (_scoreSubscribed && _scoreManager != null)
        {
            _scoreManager.ScoreChanged -= HandleScoreChanged;
        }

        if (_eventSubscribed && _eventManager != null)
        {
            _eventManager.EventStarted -= HandleEventChanged;
            _eventManager.EventEnded -= HandleEventChanged;
        }

        _scoreManager = null;
        _eventManager = null;
        _scoreSubscribed = false;
        _eventSubscribed = false;
    }

    private void HandleScoreChanged(int score)
    {
        if (scoreLabel != null)
        {
            scoreLabel.text = $"Score  {score}";
        }
    }

    private void HandleEventChanged(GameEvent _)
    {
        RefreshEventLabel();
    }

    private void RefreshDynamicLabels()
    {
        RefreshHunterLabel();
        RefreshGuideLabel();
        RefreshEventLabel();
    }

    private void RefreshHunterLabel()
    {
        if (hunterLabel == null)
        {
            return;
        }

        var hunter = HunterAI.GetPrimaryHunter();
        if (hunter == null)
        {
            hunterLabel.text = "Hunter  --";
            return;
        }

        hunterLabel.text = hunter.currentState switch
        {
            HunterState.Investigate => "Hunter  Investigating",
            HunterState.Chase => "Hunter  Chasing",
            HunterState.Lockdown => $"Hunter  Lockdown {Mathf.CeilToInt(hunter.LockdownRemainingTime):00}s",
            _ => "Hunter  Patrolling"
        };
    }

    private void RefreshEventLabel()
    {
        if (eventLabel == null)
        {
            return;
        }

        var activeEvent = EventManager.Instance != null ? EventManager.Instance.ActiveEvent : null;
        eventLabel.text = activeEvent != null ? $"Event  {activeEvent.displayName}" : "Event  None";
    }

    private void RefreshGuideLabel()
    {
        if (guideLabel == null)
        {
            return;
        }

        var hunter = HunterAI.GetPrimaryHunter();
        var activeEvent = EventManager.Instance != null ? EventManager.Instance.ActiveEvent : null;
        if (hunter != null && hunter.currentState == HunterState.Lockdown)
        {
            guideLabel.text = "Lockdown: break sight and disappear into a crowd.";
            return;
        }

        if (hunter != null && hunter.currentState == HunterState.Chase)
        {
            guideLabel.text = "Hunter is chasing. Reach a crowd or hide before suspicion caps.";
            return;
        }

        if (activeEvent != null)
        {
            guideLabel.text = activeEvent.displayName switch
            {
                "Rain" => "Rain: get under cover or suspicion spikes.",
                "Police Check" => "Police check: avoid the blocked zone and follow the crowd flow.",
                "Blackout" => "Blackout: empty streets are dangerous. Stay near groups.",
                _ => $"Event live: {activeEvent.displayName}. Use it to blend in."
            };
            return;
        }

        if (TimeManager.Instance != null && TimeManager.Instance.ElapsedRealSeconds < 15f)
        {
            guideLabel.text = "Survive until 20:00. Move like the crowd. Stop inside mission zones to score.";
            return;
        }

        guideLabel.text = "Blend in, stay low, and complete missions when the street feels safe.";
    }
}
