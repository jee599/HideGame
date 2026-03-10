using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SuspicionMeterUI : MonoBehaviour
{
    public SuspicionSystem suspicionSystem;
    public Image fillImage;
    public TextMeshProUGUI valueLabel;
    public Gradient fillGradient;

    private void Start()
    {
        if (suspicionSystem == null)
        {
            suspicionSystem = FindFirstObjectByType<SuspicionSystem>();
        }

        if (suspicionSystem != null)
        {
            suspicionSystem.SuspicionChanged += UpdateView;
            UpdateView(suspicionSystem.suspicion);
        }
    }

    private void OnDestroy()
    {
        if (suspicionSystem != null)
        {
            suspicionSystem.SuspicionChanged -= UpdateView;
        }
    }

    private void UpdateView(float suspicion)
    {
        var normalized = suspicion / 100f;
        if (fillImage != null)
        {
            fillImage.fillAmount = normalized;
            if (fillGradient != null)
            {
                fillImage.color = fillGradient.Evaluate(normalized);
            }
        }

        if (valueLabel != null)
        {
            valueLabel.text = $"Suspicion {Mathf.RoundToInt(suspicion)}";
        }
    }
}
