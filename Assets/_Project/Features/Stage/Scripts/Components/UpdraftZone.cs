using UnityEngine;

public sealed class UpdraftZone : MonoBehaviour
{
    [Space(8)]
    [Header("気流の上下運動")]
    [Tooltip("上下運動が1周する時間（秒）。")]
    [SerializeField, Min(0.1f)] private float period = 2f;
    [Tooltip("上下運動の中心からの最大変位（unit）。")]
    [SerializeField, Min(0.05f)] private float amplitude = 0.65f;
    [Tooltip("ジャンプ速度へ加算する上昇速度の倍率。")]
    [SerializeField, Min(0f)] private float boostMultiplier = 6f;
    [Space(8)]
    [Header("気流への捕捉")]
    [Tooltip("気流の中心へ引き寄せる最大速度（unit/秒）。")]
    [SerializeField, Min(0.1f)] private float captureSpeed = 10f;
    [Tooltip("中心へ引き寄せる加速度（unit/秒²）。")]
    [SerializeField, Min(0.1f)] private float captureAcceleration = 60f;
    [Tooltip("中心に到達したとみなす距離（unit）。減速条件も満たす必要があります。")]
    [SerializeField, Min(0.01f)] private float captureArrivalDistance = 0.15f;

    [Header("上昇気流の判定")]
    [SerializeField] private BoxCollider2D area;

    public float Period => period;
    public float Amplitude => amplitude;
    public float BoostMultiplier => boostMultiplier;
    public float CaptureSpeed => captureSpeed;
    public float CaptureAcceleration => captureAcceleration;
    public float CaptureArrivalDistance => captureArrivalDistance;
    public float CenterX => area.bounds.center.x;
    public float CenterY => area.bounds.center.y;

    private void Awake()
    {
        if (!area.isTrigger || period <= 0f || amplitude <= 0f || boostMultiplier < 0f ||
            captureSpeed <= 0f || captureAcceleration <= 0f || captureArrivalDistance <= 0f)
            throw new System.InvalidOperationException("UpdraftZone の設定が不正です。");
    }
}
