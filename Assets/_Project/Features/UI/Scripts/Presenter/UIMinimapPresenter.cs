using System;
using UnityEngine;

public sealed class UIMinimapPresenter : MonoBehaviour
{
    [Header("参照")]
    [Tooltip("位置を表示するプレイヤー。")]
    [SerializeField] private Transform player;
    [SerializeField] private UIMinimapView view;

    [Header("地図に対応するワールド範囲")]
    [Tooltip("X・Yは左下の座標、Width・Heightは地図全体の幅と高さ。実行中も変更できます。")]
    [SerializeField] private Rect stageBounds = new Rect(-115.85f, -35.87f, 180f, 120f);

    private readonly UIMinimapLogic logic = new UIMinimapLogic();

    private void Awake()
    {
        if (player == null || view == null)
            throw new InvalidOperationException("Minimap のPlayerとViewを設定してください。");
        UIMinimapLogic.ValidateBounds(stageBounds.x, stageBounds.y, stageBounds.width, stageBounds.height);
    }

    private void LateUpdate()
    {
        Vector3 position = player.position;
        view.Render(logic.Evaluate(position.x, position.y,
            stageBounds.x, stageBounds.y, stageBounds.width, stageBounds.height));
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireCube(new Vector3(stageBounds.center.x, stageBounds.center.y, 0f),
            new Vector3(stageBounds.width, stageBounds.height, 0f));
    }
}
