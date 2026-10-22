using System;
using TMPro;
using UnityEngine;

public sealed class UIUpdraftTimingView : MonoBehaviour
{
    [Space(8)]
    [Header("UI参照")] [Tooltip("ゲージ全体の表示・非表示を切り替えるGameObject。")] [SerializeField] 
    private GameObject content;

    [Tooltip("タイミングの進行度をfillAmountで表示するImage。")] [SerializeField] 
    private UnityEngine.UI.Image fill;

    [Tooltip("ゲージ内を上下に移動するマーカーのRectTransform。")] [SerializeField] 
    private RectTransform marker;

    [Tooltip("状態に応じて色を変更するマーカーのImage。")] [SerializeField] 
    private UnityEngine.UI.Image markerImage;

    [Tooltip("READYING・JUMP!・SPACEを表示するテキスト。")] [SerializeField] 
    private TMP_Text statusLabel;

    [Space(8)] [Header("表示色")] [Tooltip("ジャンプ可能で、最適タイミング以外のときの色。")] [SerializeField] 
    private Color normalColor = new Color(0.28f, 0.8f, 1f);

    [Tooltip("最適なジャンプタイミングの色。")] [SerializeField] 
    private Color optimalColor = new Color(0.92f, 1f, 0.35f);

    [Tooltip("ジャンプできないときの色。")] [SerializeField] 
    private Color unavailableColor = new Color(0.5f, 0.56f, 0.62f);

    private void Awake()
    {
        if (content == null || fill == null || marker == null ||
            markerImage == null || statusLabel == null)
            throw new InvalidOperationException("UpdraftTimingView のUI参照を設定してください。");

        content.SetActive(false);
    }

    public void Render(UpdraftTimingPresentation presentation)
    {
        if (content.activeSelf != presentation.Visible)
            content.SetActive(presentation.Visible);

        if (!presentation.Visible)
            return;

        fill.fillAmount = presentation.Fill;
        marker.anchorMin = new Vector2(0.5f, presentation.Fill);
        marker.anchorMax = marker.anchorMin;
        marker.anchoredPosition = Vector2.zero;

        Color color = !presentation.Ready ? unavailableColor :
            presentation.Optimal ? optimalColor : normalColor;
        fill.color = color;
        markerImage.color = color;
        statusLabel.text = !presentation.Ready ? "READYING" :
            presentation.Optimal ? "JUMP!" : "SPACE";
        statusLabel.color = color;
    }
}