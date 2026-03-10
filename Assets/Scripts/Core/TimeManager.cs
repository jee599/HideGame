using System;
using UnityEngine;

public class TimeManager : MonoBehaviour
{
    public static TimeManager Instance { get; private set; }

    [Header("Settings")]
    [Min(1f)] public float realDurationSeconds = 180f;
    [Range(0f, 24f)] public float gameStartHour = 8f;
    [Range(0f, 24f)] public float gameEndHour = 20f;

    public float CurrentGameHour { get; private set; }
    public float ElapsedRealSeconds { get; private set; }
    public float NormalizedTime => Mathf.InverseLerp(gameStartHour, gameEndHour, CurrentGameHour);
    public bool IsGameOver => CurrentGameHour >= gameEndHour;

    public event Action<float> TimeUpdated;
    public event Action DayEnded;

    private bool _raisedDayEnded;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        ResetClock();
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

        if (_raisedDayEnded)
        {
            return;
        }

        ElapsedRealSeconds = Mathf.Min(ElapsedRealSeconds + Time.deltaTime, realDurationSeconds);
        var t = Mathf.Clamp01(ElapsedRealSeconds / realDurationSeconds);
        CurrentGameHour = Mathf.Lerp(gameStartHour, gameEndHour, t);
        TimeUpdated?.Invoke(CurrentGameHour);

        if (IsGameOver)
        {
            _raisedDayEnded = true;
            DayEnded?.Invoke();
        }
    }

    public void ResetClock()
    {
        _raisedDayEnded = false;
        ElapsedRealSeconds = 0f;
        CurrentGameHour = gameStartHour;
        TimeUpdated?.Invoke(CurrentGameHour);
    }

    public float GetMinutesRemaining()
    {
        return Mathf.Max(0f, (gameEndHour - CurrentGameHour) * 60f);
    }

    private void OnValidate()
    {
        gameEndHour = Mathf.Max(gameStartHour + 0.1f, gameEndHour);
    }
}
