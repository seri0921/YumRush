using System;
using UnityEngine;

namespace Project.Treasure
{
    public sealed class TreasureDirectionView : MonoBehaviour
    {
        [SerializeField] private Canvas _canvas;
        [SerializeField] private UnityEngine.UI.Image _arrow;
        [SerializeField, Min(1)] private float _arrowSize = 48;
        [SerializeField, Min(0)] private float _edgePadding = 24;
        [SerializeField] private Color _arrowColor = Color.white;

        // Reserve the half diagonal so a rotated arrow also remains inside the screen.
        public float InsetPixels => (_edgePadding + _arrowSize * 0.707107f) * _canvas.scaleFactor;

        private void Awake()
        {
            if (_canvas == null || _arrow == null || _arrow.sprite == null)
                throw new InvalidOperationException("Treasure direction UI requires a Canvas, Image and arrow sprite.");
            Hide();
        }

        public void Render(Vector2 screenPosition, float angle)
        {
            var parent = (RectTransform)_arrow.rectTransform.parent;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(parent, screenPosition, null, out var localPosition);
            _arrow.rectTransform.anchoredPosition = localPosition;
            _arrow.rectTransform.sizeDelta = Vector2.one * _arrowSize;
            _arrow.rectTransform.localRotation = Quaternion.Euler(0, 0, angle);
            _arrow.color = _arrowColor;
            _arrow.enabled = true;
        }

        public void Hide() => _arrow.enabled = false;
    }
}
