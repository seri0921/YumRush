using System;
using UnityEngine;

// 共有する調整値のみを保持する。実行中の状態は各プレイヤーが管理する。
[CreateAssetMenu(fileName = "PlayerMovementConfig", menuName = "ScriptableObject/Parameter/Player Config")]
public sealed class PlayerMovementConfig : ScriptableObject
{
    [Space(8)] [Header("地上移動・ジャンプ")] [Tooltip("地上での目標水平速度（unit/秒）。")] [SerializeField, Min(0.1f)]
    private float moveSpeed = 10f;

    [Tooltip("地上ジャンプと気流ジャンプの基本上向き速度（unit/秒）。")] [SerializeField, Min(0.1f)]
    private float jumpSpeed = 25f;

    [Space(8)] [Header("接地判定")] [Tooltip("接地判定に含めるレイヤー。Triggerは除外します。")] [SerializeField]
    private LayerMask groundLayers = ~0;

    [Tooltip("足元へColliderを検査する距離（unit）。")] [SerializeField, Min(0.01f)]
    private float groundProbeDistance = 0.08f;

    [Space(8)] [Header("気流との相互作用")] [Tooltip("気流ジャンプ後、下降時に再浮遊できる気流目標位置との高さ差（unit）。")] [SerializeField, Min(0f)]
    private float recaptureHeight = 0.8f;

    [Tooltip("気流の上下運動へ追従する最大速度（unit/秒）。")] [SerializeField, Min(0.1f)]
    private float windMaxSpeed = 10f;

    [Tooltip("気流の上下運動へ追従する加速度（unit/秒²）。")] [SerializeField, Min(0.1f)]
    private float windAcceleration = 14f;

    [Tooltip("気流へ入った瞬間に許容する最大落下速度（unit/秒）。")] [SerializeField, Min(0.1f)]
    private float entryMaxFallSpeed = 4f;

    [Space(8)] [Header("方向性風（FanWind）")]
    
    [Tooltip("風に乗った時の最大水平速度。")]
    
    [SerializeField, Min(0.1f)] private float fanWindMaxSpeed = 25f;

    [Tooltip("風の中での加速度。")]
    [SerializeField, Min(0.1f)] private float fanWindAcceleration = 30f;

    [Tooltip("逆風時の抵抗倍率。")]
    [SerializeField, Min(0.1f)] private float fanWindResistance = 2.5f;

    [Space(8)] [Header("通常空中移動")] [Tooltip("空中で目標水平速度へ近づける加速度（unit/秒²）。")] [SerializeField, Min(0.1f)]
    private float airAcceleration = 14f;

    [Tooltip("空中で左右入力したときの目標水平速度（unit/秒）。")] [SerializeField, Min(0.1f)]
    private float airMaximumSpeed = 8f;

    [Tooltip("空中で左右入力を離したときの追加減衰係数（1/秒）。大きいほど減速します。")] [SerializeField, Min(0f)]
    private float airDamping = 0.5f;

    [Space(8)] [Header("滑空")] [Tooltip("滑空中の合成速度の上限（unit/秒）。")] [SerializeField, Min(0.1f)]
    private float glideMaximumSpeed = 32f;

    [Tooltip("滑空中の揚力係数。大きいほど速度に応じた揚力が強くなります。")] [SerializeField, Min(0f)]
    private float glideLiftCoefficient = 0.2f;

    [Tooltip("滑空中の抗力係数。大きいほど速度を失いやすくなります。")] [SerializeField, Min(0f)]
    private float glideDragCoefficient = 0.003f;

    [Tooltip("揚力が発生し始める合成速度（unit/秒）。")] [SerializeField, Min(0f)]
    private float glideMinimumLiftSpeed = 1f;

    [Tooltip("降下時の引き起こし補助が強まり始める速度（unit/秒）。最低揚力速度以上に設定します。")] [SerializeField, Min(0f)]
    private float glidePullupStartSpeed = 15f;

    [Tooltip("高速降下時の引き起こし補助の強さ。")] [SerializeField, Min(0f)]
    private float glidePullupStrength = 1.3f;

    [Tooltip("滑空中の左右操舵の強さ（unit/秒²）。")] [SerializeField, Min(0f)]
    private float glideSteerAcceleration = 18f;

    [Tooltip("逆方向へ操舵するときの速度減衰係数。大きいほど旋回で減速します。")] [SerializeField, Min(0f)]
    private float glideTurnLoss = 5f;

    [Space(8)] [Header("空中ダッシュ")] [Tooltip("ダッシュ中の移動速度（unit/秒）。")] [SerializeField, Min(0.1f)]
    private float airDashSpeed = 25f;

    [Tooltip("ダッシュの継続時間（秒）。")] [SerializeField, Min(0.01f)]
    private float airDashDuration = 0.18f;

    [Space(8)] [Header("敵弾による被弾")] [Tooltip("被弾時に設定する下向き速度（unit/秒）。")] [SerializeField, Min(0.1f)]
    private float enemyKnockbackSpeed = 12f;

    [Tooltip("被弾後に操作を受け付けない時間（秒）。")] [SerializeField, Min(0.01f)]
    private float enemyControlLockDuration = 0.35f;

    [Tooltip("被弾後に次の敵弾を無効にする時間（秒）。")] [SerializeField, Min(0.01f)]
    private float enemyInvulnerabilityDuration = 1f;

    [Space(8)] [Header("対象検出")] [Tooltip("追尾攻撃の対象を探す半径（unit）。")] [SerializeField, Min(0.01f)]
    private float attackRange = 7f;

    [Space(8)] [Header("追尾移動")] [Tooltip("対象へ向かう移動速度（unit/秒）。")] [SerializeField, Min(0.01f)]
    private float homingSpeed = 25f;

    [Tooltip("追尾移動の制限時間（秒）。準備演出の時間は含みません。")] [SerializeField, Min(0.01f)]
    private float homingMaximumDuration = 1f;

    [Tooltip("命中とみなすCollider間の距離（unit）。重なった場合も命中します。")] [SerializeField, Min(0f)]
    private float contactDistance = 0.15f;

    [Tooltip("攻撃成功後の上向き速度（unit/秒）。")] [SerializeField, Min(0f)]
    private float successUpwardSpeed = 40f;

    [Space(8)] [Header("対象フィルター")] [Tooltip("追尾対象の検索に含めるレイヤー。")] [SerializeField]
    private LayerMask targetLayers = ~0;

    public float MoveSpeed => moveSpeed;
    public float JumpSpeed => jumpSpeed;
    public LayerMask GroundLayers => groundLayers;
    public float GroundProbeDistance => groundProbeDistance;
    public float RecaptureHeight => recaptureHeight;
    public float WindMaxSpeed => windMaxSpeed;
    public float WindAcceleration => windAcceleration;
    public float FanWindMaxSpeed => fanWindMaxSpeed;
    public float FanWindAcceleration => fanWindAcceleration;
    public float FanWindResistance => fanWindResistance;
    public float EntryMaxFallSpeed => entryMaxFallSpeed;
    public float AirAcceleration => airAcceleration;
    public float AirMaximumSpeed => airMaximumSpeed;
    public float AirDamping => airDamping;
    public float GlideMaximumSpeed => glideMaximumSpeed;
    public float GlideLiftCoefficient => glideLiftCoefficient;
    public float GlideDragCoefficient => glideDragCoefficient;
    public float GlideMinimumLiftSpeed => glideMinimumLiftSpeed;
    public float GlidePullupStartSpeed => glidePullupStartSpeed;
    public float GlidePullupStrength => glidePullupStrength;
    public float GlideSteerAcceleration => glideSteerAcceleration;
    public float GlideTurnLoss => glideTurnLoss;
    public float AirDashSpeed => airDashSpeed;
    public float AirDashDuration => airDashDuration;
    public float EnemyKnockbackSpeed => enemyKnockbackSpeed;
    public float EnemyControlLockDuration => enemyControlLockDuration;
    public float EnemyInvulnerabilityDuration => enemyInvulnerabilityDuration;
    public float AttackRange => attackRange;
    public float HomingSpeed => homingSpeed;
    public float HomingMaximumDuration => homingMaximumDuration;
    public float ContactDistance => contactDistance;
    public float SuccessUpwardSpeed => successUpwardSpeed;
    public LayerMask TargetLayers => targetLayers;

    public void Validate()
    {
        ValidateNumber(moveSpeed, nameof(moveSpeed), true);
        ValidateNumber(jumpSpeed, nameof(jumpSpeed), true);
        ValidateNumber(groundProbeDistance, nameof(groundProbeDistance), true);
        ValidateNumber(recaptureHeight, nameof(recaptureHeight), false);
        ValidateNumber(windMaxSpeed, nameof(windMaxSpeed), true);
        ValidateNumber(windAcceleration, nameof(windAcceleration), true);
        ValidateNumber(entryMaxFallSpeed, nameof(entryMaxFallSpeed), true);
        ValidateNumber(airAcceleration, nameof(airAcceleration), true);
        ValidateNumber(airMaximumSpeed, nameof(airMaximumSpeed), true);
        ValidateNumber(airDamping, nameof(airDamping), false);
        ValidateNumber(glideMaximumSpeed, nameof(glideMaximumSpeed), true);
        ValidateNumber(glideLiftCoefficient, nameof(glideLiftCoefficient), false);
        ValidateNumber(glideDragCoefficient, nameof(glideDragCoefficient), false);
        ValidateNumber(glideMinimumLiftSpeed, nameof(glideMinimumLiftSpeed), false);
        ValidateNumber(glidePullupStartSpeed, nameof(glidePullupStartSpeed), false);
        ValidateNumber(glidePullupStrength, nameof(glidePullupStrength), false);
        ValidateNumber(glideSteerAcceleration, nameof(glideSteerAcceleration), false);
        ValidateNumber(glideTurnLoss, nameof(glideTurnLoss), false);
        ValidateNumber(airDashSpeed, nameof(airDashSpeed), true);
        ValidateNumber(airDashDuration, nameof(airDashDuration), true);
        ValidateNumber(enemyKnockbackSpeed, nameof(enemyKnockbackSpeed), true);
        ValidateNumber(enemyControlLockDuration, nameof(enemyControlLockDuration), true);
        ValidateNumber(enemyInvulnerabilityDuration, nameof(enemyInvulnerabilityDuration), true);
        ValidateNumber(attackRange, nameof(attackRange), true);
        ValidateNumber(homingSpeed, nameof(homingSpeed), true);
        ValidateNumber(homingMaximumDuration, nameof(homingMaximumDuration), true);
        ValidateNumber(contactDistance, nameof(contactDistance), false);
        ValidateNumber(successUpwardSpeed, nameof(successUpwardSpeed), false);
        
        if (glidePullupStartSpeed < glideMinimumLiftSpeed)
            throw new InvalidOperationException("PlayerConfig: 引き起こし開始速度は最低揚力速度以上にしてください。");
    }

    private static void ValidateNumber(float value, string fieldName, bool requiresPositive)
    {
        if (float.IsNaN(value) || float.IsInfinity(value) ||
            (requiresPositive ? value <= 0f : value < 0f))
            throw new InvalidOperationException($"PlayerConfig.{fieldName} の設定が不正です。");
    }
}
