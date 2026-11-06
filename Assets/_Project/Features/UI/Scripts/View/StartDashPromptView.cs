using System;
using TMPro;
using UnityEngine;

public sealed class StartDashPromptView : MonoBehaviour
{
    [SerializeField] private TMP_Text _label;
    [SerializeField, TextArea] private string _message = "Press X or Y Button!";
    [Tooltip("Canvas座標でのプレイヤーからの表示位置補正。Yを小さくすると下に移動します。")]
    [SerializeField] private Vector2 _screenOffset = new Vector2(0f, -80f);

    private Canvas _canvas;
    private RectTransform _labelParent;

    private void Awake()
    {
        if (_label == null)
            throw new InvalidOperationException("Start dash prompt requires a text label.");

        _canvas = _label.GetComponentInParent<Canvas>();
        _labelParent = _label.rectTransform.parent as RectTransform;
        if (_canvas == null || _labelParent == null)
            throw new InvalidOperationException("Start dash prompt requires a Canvas and RectTransform parent.");

        Hide();
    }

    public void Render(Vector3 playerPosition, Camera worldCamera)
    {
        if (worldCamera == null)
        {
            Hide();
            return;
        }

        Vector3 screenPosition = worldCamera.WorldToScreenPoint(playerPosition);
        Camera uiCamera = _canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : _canvas.worldCamera;
        if (screenPosition.z <= 0f || !RectTransformUtility.ScreenPointToLocalPointInRectangle(
                _labelParent, screenPosition, uiCamera, out Vector2 localPosition))
        {
            Hide();
            return;
        }

        _label.rectTransform.localPosition = localPosition + _screenOffset;
        _label.text = _message;
        _label.enabled = true;
    }

    public void Hide() => _label.enabled = false;
}
