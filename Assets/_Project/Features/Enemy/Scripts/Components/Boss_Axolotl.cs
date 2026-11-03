using UnityEngine;

public class Boss_Axolotl : EnemyBase
{
    [Header("HP")]
    [SerializeField] private int maxHP = 3;

    [Header("無敵時間")]
    [SerializeField, Min(0.01f)] private float coolDownDuration = 1.0f;

    [Header("バブルバリア")]
    [SerializeField, Min(0.1f)] private float barrierResetInterval = 5.0f;
    [SerializeField, Min(0.01f)] private float breakAnimationDuration = 0.5f;

    [Header("移動設定")]
    [SerializeField] private float moveSpeed = 2.0f;   // 速度
    [SerializeField] private float moveWidth = 2.0f;   //左右移動距離

    [Header("アニメーション")]
    [SerializeField] private Axolotl_Animation animationView;
    [SerializeField, Min(0.01f)] private float blinkDuration = 0.2f;

    [Header("射撃設定")]
    [SerializeField] private EnemyBullet projectilePrefab;
    [SerializeField, Min(0.01f)] private float detectionRange = 12f;
    [SerializeField, Min(0.01f)] private float fireInterval = 1.2f;
    [SerializeField, Min(0.01f)] private float projectileSpeed = 8f;
    [SerializeField] private Vector2 muzzleOffset = Vector2.zero;

    private readonly EnemyFireLogic barrierResetTimerLogic = new EnemyFireLogic();
    private readonly EnemyFireLogic breakTimerLogic = new EnemyFireLogic();
    private readonly EnemyFireLogic coolDownTimerLogic = new EnemyFireLogic();

    private Vector3 initialPosition;
    private bool hasBarrier = true;
    private bool isBreaking;
    public event System.Action BarrierBroken;
    private bool isCool;

    protected override void OnInitialize()
    {
        HP = maxHP;
        initialPosition = transform.position;

        if (animationView == null)
            animationView = GetComponentInChildren<Axolotl_Animation>();
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

        //バリア再展開用タイマー
        if (!hasBarrier && !isBreaking)
        {
            if (barrierResetTimerLogic.Tick(Time.deltaTime, 0f, 1f, barrierResetInterval))
            {
                hasBarrier = true;
            }
        }

        // タイマー更新
        if (isBreaking)
        {
            if (breakTimerLogic.Tick(Time.deltaTime, 0f, 1f, breakAnimationDuration))
            {
                isBreaking = false;
            }
        }

        // バリア展開中のみ左右移動
        if (hasBarrier)
        {
            float newX = initialPosition.x + Mathf.Sin(Time.time * moveSpeed) * moveWidth;
            transform.position = new Vector3(newX, transform.position.y, transform.position.z);
        }

        //射撃
        Vector2 origin = (Vector2)transform.position + muzzleOffset;
        float sqrDistance = GetSqrDistanceToTarget(origin);

        if (fireLogic.Tick(Time.deltaTime, sqrDistance, detectionRange, fireInterval))
        {
            Vector2 direction = GetDirectionToTarget(origin);
            EnemyBullet projectile = Instantiate(projectilePrefab, origin, Quaternion.identity);
            projectile.Launch(direction, projectileSpeed);
            NotifyAttackFired();
        }

        animationView.Render(hasBarrier, isBreaking);
    }

    public override void ReceiveAttack()
    {
        if (IsDefeated || isCool) return;

        SetCoolingDown(true);

        //バリアがみがわりになる
        if (hasBarrier)
        {
            hasBarrier = false;
            isBreaking = true;
            BarrierBroken?.Invoke();
            return;
        }

        if (isBreaking) return;

        HP--;
        animationView.PlayHitBlink(Color.red, blinkDuration);

        if (HP <= 0)
        {
            if (!TryDefeat())
                return;
            DropItem();
            hitCollider.enabled = false;
            gameObject.SetActive(false);
            Destroy(gameObject);
        }
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
