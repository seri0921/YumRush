using UnityEngine;

public class Boss_Bat : EnemyBase
{

    private enum BossState
    {
        Normal,
        Protect
    }

    [Header("É{ÉXHP")]
    [SerializeField] private int maxHP = 3;

    [Header("âHÇäJÇ≠ïpìx")]
    [SerializeField, Min(0.1f)] private float stateInterval = 3.0f;

    [Header("îÌíeéûÇÃéÀåÇí‚é~")]
    [SerializeField, Min(0.1f)] private float attackPauseDuration = 1.5f;

    [Header("ÉAÉjÉÅÅ[ÉVÉáÉì")]
    [SerializeField] private Bat_Animation animationView;
    [SerializeField, Min(0.01f)] private float blinkDuration = 0.2f;

    [Header("îÌíeéûñ≥ìGéûä‘")]
    [SerializeField, Min(0.1f)] private float coolDown = 1.0f;

    [Header("éÀåÇê›íË")]
    [SerializeField] private EnemyBullet projectilePrefab;
    [SerializeField, Min(0.01f)] private float detectionRange = 12f;
    [SerializeField, Min(0.01f)] private float fireInterval = 1.0f;
    [SerializeField, Min(0.01f)] private float projectileSpeed = 10f;
    [SerializeField] private Vector2 muzzleOffset = Vector2.zero;

    private readonly EnemyFireLogic stateTimerLogic = new EnemyFireLogic();
    private readonly EnemyFireLogic attackPauseTimerLogic = new EnemyFireLogic();
    private readonly EnemyFireLogic coolDownTimerLogic = new EnemyFireLogic();
    private BossState currentState = BossState.Normal;
    private bool isAttackPaused;
    private bool isCool;

    protected override void OnInitialize()
    {
        HP = maxHP;
        if (animationView == null)
            animationView = GetComponentInChildren<Bat_Animation>();
    }


    protected override void UpdateBehavior() 
    {
        if (isCool)
        {
            if (coolDownTimerLogic.Tick(Time.deltaTime, 0f, 1f, coolDown))
            {
                SetCoolingDown(false);
            }
        }

        if (stateTimerLogic.Tick(Time.deltaTime, 0f, 1f, stateInterval))
        {
            currentState = (currentState == BossState.Normal) ? BossState.Protect : BossState.Normal;
        }

        if (isAttackPaused)
        {
            if (attackPauseTimerLogic.Tick(Time.deltaTime, 0f, 1f, attackPauseDuration))
            {
                isAttackPaused = false;
            }
        }

        if (!isAttackPaused)
        {
            Vector2 origin = (Vector2)transform.position + muzzleOffset;
            float sqrDistance = GetSqrDistanceToTarget(origin);

            if (fireLogic.Tick(Time.deltaTime, sqrDistance, detectionRange, fireInterval))
            {
                Vector2 direction = GetDirectionToTarget(origin);
                EnemyBullet projectile = Instantiate(projectilePrefab, origin, Quaternion.identity);
                projectile.Launch(direction, projectileSpeed);
            }
        }

        bool isProtect = currentState == BossState.Protect;
        animationView.Render(isProtect, !isAttackPaused);
    }
    public override void ReceiveAttack() 
    {
        if (IsDefeated || isCool) return;

        SetCoolingDown(true);
        isAttackPaused = true;

        //âHÇèÙÇÒÇ≈Ç¢ÇÈÇ∆Ç´ÇÕê¬ì_ñ≈Ç≈ÉKÅ[ÉhÇ∆Ç©Ç…Ç»ÇËÇ‹ÇπÇÒÇ©
        if (currentState == BossState.Protect)
        {
            animationView.PlayHitBlink(Color.cyan, blinkDuration);
            return;
        }

        HP--;
        //ê‘ì_ñ≈
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

    private void SetCoolingDown(bool setCool)
    {
        isCool = setCool;
        if (hitCollider != null)
        {
            hitCollider.enabled = !setCool;
        }
    }

}
