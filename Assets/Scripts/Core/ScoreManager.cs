using System;
using UnityEngine;

public class ScoreManager : MonoBehaviour
{
    public static ScoreManager Instance { get; private set; }

    [SerializeField] private int survivalBonus = 200;

    public int CurrentScore { get; private set; }
    public bool Survived { get; private set; }

    public event Action<int> ScoreChanged;
    public event Action<bool, int> RunFinished;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }

    public void ResetScore()
    {
        CurrentScore = 0;
        Survived = false;
        ScoreChanged?.Invoke(CurrentScore);
    }

    public void AddScore(int amount)
    {
        CurrentScore = Mathf.Max(0, CurrentScore + amount);
        ScoreChanged?.Invoke(CurrentScore);
    }

    public void FinalizeRun(bool survived)
    {
        Survived = survived;
        if (survived)
        {
            AddScore(survivalBonus);
        }

        RunFinished?.Invoke(Survived, CurrentScore);
    }
}
