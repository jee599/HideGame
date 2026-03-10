using System;
using System.Collections;
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
    [SerializeField] private float resultSceneDelaySeconds = 1.5f;

    public GameState CurrentState { get; private set; } = GameState.Ready;
    public bool IsPlaying => CurrentState == GameState.Playing;

    public event Action<GameState> StateChanged;

    private bool _runResolved;
    private Coroutine _resultSceneRoutine;

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
        if (_resultSceneRoutine != null)
        {
            StopCoroutine(_resultSceneRoutine);
            _resultSceneRoutine = null;
        }

        SceneFlow.ClearSummary();
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

        var score = ScoreManager.Instance != null ? ScoreManager.Instance.CurrentScore : 0;
        var suspicion = FindFirstObjectByType<SuspicionSystem>();
        var missionManager = FindFirstObjectByType<MissionManager>();
        var disguise = FindFirstObjectByType<PlayerDisguise>();
        SceneFlow.RecordRun(
            survived,
            score,
            suspicion != null ? suspicion.PeakSuspicion : 0f,
            missionManager != null ? missionManager.CompletedMissionCount : 0,
            disguise != null ? disguise.UsedDisguises : 0,
            TimeManager.Instance != null ? TimeManager.Instance.ElapsedRealSeconds : 0f);

        if (_resultSceneRoutine != null)
        {
            StopCoroutine(_resultSceneRoutine);
        }

        _resultSceneRoutine = StartCoroutine(LoadResultSceneAfterDelay());
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

    private IEnumerator LoadResultSceneAfterDelay()
    {
        yield return new WaitForSecondsRealtime(Mathf.Max(0f, resultSceneDelaySeconds));
        _resultSceneRoutine = null;
        SceneFlow.LoadResultScene();
    }
}

public static class SceneFlow
{
    public const string MainMenuSceneName = "MainMenu";
    public const string GameSceneName = "GameScene";
    public const string ResultSceneName = "ResultScene";

    public static bool HasSummary { get; private set; }
    public static bool LastSurvived { get; private set; }
    public static int LastScore { get; private set; }
    public static float LastPeakSuspicion { get; private set; }
    public static int LastCompletedMissions { get; private set; }
    public static int LastUsedDisguises { get; private set; }
    public static float LastRealSeconds { get; private set; }

    public static void StartNewRun()
    {
        ClearSummary();
        UnityEngine.SceneManagement.SceneManager.LoadScene(GameSceneName);
    }

    public static void LoadMainMenu()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene(MainMenuSceneName);
    }

    public static void LoadResultScene()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene(ResultSceneName);
    }

    public static void RecordRun(bool survived, int score, float peakSuspicion, int completedMissions, int usedDisguises, float realSeconds)
    {
        HasSummary = true;
        LastSurvived = survived;
        LastScore = Mathf.Max(0, score);
        LastPeakSuspicion = Mathf.Max(0f, peakSuspicion);
        LastCompletedMissions = Mathf.Max(0, completedMissions);
        LastUsedDisguises = Mathf.Max(0, usedDisguises);
        LastRealSeconds = Mathf.Max(0f, realSeconds);
    }

    public static void ClearSummary()
    {
        HasSummary = false;
        LastSurvived = false;
        LastScore = 0;
        LastPeakSuspicion = 0f;
        LastCompletedMissions = 0;
        LastUsedDisguises = 0;
        LastRealSeconds = 0f;
    }
}
