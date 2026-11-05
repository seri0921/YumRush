using UnityEngine;
using System;
public class HarmlessBonus : HarmlessEnemyBase
{
    [Space(8)]
    [Header("耐久度設定")]
    [SerializeField, Min(1)] private int hp = 5;

    [Tooltip("被弾後の無敵時間")]
    [SerializeField, Min(0.01f)] private float coolDownDuration = 0.5f;

    [Tooltip("HPが0になってから消滅するまでの猶予時間（秒）")]
    [SerializeField, Min(0f)] private float defeatDelayDuration = 2.0f;

    [Space(8)]
    [Header("ドロップ設定")]
    [SerializeField] private ScoreItem scoreItemPrefab;

    [Tooltip("1回の被弾で吹き出すアイテム数の最小値")]
    [SerializeField, Min(1)] private int minDropCount = 5;

    [Tooltip("1回の被弾で吹き出すアイテム数の最大値")]
    [SerializeField, Min(1)] private int maxDropCount = 8;

    [Space(8)]
    [Header("散開ベクトルの力")]
    [Tooltip("左右方向への散開力（-X 〜 +X）")]
    [SerializeField, Min(0.1f)] private float horizontalForce = 4.0f;

    [Tooltip("上方向への最小初速")]
    [SerializeField, Min(0.1f)] private float minVerticalForce = 2.0f;

    [Tooltip("上方向への最大初速")]
    [SerializeField, Min(0.1f)] private float maxVerticalForce = 3.0f;

    [Space(8)]
    [Header("ビジュアル・演出")]
    [SerializeField] private BonusEnemyView animationView;
    [SerializeField, Min(0.01f)] private float blinkDuration = 0.2f;

    private readonly EnemyFireLogic coolDownTimerLogic = new EnemyFireLogic();
    private readonly EnemyFireLogic defeatDelayTimerLogic = new EnemyFireLogic();
    private int currentHp;
    private bool isCool;
    private bool isPendingDefeat;

    protected override void Awake()
    {
        base.Awake();
        currentHp = hp;
        if (animationView == null)
            animationView = GetComponentInChildren<BonusEnemyView>();
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

        if (isPendingDefeat)
        {
            if (defeatDelayTimerLogic.Tick(Time.deltaTime, 0f, 1f, defeatDelayDuration))
            {
                FinalizeDefeat();
            }
        }

    }

    public override void ReceiveAttack()
    {
        if (IsDefeated || isCool) return;

        SetCoolingDown(true);

        currentHp--;

        if (animationView != null)
        {
            animationView.Render(currentHp);
            animationView.PlayHitBlink(Color.yellow, blinkDuration);
            animationView.PlayWobble();
        }

        EjectScoreItems();

        if (currentHp <= 0)
        {
            StartDefeatPhase();
        }
    }

    private void StartDefeatPhase()
    {
        isPendingDefeat = true;

        if (hitCollider != null)
        {
            hitCollider.enabled = false;
        }
        if (TryGetComponent<RespawnTarget>(out var respawnTarget))
        {
            respawnTarget.NotifyConsumed();
        }
    }

    private void FinalizeDefeat()
    {
        if (!defeatLogic.TryDefeat())
            return;

        gameObject.SetActive(false);
        Destroy(gameObject);
    }

    private void EjectScoreItems()
    {
        if (scoreItemPrefab == null) return;

        int dropCount = UnityEngine.Random.Range(minDropCount, maxDropCount + 1);

        for (int i = 0; i < dropCount; i++)
        {
            ScoreItem item = Instantiate(scoreItemPrefab, transform.position, Quaternion.identity);

            float randomX = UnityEngine.Random.Range(-horizontalForce, horizontalForce);
            float randomY = UnityEngine.Random.Range(minVerticalForce, maxVerticalForce);

            Vector2 popForce = new Vector2(randomX, randomY);

            if (item.TryGetComponent<ItemPhysicPopper>(out var popper))
            {
                popper.Pop(popForce);
            }
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
