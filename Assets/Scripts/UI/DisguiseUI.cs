using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DisguiseUI : MonoBehaviour
{
    public PlayerDisguise playerDisguise;
    public Button disguiseButton;
    public Image progressFill;
    public TextMeshProUGUI chargesLabel;

    private int _charges;
    private bool _isDisguising;

    private void Start()
    {
        if (playerDisguise == null)
        {
            playerDisguise = FindFirstObjectByType<PlayerDisguise>();
        }

        if (disguiseButton != null)
        {
            disguiseButton.onClick.AddListener(HandleDisguiseClicked);
        }

        if (playerDisguise != null)
        {
            playerDisguise.ProgressChanged += UpdateProgress;
            playerDisguise.ChargesChanged += UpdateCharges;
            playerDisguise.DisguiseStateChanged += UpdateButtonState;
            UpdateCharges(playerDisguise.remainingDisguises);
            UpdateButtonState(playerDisguise.isDisguising);
            UpdateProgress(0f);
        }
    }

    private void OnDestroy()
    {
        if (disguiseButton != null)
        {
            disguiseButton.onClick.RemoveListener(HandleDisguiseClicked);
        }

        if (playerDisguise == null)
        {
            return;
        }

        playerDisguise.ProgressChanged -= UpdateProgress;
        playerDisguise.ChargesChanged -= UpdateCharges;
        playerDisguise.DisguiseStateChanged -= UpdateButtonState;
    }

    private void HandleDisguiseClicked()
    {
        playerDisguise?.TryStartDisguise();
    }

    private void UpdateProgress(float progress)
    {
        if (progressFill != null)
        {
            progressFill.fillAmount = progress;
        }
    }

    private void UpdateCharges(int charges)
    {
        _charges = charges;
        if (chargesLabel != null)
        {
            chargesLabel.text = $"Disguise x{charges}";
        }

        RefreshButtonState();
    }

    private void UpdateButtonState(bool isDisguising)
    {
        _isDisguising = isDisguising;
        RefreshButtonState();
    }

    private void RefreshButtonState()
    {
        if (disguiseButton == null)
        {
            return;
        }

        disguiseButton.interactable = !_isDisguising && _charges > 0;
    }
}
