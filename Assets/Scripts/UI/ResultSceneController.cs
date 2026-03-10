using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ResultSceneController : MonoBehaviour
{
    public TextMeshProUGUI titleLabel;
    public TextMeshProUGUI scoreLabel;
    public TextMeshProUGUI summaryLabel;
    public TextMeshProUGUI detailLabel;
    public Button retryButton;
    public Button menuButton;

    private void Start()
    {
        if (retryButton != null)
        {
            retryButton.onClick.AddListener(HandleRetryClicked);
        }

        if (menuButton != null)
        {
            menuButton.onClick.AddListener(HandleMenuClicked);
        }

        RefreshLabels();
    }

    private void OnDestroy()
    {
        if (retryButton != null)
        {
            retryButton.onClick.RemoveListener(HandleRetryClicked);
        }

        if (menuButton != null)
        {
            menuButton.onClick.RemoveListener(HandleMenuClicked);
        }
    }

    private void RefreshLabels()
    {
        if (!SceneFlow.HasSummary)
        {
            if (titleLabel != null)
            {
                titleLabel.text = "No Run Summary";
            }

            if (scoreLabel != null)
            {
                scoreLabel.text = "Start a run from the main menu.";
            }

            if (summaryLabel != null)
            {
                summaryLabel.text = "Your latest survival report appears here after a match.";
            }

            if (detailLabel != null)
            {
                detailLabel.text = "Missions 0   Peak Suspicion 0   Disguises 0";
            }

            return;
        }

        if (titleLabel != null)
        {
            titleLabel.text = SceneFlow.LastSurvived ? "Blend Successful" : "Hunter Found You";
        }

        if (scoreLabel != null)
        {
            scoreLabel.text = $"Score {SceneFlow.LastScore}";
        }

        if (summaryLabel != null)
        {
            summaryLabel.text = SceneFlow.LastSurvived
                ? $"You survived the full shift and stayed hidden for {Mathf.CeilToInt(SceneFlow.LastRealSeconds)} seconds."
                : $"You were exposed after {Mathf.CeilToInt(SceneFlow.LastRealSeconds)} seconds. Next run: stay inside crowds longer.";
        }

        if (detailLabel != null)
        {
            detailLabel.text =
                $"Missions {SceneFlow.LastCompletedMissions}   Peak Suspicion {Mathf.RoundToInt(SceneFlow.LastPeakSuspicion)}   Disguises {SceneFlow.LastUsedDisguises}";
        }
    }

    private void HandleRetryClicked()
    {
        SceneFlow.StartNewRun();
    }

    private void HandleMenuClicked()
    {
        SceneFlow.LoadMainMenu();
    }
}
