using System;
using UnityEngine;

public sealed class ShootingEnemy : EnemyBase
{

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

    protected override void OnInitialize()
    {
        if (target == null || projectilePrefab == null)
            throw new InvalidOperationException($"{gameObject.name}: PlayerとProjectile Prefabが必要です。");

        if (detectionRange <= 0f || fireInterval <= 0f || projectileSpeed <= 0f)
            throw new InvalidOperationException($"{gameObject.name}: 射撃設定は正の値にしてください。");

        if (!hitCollider.isTrigger)
            throw new InvalidOperationException($"{gameObject.name}: Collider2DをTriggerにしてください。");
    }

    protected override void UpdateBehavior()
    {
        Vector2 origin = (Vector2)transform.position + muzzleOffset;

        // 基底クラス（AttackerEnemyBase）の距離・方向判定を利用
        float sqrDistance = GetSqrDistanceToTarget(origin);
        if (!fireLogic.Tick(Time.deltaTime, sqrDistance, detectionRange, fireInterval))
            return;

        Vector2 direction = GetDirectionToTarget(origin);
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

