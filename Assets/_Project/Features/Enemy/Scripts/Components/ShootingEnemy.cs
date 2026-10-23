using System;
using UnityEngine;

public sealed class ShootingEnemy : EnemyBase
{
    [Space(8)]
    [Header("参照")]
    [Tooltip("射撃対象のプレイヤー。未設定時はシーン内から取得します。")]
    [SerializeField]
    private PlayerPresenter target;

    [Tooltip("発射する弾のPrefab。UpdraftEnemyProjectileが必要です。")]
    [SerializeField]
    private EnemyBullet projectilePrefab;

    [Space(8)]
    [Header("射撃")]
    [Tooltip("プレイヤーを検知する距離（unit）。発射位置から測定します。")]
    [SerializeField, Min(0.01f)]
    private float detectionRange = 8f;

    [Tooltip("弾を発射する間隔（秒）。")]
    [SerializeField, Min(0.01f)]
    private float fireInterval = 1.5f;

    [Tooltip("発射した弾の移動速度（unit/秒）。")]
    [SerializeField, Min(0.01f)]
    private float projectileSpeed = 8f;

    [Tooltip("敵の位置に加算する発射位置のオフセット（ワールド座標、unit）。")]
    [SerializeField]
    private Vector2 muzzleOffset = Vector2.zero;

    private readonly EnemyFireLogic fireLogic = new EnemyFireLogic();


    protected override void Awake()
    {
        // Prefab単体を配置した場合は、シーン内のプレイヤーを一度だけ取得する。
        base.Awake();

        if (target == null)
            target = FindFirstObjectByType<PlayerPresenter>();

        if (target == null || projectilePrefab == null)
            throw new InvalidOperationException($"{gameObject.name}: PlayerとProjectile Prefabが必要です。");

        if (detectionRange <= 0f || fireInterval <= 0f || projectileSpeed <= 0f)
            throw new InvalidOperationException($"{gameObject.name}: 射撃設定は正の値にしてください。");
        if (!hitCollider.isTrigger)
            throw new InvalidOperationException("UpdraftEnemy: Collider2DをTriggerにしてください。");
    }

    protected override void UpdateBehavior()
    {
        if (IsDefeated)
            return;
        Vector2 origin = (Vector2)transform.position + muzzleOffset;
        Vector2 targetPosition = target.transform.position;
        if (!fireLogic.Tick(Time.deltaTime, (targetPosition - origin).sqrMagnitude,
                detectionRange, fireInterval))
            return;

        Vector2 displacement = targetPosition - origin;
        Vector2 direction = displacement.sqrMagnitude > 0f
            ? displacement.normalized
            : Vector2.down;
        EnemyBullet projectile = Instantiate(projectilePrefab, origin, Quaternion.identity);
        projectile.Launch(direction, projectileSpeed);
    }

    public override void ReceiveAttack()
    {
        if (!defeatLogic.TryDefeat())
            return;
        hitCollider.enabled = false;
        if (TryGetComponent<RespawnTarget>(out var respawnTarget))
            respawnTarget.NotifyConsumed();
        gameObject.SetActive(false);
        Destroy(gameObject);
    }
}

