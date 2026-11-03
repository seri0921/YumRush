using System;
using UnityEngine;

public abstract class EnemyBase : MonoBehaviour, IEnemy
{
    [Space(8)]
    [Header("参照")]
    [Tooltip("射撃対象のプレイヤー。未設定時はシーン内から取得します。")]
    [SerializeField]
    protected PlayerPresenter target;

    [Space(8)]
    [Header("ドロップ")]
    [SerializeField] private ScoreItem dropItemPrefab;

    [SerializeField] private EnemyScoreConfig _scoreConfig;
    [SerializeField] private ScorePresenter _scorePresenter;

    protected readonly EnemyDefeatLogic defeatLogic = new EnemyDefeatLogic();
    protected readonly EnemyFireLogic fireLogic = new EnemyFireLogic();

    protected Collider2D hitCollider;
    public int HP = 0;
    public bool IsDefeated => defeatLogic.IsDefeated;
    public event Action Defeated;
    public event Action AttackFired;
    protected bool HasTarget => target != null;
    protected Vector3 TargetPosition => HasTarget ? target.transform.position : Vector3.zero;
    protected virtual void Awake()
    {
        hitCollider = GetComponent<Collider2D>();
        if (target == null)
            target = FindFirstObjectByType<PlayerPresenter>();

        if (_scoreConfig == null)
            throw new InvalidOperationException($"{name} requires an EnemyScoreConfig.");
        if (_scorePresenter == null)
            _scorePresenter = FindFirstObjectByType<ScorePresenter>();
        if (_scorePresenter == null)
            throw new InvalidOperationException($"{name} requires a ScorePresenter in the scene.");

        OnInitialize();
    }

    protected virtual void OnInitialize() { }

    protected virtual void Update()
    {
        if (IsDefeated || Time.timeScale == 0f)
            return;

        UpdateBehavior();
    }

    protected float GetSqrDistanceToTarget(Vector2 origin)
    {
        if (!HasTarget) return float.PositiveInfinity;
        return ((Vector2)target.transform.position - origin).sqrMagnitude;
    }

    protected Vector2 GetDirectionToTarget(Vector2 origin)
    {
        if (!HasTarget) return Vector2.down;
        Vector2 displacement = (Vector2)target.transform.position - origin;
        return displacement.sqrMagnitude > 0f ? displacement.normalized : Vector2.down;
    }

    protected void NotifyAttackFired() => AttackFired?.Invoke();

    protected bool TryDefeat()
    {
        if (!defeatLogic.TryDefeat())
            return false;

        _scorePresenter.Add(_scoreConfig.Points);
        Defeated?.Invoke();
        return true;
    }

    protected void DropItem()
    {
        if (dropItemPrefab == null) return;
        Instantiate(dropItemPrefab, transform.position, Quaternion.identity);
    }

    protected abstract void UpdateBehavior();
    public abstract void ReceiveAttack();
}
