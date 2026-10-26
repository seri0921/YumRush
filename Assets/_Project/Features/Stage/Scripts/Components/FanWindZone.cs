using UnityEngine;

public class FanWindZone : MonoBehaviour, IFanField
{
    [Header("風の設定")]
    [Tooltip("風の吹き出す方向（ローカル空間の向きを使う場合は Check）")]
    [SerializeField] private bool useLocalUpDirection = true;

    [Tooltip("useLocalUpDirection が false の場合に使われる固定方向")]
    [SerializeField] private Vector2 customDirection = Vector2.up;

    [Tooltip("風の強さ")]
    [SerializeField] private float windStrength = 15f;

    [Tooltip("上昇・下降風の補正倍率")]
    [SerializeField] private float updraftMultiplier = 1f;

    [Tooltip("重ね合わせ時の優先度（ギミックは高めに設定）")]
    [SerializeField] private int priority = 10;

    public Vector2 WindVector
    {
        get
        {
            Vector2 dir = useLocalUpDirection ? (Vector2)transform.up : customDirection;
            return dir.normalized * windStrength;
        }
    }

    public float UpdraftMultiplier => updraftMultiplier;
    public int Priority => priority;

    private void Reset()
    {
        var col = GetComponent<Collider2D>();
        if (col != null) col.isTrigger = true;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawRay(transform.position, WindVector.normalized * 2f);
    }
}
