using TMPro;
using UnityEngine;

public class GameOverUI : MonoBehaviour
{
    public GameObject root;
    public TextMeshProUGUI titleLabel;
    public TextMeshProUGUI scoreLabel;
    public TextMeshProUGUI summaryLabel;

    private bool _subscribed;

    private void Start()
    {
        if (root != null)
        {
            root.SetActive(false);
        }

        TrySubscribe();
    }

    private void Update()
    {
        if (!_subscribed)
        {
            TrySubscribe();
        }
    }

    private void OnDestroy()
    {
        if (_subscribed && ScoreManager.Instance != null)
        {
            ScoreManager.Instance.RunFinished -= HandleRunFinished;
        }
    }

    private void TrySubscribe()
    {
        if (ScoreManager.Instance == null || _subscribed)
        {
            return;
        }

        ScoreManager.Instance.RunFinished += HandleRunFinished;
        _subscribed = true;
    }

    private void HandleRunFinished(bool survived, int score)
    {
        if (root != null)
        {
            root.SetActive(true);
        }

        if (titleLabel != null)
        {
            titleLabel.text = survived ? "Blend Successful" : "Caught";
        }

        if (scoreLabel != null)
        {
            scoreLabel.text = $"Score {score}";
        }

        if (summaryLabel != null)
        {
            summaryLabel.text = survived ? "You made it to 20:00." : "The Hunter locked you down.";
        }
    }
}
