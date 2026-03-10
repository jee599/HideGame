using UnityEngine;
using UnityEngine.UI;

public class MinimapUI : MonoBehaviour
{
    public RectTransform minimapRoot;
    public Vector2 collapsedSize = new Vector2(180f, 180f);
    public Vector2 expandedSize = new Vector2(320f, 320f);
    public Button toggleButton;

    private bool _expanded;

    private void Start()
    {
        if (toggleButton != null)
        {
            toggleButton.onClick.AddListener(Toggle);
        }

        ApplySize();
    }

    private void OnDestroy()
    {
        if (toggleButton != null)
        {
            toggleButton.onClick.RemoveListener(Toggle);
        }
    }

    public void Toggle()
    {
        _expanded = !_expanded;
        ApplySize();
    }

    private void ApplySize()
    {
        if (minimapRoot != null)
        {
            minimapRoot.sizeDelta = _expanded ? expandedSize : collapsedSize;
        }
    }
}
