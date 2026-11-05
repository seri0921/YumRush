using System;
using TMPro;
using UnityEngine;

public sealed class UIUpdraftTimingView : MonoBehaviour
{
    [Header("UI参照")]
    [SerializeField] private GameObject content;
    [SerializeField] private RectTransform marker;
    [SerializeField] private UnityEngine.UI.Image markerImage;
    [SerializeField] private RectTransform referenceCircle;
    [SerializeField] private TMP_Text statusLabel;
    [Header("円の表示")]
    [SerializeField, Min(1f)] private float _maximumScale = 2.5f;
    [SerializeField, Min(0.01f)] private float _minimumScale = 1f;
    [Tooltip("毎秒の回転角度。正数で反時計回り、負数で時計回り。")]
    [SerializeField] private float _rotationSpeed = 90f;
    [Header("ジャスト成功演出")]
    [SerializeField] private ItemCollectEffectPlayback _successEffectPrefab;
    [SerializeField] private Color normalColor = new Color(0.28f, 0.8f, 1f);
    [SerializeField] private Color optimalColor = new Color(0.92f, 1f, 0.35f);
    [SerializeField] private Color unavailableColor = new Color(0.5f, 0.56f, 0.62f);
    private Canvas _canvas;
    private RectTransform _contentRect;
    private float _rotationAngle;

    private void Awake()
    {
        if (content == null || marker == null || markerImage == null ||
            referenceCircle == null || statusLabel == null || _successEffectPrefab == null)
            throw new InvalidOperationException("UpdraftTimingView のUI参照を設定してください。");
        _canvas = GetComponentInParent<Canvas>();
        _contentRect = (RectTransform)content.transform;
        content.SetActive(false);
    }

    public void PlaySuccess(Vector3 position)
    {
        Instantiate(_successEffectPrefab, position, Quaternion.identity);
    }

    public void Render(UpdraftTimingPresentation presentation, Vector2 screenCenter, float screenDiameter)
    {
        if (content.activeSelf != presentation.Visible)
            content.SetActive(presentation.Visible);
        if (!presentation.Visible)
            return;
        Camera uiCamera = _canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : _canvas.worldCamera;
        RectTransform parent = (RectTransform)_contentRect.parent;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(parent, screenCenter, uiCamera, out Vector2 localCenter);
        _contentRect.localPosition = new Vector3(localCenter.x, localCenter.y, 0f);
        _contentRect.localScale = Vector3.one;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(parent,
            screenCenter + Vector2.up * screenDiameter, uiCamera, out Vector2 localEdge);
        float diameter = Vector2.Distance(localCenter, localEdge);
        referenceCircle.sizeDelta = Vector2.one * diameter;
        marker.sizeDelta = Vector2.one * diameter * Mathf.Lerp(_maximumScale, _minimumScale, presentation.Fill);
        _rotationAngle = Mathf.Repeat(_rotationAngle + _rotationSpeed * Time.deltaTime, 360f);
        Quaternion rotation = Quaternion.Euler(0f, 0f, _rotationAngle);
        marker.localRotation = rotation;
        referenceCircle.localRotation = rotation;
        statusLabel.rectTransform.anchoredPosition = new Vector2(0f, -diameter * _maximumScale * 0.5f - 20f);
        Color color = !presentation.Ready ? unavailableColor :
            presentation.Optimal ? optimalColor : normalColor;
        markerImage.color = color;
        statusLabel.text = !presentation.Ready ? "READYING" : presentation.Optimal ? "JUMP!" : "SPACE";
        statusLabel.color = color;
    }
}
