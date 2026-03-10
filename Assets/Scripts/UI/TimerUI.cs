using TMPro;
using UnityEngine;

public class TimerUI : MonoBehaviour
{
    public TextMeshProUGUI timerLabel;

    private void OnEnable()
    {
        if (TimeManager.Instance != null)
        {
            TimeManager.Instance.TimeUpdated += HandleTimeUpdated;
            HandleTimeUpdated(TimeManager.Instance.CurrentGameHour);
        }
    }

    private void OnDisable()
    {
        if (TimeManager.Instance != null)
        {
            TimeManager.Instance.TimeUpdated -= HandleTimeUpdated;
        }
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
        timerLabel.text = $"Timer {minutes:00}:{seconds:00}";
    }
}
