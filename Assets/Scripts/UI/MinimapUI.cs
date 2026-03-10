using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MinimapUI : MonoBehaviour
{
    public RectTransform minimapRoot;
    public RectTransform mapFrame;
    public Vector2 collapsedSize = new Vector2(180f, 180f);
    public Vector2 expandedSize = new Vector2(320f, 320f);
    public Vector2 collapsedMapSize = new Vector2(300f, 144f);
    public Vector2 expandedMapSize = new Vector2(420f, 220f);
    public Button toggleButton;
    public RawImage mapImage;
    public TextMeshProUGUI stateLabel;
    public TextMeshProUGUI toggleLabel;

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

        if (mapFrame != null)
        {
            mapFrame.sizeDelta = _expanded ? expandedMapSize : collapsedMapSize;
        }

        if (stateLabel != null)
        {
            stateLabel.text = _expanded ? "MINIMAP  TAP TO SHRINK" : "MINIMAP";
        }

        if (toggleLabel != null)
        {
            toggleLabel.text = _expanded ? "SHRINK" : "EXPAND";
        }

        if (mapImage != null)
        {
            mapImage.color = Color.white;
        }
    }
}
