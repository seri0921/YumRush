using UnityEngine;
using UnityEngine.UI;
public class KeyIndicatorUI : MonoBehaviour
{
    [SerializeField] private RectTransform _arrowRect;

    [Header("スプライトの本来の向き")]
    [SerializeField] private SpriteFacingDirection _spriteFacing = SpriteFacingDirection.Right;

    public enum SpriteFacingDirection
    {
        Up, 
        Right 
    }

    private RectTransform _rectTransform;

    private void Awake()
    {
        _rectTransform = GetComponent<RectTransform>();
        if (_arrowRect == null) _arrowRect = _rectTransform;
    }

    public void UpdatePositionAndRotation(Vector2 canvasLocalPosition, Vector2 direction, bool isOffScreen)
    {
        gameObject.SetActive(isOffScreen);
        if (!isOffScreen) return;

        _rectTransform.anchoredPosition = canvasLocalPosition;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        float offset = (_spriteFacing == SpriteFacingDirection.Up) ? -90f : 0f;

        _arrowRect.localRotation = Quaternion.Euler(0f, 0f, angle + offset);
    }

}
