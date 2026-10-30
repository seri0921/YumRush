using UnityEngine;

public sealed class SpreadShootEnemy : EnemyBase
{
    private enum AttackPhase
    {
        Idle,           // 待機（プレイヤー検知待ち）
        Windup,         // 溜め（Attackアニメ再生＆発射準備）
        Bursting,       // マシンガン連射中
        CoolingDown     // 攻撃後のクールタイム
    }

    [Header("無敵時間")]
    [SerializeField, Min(0.01f)] private float coolDownDuration = 1.0f;

    [Header("マシンガン設定")]
    [Tooltip("プレイヤー検知範囲")]
    [SerializeField, Min(0.01f)] private float detectionRange = 8f;

    [Tooltip("溜め")]
    [SerializeField, Min(0.01f)] private float windupDuration = 0.8f;

    [Tooltip("弾数")]
    [SerializeField, Min(1)] private int burstCount = 5;

    [Tooltip("連射感覚")]
    [SerializeField, Min(0.01f)] private float burstInterval = 0.1f;

    [Tooltip("クールダウン")]
    [SerializeField, Min(0.01f)] private float attackCooldown = 2.0f;

    [Tooltip("散弾設定")]
    [SerializeField, Range(0f, 180f)] private float spreadAngle = 30f;
    [Tooltip("最小弾速")]
    [SerializeField, Min(0.01f)] private float minProjectileSpeed = 6f;
    [Tooltip("最大弾速")]
    [SerializeField, Min(0.01f)] private float maxProjectileSpeed = 10f;

    [Header("アニメーション")]
    [SerializeField] private Owl_Animation animationView;
    [SerializeField, Min(0.01f)] private float blinkDuration = 0.2f;

    [Header("射撃設定")]
    [SerializeField] private EnemyBullet projectilePrefab;
    [SerializeField] private Vector2 muzzleOffset = Vector2.zero;

    private readonly EnemyFireLogic phaseTimerLogic = new EnemyFireLogic();
    private readonly EnemyFireLogic burstTimerLogic = new EnemyFireLogic();
    private readonly EnemyFireLogic coolDownTimerLogic = new EnemyFireLogic();

    private AttackPhase currentPhase = AttackPhase.Idle;
    private int currentBurstFired;
    private bool isCool;
    private float lastFacingDirectionX = -1f;

    protected override void OnInitialize()
    {
        if (animationView == null)
            animationView = GetComponentInChildren<Owl_Animation>();
    }

    protected override void UpdateBehavior()
    {
        if (isCool)
        {
            if (coolDownTimerLogic.Tick(Time.deltaTime, 0f, 1f, coolDownDuration))
            {
                SetCoolingDown(false);
            }
        }

        Vector2 origin = (Vector2)transform.position + muzzleOffset;
        float sqrDistance = GetSqrDistanceToTarget(origin);
        Vector2 dirToTarget = GetDirectionToTarget(origin);

        if (dirToTarget.x != 0f)
        {
            lastFacingDirectionX = dirToTarget.x;
        }

        switch (currentPhase)
        {
            case AttackPhase.Idle:
                if (sqrDistance <= detectionRange * detectionRange)
                {
                    currentPhase = AttackPhase.Windup;
                }
                break;

            case AttackPhase.Windup:
                if (phaseTimerLogic.Tick(Time.deltaTime, 0f, 1f, windupDuration))
                {
                    currentPhase = AttackPhase.Bursting;
                    currentBurstFired = 0;
                }
                break;

            case AttackPhase.Bursting:
                if (burstTimerLogic.Tick(Time.deltaTime, 0f, 1f, burstInterval))
                {
                    FireBullet(origin, dirToTarget);
                    currentBurstFired++;

                    if (currentBurstFired >= burstCount)
                    {
                        currentPhase = AttackPhase.CoolingDown;
                    }
                }
                break;

            case AttackPhase.CoolingDown:
                if (phaseTimerLogic.Tick(Time.deltaTime, 0f, 1f, attackCooldown))
                {
                    currentPhase = AttackPhase.Idle;
                }
                break;
        }

        bool isAttacking = (currentPhase == AttackPhase.Windup || currentPhase == AttackPhase.Bursting);
        animationView.Render(isAttacking, lastFacingDirectionX);
    }

    public override void ReceiveAttack()
    {
        if (IsDefeated || isCool) return;

        SetCoolingDown(true);

        HP--;
        animationView.PlayHitBlink(Color.red, blinkDuration);

        if (HP <= 0)
        {
            if (!defeatLogic.TryDefeat())
                return;

            hitCollider.enabled = false;
            gameObject.SetActive(false);
            Destroy(gameObject);
        }
    }

    private void FireBullet(Vector2 origin, Vector2 baseDirection)
    {
        if (projectilePrefab == null) return;

        float randomAngle = Random.Range(-spreadAngle * 0.5f, spreadAngle * 0.5f);
        Vector2 shotDirection = Quaternion.Euler(0f, 0f, randomAngle) * baseDirection;

        float speed = Random.Range(minProjectileSpeed, maxProjectileSpeed);

        EnemyBullet projectile = Instantiate(projectilePrefab, origin, Quaternion.identity);
        projectile.Launch(shotDirection, speed);
    }

    private void SetCoolingDown(bool setCool)
    {
        isCool = setCool;
        if (hitCollider != null)
        {
            hitCollider.enabled = !setCool;
        }
    }

}
