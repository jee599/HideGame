using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MainMenuController : MonoBehaviour
{
    public Button startButton;
    public Button quitButton;
    public TextMeshProUGUI statusLabel;

    private void Start()
    {
        if (startButton != null)
        {
            startButton.onClick.AddListener(HandleStartClicked);
        }

        if (quitButton != null)
        {
            quitButton.onClick.AddListener(HandleQuitClicked);
        }

        if (statusLabel != null)
        {
            statusLabel.text = "3 minutes. 100 citizens. Blend in before the Hunter identifies you.";
        }
    }

    private void OnDestroy()
    {
        if (startButton != null)
        {
            startButton.onClick.RemoveListener(HandleStartClicked);
        }

        if (quitButton != null)
        {
            quitButton.onClick.RemoveListener(HandleQuitClicked);
        }
    }

    private void HandleStartClicked()
    {
        SceneFlow.StartNewRun();
    }

    private void HandleQuitClicked()
    {
        Application.Quit();
    }
}
