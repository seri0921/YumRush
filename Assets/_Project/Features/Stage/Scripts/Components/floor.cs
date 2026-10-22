using UnityEngine;

[ExecuteAlways]
public class PlatformController : MonoBehaviour
{
    [Header("足場のサイズ設定")]
    [Tooltip("中間部分の幅")]
    [Min(0.1f)] // マイナスにならないように制限
    public float middleWidth = 3f;

    [Tooltip("両端パーツ1つ分の幅（画像サイズに合わせて設定してください）")]
    public float edgeWidth = 1f;

    [Header("各コンポーネントの参照")]
    public SpriteRenderer middleSprite;
    public Transform leftEdge;
    public Transform rightEdge;
    public BoxCollider2D mainCollider;

    // インスペクターで値が変更された時に呼ばれる
    private void OnValidate()
    {
        UpdatePlatform();
    }

    // ゲーム開始時やPrefab生成時に呼ばれる
    private void Start()
    {
        UpdatePlatform();
    }

    // 足場のサイズと配置を自動計算する処理
    public void UpdatePlatform()
    {
        // 参照がセットされていない場合はエラーを防ぐため処理を抜ける
        if (middleSprite == null || leftEdge == null || rightEdge == null || mainCollider == null)
            return;

        // 1. 中間画像の幅を変更
        // ※middleSpriteのDraw Modeが「Tiled」になっている必要があります
        Vector2 middleSize = middleSprite.size;
        middleSize.x = middleWidth;
        middleSprite.size = middleSize;

        // 2. 両端パーツのローカル座標（位置）を計算して移動
        // 中心(0)から「中間の半分の幅」＋「端の半分の幅」だけ移動させた位置が正しい端の位置
        float offset = (middleWidth / 2f) + (edgeWidth / 2f);

        leftEdge.localPosition = new Vector3(-offset, 0f, 0f);
        rightEdge.localPosition = new Vector3(offset, 0f, 0f);

        // 3. 親オブジェクトの物理コライダー（全体の当たり判定）の幅を更新
        // 全体の幅 ＝ 中間の幅 ＋ (両端の幅 × 2)
        Vector2 colliderSize = mainCollider.size;
        colliderSize.x = middleWidth + (edgeWidth * 2f);
        mainCollider.size = colliderSize;
    }
}
