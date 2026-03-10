using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class JoystickUI : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
{
    public static JoystickUI ActiveJoystick { get; private set; }

    public RectTransform background;
    public RectTransform handle;
    public float maxRadius = 80f;

    public Vector2 Output { get; private set; }

    private Canvas _canvas;

    private void Awake()
    {
        _canvas = GetComponentInParent<Canvas>();
        if (ActiveJoystick == null)
        {
            ActiveJoystick = this;
        }
    }

    private void OnDestroy()
    {
        if (ActiveJoystick == this)
        {
            ActiveJoystick = null;
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        UpdateHandle(eventData);
    }

    public void OnDrag(PointerEventData eventData)
    {
        UpdateHandle(eventData);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        Output = Vector2.zero;
        if (handle != null)
        {
            handle.anchoredPosition = Vector2.zero;
        }
    }

    private void UpdateHandle(PointerEventData eventData)
    {
        if (background == null || handle == null)
        {
            return;
        }

        if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(
                background,
                eventData.position,
                _canvas != null && _canvas.renderMode != RenderMode.ScreenSpaceOverlay ? _canvas.worldCamera : null,
                out var localPoint))
        {
            return;
        }

        var clamped = Vector2.ClampMagnitude(localPoint, maxRadius);
        handle.anchoredPosition = clamped;
        Output = clamped / maxRadius;
    }
}
