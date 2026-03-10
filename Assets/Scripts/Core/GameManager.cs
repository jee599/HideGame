using System;
using UnityEngine;

public enum GameState
{
    Ready,
    Playing,
    GameOver
}

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [SerializeField] private bool autoStartOnAwake = true;

    public GameState CurrentState { get; private set; } = GameState.Ready;
    public bool IsPlaying => CurrentState == GameState.Playing;

    public event Action<GameState> StateChanged;

    private bool _runResolved;

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
        if (TimeManager.Instance != null)
        {
            TimeManager.Instance.DayEnded += HandleDayEnded;
        }

        if (autoStartOnAwake)
        {
            StartGame();
        }
    }

    private void OnDestroy()
    {
        if (TimeManager.Instance != null)
        {
            TimeManager.Instance.DayEnded -= HandleDayEnded;
        }

        if (Instance == this)
        {
            Instance = null;
        }
    }

    public void StartGame()
    {
        _runResolved = false;
        TimeManager.Instance?.ResetClock();
        ScoreManager.Instance?.ResetScore();
        EventManager.Instance?.ResetSession();
        FindFirstObjectByType<SuspicionSystem>()?.ResetSuspicion();
        FindFirstObjectByType<PlayerDisguise>()?.ResetDisguises();
        FindFirstObjectByType<MissionManager>()?.ResetSession();
        SetState(GameState.Playing);
    }

    public void EndGame(bool survived)
    {
        if (_runResolved)
        {
            return;
        }

        _runResolved = true;
        ScoreManager.Instance?.FinalizeRun(survived);
        SetState(GameState.GameOver);
    }

    private void HandleDayEnded()
    {
        EndGame(true);
    }

    private void SetState(GameState newState)
    {
        if (CurrentState == newState)
        {
            return;
        }

        CurrentState = newState;
        StateChanged?.Invoke(CurrentState);
    }
}
