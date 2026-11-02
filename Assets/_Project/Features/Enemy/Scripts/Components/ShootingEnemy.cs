using System;
using UnityEngine;

public sealed class ShootingEnemy : EnemyBase
{
    [Space(8)]
    [Header("ビジュアル参照")]
    [SerializeField] private ShooterEnemyAnimationBase animationView;

    [Space(8)]
    [Header("移動・漂い設定")]
    [Tooltip("左右に漂う速度")]
    [SerializeField, Min(0.01f)] private float moveSpeed = 2.0f;

    [Tooltip("左右に漂う幅")]
    [SerializeField, Min(0.01f)] private float moveWidth = 1.5f;


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
    private Vector3 initialPosition;
    private float elapsedTime;
    private bool isActivated;
    private float initialDirectionX = 1f;
    protected override void OnInitialize()
    {
        initialPosition = transform.position;

        if (animationView == null)
            animationView = GetComponentInChildren<ShooterEnemyAnimationBase>();
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
        float sqrDistance = GetSqrDistanceToTarget(origin);

        if (!isActivated)
        {
            if (sqrDistance <= detectionRange * detectionRange)
            {
                isActivated = true;
                Vector2 dirToTarget = GetDirectionToTarget(origin);
                initialDirectionX = dirToTarget.x >= 0f ? 1f : -1f;
            }
            else
            {
                animationView.Render(new ShooterAnimationState(
                                    isActivated: false,
                                    isFiring: false,
                                    facingDirectionX: 0f
                                ));
                return;
            }
        }

        elapsedTime += Time.deltaTime;
        float offsetScope = Mathf.Sin(elapsedTime * moveSpeed) * moveWidth * initialDirectionX;
        transform.position = new Vector3(initialPosition.x + offsetScope, transform.position.y, transform.position.z);

        Vector2 direction = GetDirectionToTarget(origin);
        bool isFiring = fireLogic.Tick(Time.deltaTime, sqrDistance, detectionRange, fireInterval);
        if (isFiring)
        {
            EnemyBullet projectile = Instantiate(projectilePrefab, origin, Quaternion.identity);
            projectile.Launch(direction, projectileSpeed);
        }

        animationView.Render(new ShooterAnimationState(
                    isActivated: true,
                    isFiring: isFiring,
                    facingDirectionX: direction.x
                ));
    }

    public override void ReceiveAttack()
    {
        if (!TryDefeat())
            return;
        hitCollider.enabled = false;
        if (TryGetComponent<RespawnTarget>(out var respawnTarget))
            respawnTarget.NotifyConsumed();
        gameObject.SetActive(false);
        Destroy(gameObject);
    }
}

