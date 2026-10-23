using System;
using UnityEngine;

public abstract class EnemyBase : MonoBehaviour, IEnemy
{
    [Space(8)]
    [Header("参照")]
    [Tooltip("射撃対象のプレイヤー。未設定時はシーン内から取得します。")]
    [SerializeField]
    protected PlayerPresenter target;

    protected readonly EnemyDefeatLogic defeatLogic = new EnemyDefeatLogic();
    protected readonly EnemyFireLogic fireLogic = new EnemyFireLogic();

    protected Collider2D hitCollider;
    public int HP = 0;
    public bool IsDefeated => defeatLogic.IsDefeated;
    protected bool HasTarget => target != null;
    protected Vector3 TargetPosition => HasTarget ? target.transform.position : Vector3.zero;
    protected virtual void Awake()
    {
        hitCollider = GetComponent<Collider2D>();
        if (target == null)
            target = FindFirstObjectByType<PlayerPresenter>();

        OnInitialize();
    }

    protected virtual void OnInitialize() { }

    protected virtual void Update()
    {
        if (IsDefeated)
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


    protected abstract void UpdateBehavior();
    public abstract void ReceiveAttack();
}
