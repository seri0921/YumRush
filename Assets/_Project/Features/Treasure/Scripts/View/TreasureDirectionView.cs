using System;
using UnityEngine;

namespace Project.Treasure
{
    public sealed class TreasureDirectionView : MonoBehaviour
    {
        [SerializeField] private Canvas _canvas;
        [SerializeField] private UnityEngine.UI.Image _arrow;
        [SerializeField] private UnityEngine.UI.Image _icon;
        [SerializeField] private Sprite _keyArrowSprite;
        [SerializeField] private Sprite _keyIconSprite;
        [SerializeField] private Sprite _chestArrowSprite;
        [SerializeField] private Sprite _chestIconSprite;
        [SerializeField, Min(1)] private float _arrowSize = 48;
        [SerializeField, Min(0)] private float _edgePadding = 24;
        [SerializeField] private Color _arrowColor = Color.white;

        // Reserve the farthest corner from the off-center pivot at every rotation.
        public float InsetPixels => (_edgePadding + _arrowSize *
            Mathf.Max(GetRadiusRatio(_keyArrowSprite), GetRadiusRatio(_chestArrowSprite))) * _canvas.scaleFactor;

        private static float GetRadiusRatio(Sprite sprite)
        {
            Vector2 size = sprite.rect.size;
            Vector2 pivot = sprite.pivot;
            return new Vector2(Mathf.Max(pivot.x, size.x - pivot.x),
                Mathf.Max(pivot.y, size.y - pivot.y)).magnitude / size.x;
        }

        private void Awake()
        {
            if (_canvas == null || _arrow == null || _icon == null || _keyArrowSprite == null ||
                _keyIconSprite == null || _chestArrowSprite == null || _chestIconSprite == null)
                throw new InvalidOperationException("Treasure direction UI requires a Canvas, arrow and icon Images, and all four sprites.");
            Hide();
        }

        public void Render(Vector2 screenPosition, float angle, bool showChest)
        {
            var parent = (RectTransform)_arrow.rectTransform.parent;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(parent, screenPosition, null, out var localPosition);
            _arrow.rectTransform.anchoredPosition = localPosition;
            _arrow.sprite = showChest ? _chestArrowSprite : _keyArrowSprite;
            _icon.sprite = showChest ? _chestIconSprite : _keyIconSprite;
            // Both sprites use the same crop and pivot at the circular frame's center.
            Vector2 size = new Vector2(_arrowSize, _arrowSize * _arrow.sprite.rect.height / _arrow.sprite.rect.width);
            Vector2 pivot = _arrow.sprite.pivot / _arrow.sprite.rect.size;
            _arrow.rectTransform.pivot = pivot;
            _arrow.rectTransform.sizeDelta = size;
            _icon.rectTransform.pivot = pivot;
            _icon.rectTransform.anchoredPosition = localPosition;
            _icon.rectTransform.sizeDelta = size;
            _icon.rectTransform.localRotation = Quaternion.identity;
            _icon.color = _arrowColor;
            _icon.enabled = true;
            _arrow.rectTransform.localRotation = Quaternion.Euler(0, 0, angle);
            _arrow.color = _arrowColor;
            _arrow.enabled = true;
        }

        public void Hide()
        {
            _arrow.enabled = false;
            _icon.enabled = false;
        }
    }
}
