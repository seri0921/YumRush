using UnityEngine;

public class FanWindBiome : MonoBehaviour, IFanField
{
    [Header("周期パラメータ")]
    [Tooltip("風が吹いていない待ち時間（秒）")]
    [SerializeField] private float coolTime = 3f;

    [Tooltip("1回の風が吹く持続時間（秒）")]
    [SerializeField] private float windDuration = 3f;

    [Tooltip("風の基本強度")]
    [SerializeField] private float windStrength = 8.5f;

    [Tooltip("上昇・下降風の補正倍率")]
    [SerializeField] private float updraftMultiplier = 1f;

    [Tooltip("優先度（バイオームエリアのため基本は 0）")]
    [SerializeField] private int priority = 0;

    // 風の向きのシーケンス: 右 (1,0) -> 上 (0,1) -> 左 (-1,0) -> 下 (0,-1)
    private static readonly Vector2[] DirectionSequence = new Vector2[]
    {
        new Vector2(1f, 0f),  // 右
        new Vector2(0f, 1f),  // 上
        new Vector2(-1f, 0f), // 左
        new Vector2(0f, -1f)  // 下
    };

    private int directionIndex = 0;
    private float timer = 0f;
    private bool isBlowing = false; // 現在風が吹いているかどうか
    private int playerInsideCount = 0; // エリア内にいるプレイヤー数

    public Vector2 WindVector
    {
        get
        {
            // クールタイム中（風が吹いていない時）はゼロベクトルを返す
            if (!isBlowing)
                return Vector2.zero;

            return DirectionSequence[directionIndex] * windStrength;
        }
    }

    public float UpdraftMultiplier => updraftMultiplier;
    public int Priority => priority;

    private void Reset()
    {
        var col = GetComponent<Collider2D>();
        if (col != null)
        {
            col.isTrigger = true;
        }
    }

    private void Update()
    {
        // エリア内にプレイヤーがいない場合はタイマーを進めない
        if (playerInsideCount <= 0 || Time.timeScale == 0f)
            return;

        timer += Time.deltaTime;

        if (isBlowing)
        {
            // 風が吹いている時間の消化判定
            if (timer >= windDuration)
            {
                timer = 0f;
                Debug.Log("風やむよ");
                isBlowing = false; // クールタイムに入る

                // 次の風の向きへ更新（0 -> 1 -> 2 -> 3 -> 0 ...）
                directionIndex = (directionIndex + 1) % DirectionSequence.Length;
            }
        }
        else
        {
            // クールタイムの消化判定
            if (timer >= coolTime)
            {
                timer = 0f;
                Debug.Log("風吹いてるよ");
                isBlowing = true; // 風を吹き始める
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (IsPlayer(other))
        {
            playerInsideCount++;
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (IsPlayer(other))
        {
            playerInsideCount = Mathf.Max(0, playerInsideCount - 1);

            // プレイヤーが全員出たら状態を初期化する場合（お好みで調整可能）
            if (playerInsideCount == 0)
            {
                ResetCycle();
            }
        }
    }

    private bool IsPlayer(Collider2D col)
    {
        // Playerタグ判定（必要に応じてレイヤーやコンポーネント判定に変更してください）
        return col.CompareTag("Player");
    }

    /// <summary>
    /// エリアから誰もいなくなった際にサイクルを初期状態にリセットする
    /// </summary>
    public void ResetCycle()
    {
        timer = 0f;
        isBlowing = false;
        directionIndex = 0;
    }

    private void OnDrawGizmosSelected()
    {
        // Sceneビューで現在の向きと状態をデバッグ表示
        Gizmos.color = isBlowing ? Color.green : Color.gray;
        Vector2 currentDir = DirectionSequence[directionIndex];
        Gizmos.DrawRay(transform.position, currentDir * 2f);
    }
}
