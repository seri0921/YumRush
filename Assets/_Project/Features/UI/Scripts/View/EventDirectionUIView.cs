using UnityEngine;

[DisallowMultipleComponent]
public sealed class EventDirectionUIView : MonoBehaviour
{
    [SerializeField] private Sprite _arrowSprite;
    [SerializeField] private Sprite _bonusSprite;
    [SerializeField] private Sprite _bossSprite;
    [SerializeField] private Sprite _itemRainSprite;
    [SerializeField] private Color _arrowColor = Color.white;
    [SerializeField] private Color _eventColor = Color.white;
    [SerializeField, Min(1f)] private float _arrowSize = 32f;
    [SerializeField, Min(1f)] private float _eventSize = 42f;
    [SerializeField, Min(0f)] private float _iconGap = 3f;
    [SerializeField, Min(0f)] private float _distance = 72f;
    [SerializeField, Min(0.1f)] private float _blinkPeriod = 0.7f;

    private Canvas _canvas;
    private CanvasGroup _marker;
    private UnityEngine.UI.Image _arrow;
    private UnityEngine.UI.Image _icon;

    public void SetEvent(StageEventType type)
    {
        if (_canvas == null) CreateMarker();

        _icon.sprite = type switch
        {
            StageEventType.BonusTime => _bonusSprite,
            StageEventType.Boss => _bossSprite,
            StageEventType.ItemRain => _itemRainSprite,
            _ => throw new System.ArgumentOutOfRangeException(nameof(type), type, null)
        };
    }

    public void Render(Vector2 player, Vector2 direction, float elapsed)
    {
        // Scale from a 1080-pixel reference height, keeping movement in screen coordinates.
        float scale = _canvas.pixelRect.height / 1080f;
        float spacing = (_eventSize + _arrowSize) * 0.5f + _iconGap;
        _icon.rectTransform.position = player + direction * (_distance * scale);
        _arrow.rectTransform.position = player + direction * ((_distance + spacing) * scale);
        _icon.rectTransform.sizeDelta = Vector2.one * (_eventSize * scale);
        _arrow.rectTransform.sizeDelta = Vector2.one * (_arrowSize * scale);
        _arrow.rectTransform.localRotation = Quaternion.Euler(0f, 0f, Vector2.SignedAngle(Vector2.up, direction));
        _arrow.sprite = _arrowSprite;
        _arrow.color = _arrowColor;
        _icon.color = _eventColor;
        _marker.alpha = Mathf.Repeat(elapsed, _blinkPeriod) < _blinkPeriod * 0.5f ? 1f : 0f;
    }

    public void Hide()
    {
        if (_marker != null) _marker.alpha = 0f;
    }

    private void CreateMarker()
    {
        var overlay = new GameObject("Event Direction Overlay", typeof(RectTransform), typeof(Canvas));
        overlay.transform.SetParent(transform, false);
        _canvas = overlay.GetComponent<Canvas>();
        _canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        _canvas.sortingOrder = 20;

        var marker = new GameObject("Event Direction Marker", typeof(RectTransform), typeof(CanvasGroup));
        marker.transform.SetParent(overlay.transform, false);
        _marker = marker.GetComponent<CanvasGroup>();
        _marker.alpha = 0f;
        _marker.blocksRaycasts = false;
        _arrow = CreateImage("Arrow");
        _icon = CreateImage("Event Icon");
    }

    private UnityEngine.UI.Image CreateImage(string objectName)
    {
        var imageObject = new GameObject(objectName, typeof(RectTransform), typeof(UnityEngine.UI.Image));
        imageObject.transform.SetParent(_marker.transform, false);
        var image = imageObject.GetComponent<UnityEngine.UI.Image>();
        image.raycastTarget = false;
        image.preserveAspect = true;
        return image;
    }
}
