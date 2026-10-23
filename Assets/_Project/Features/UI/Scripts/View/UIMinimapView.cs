using System;
using UnityEngine;

public sealed class UIMinimapView : MonoBehaviour
{
    [Tooltip("地図画像。子のMapImageにあるImageのSource Imageを差し替えてください。画像全体がステージ範囲に対応します。")]
    [SerializeField] private UnityEngine.UI.Image mapImage;
    [Tooltip("プレイヤー位置の点。MapImageの直接の子に配置してください。")]
    [SerializeField] private RectTransform marker;

    private void Awake()
    {
        if (mapImage == null || marker == null)
            throw new InvalidOperationException("Minimap のMap ImageとMarkerを設定してください。");
        if (marker.parent != mapImage.rectTransform)
            throw new InvalidOperationException("Minimap のMarkerはMapImageの直接の子にしてください。");
        if (mapImage.preserveAspect || mapImage.type != UnityEngine.UI.Image.Type.Simple)
            throw new InvalidOperationException("Minimap のMapImageはSimpleかつPreserve Aspectオフで使用してください。");
    }

    public void Render(MinimapPosition position)
    {
        Vector2 anchor = new Vector2(position.X, position.Y);
        marker.anchorMin = anchor;
        marker.anchorMax = anchor;
        marker.anchoredPosition = Vector2.zero;
    }
}
