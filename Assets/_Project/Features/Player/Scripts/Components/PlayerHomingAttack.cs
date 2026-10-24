using System;
using UnityEngine;
using UnityEngine.Serialization;

/// <summary>
/// 対象の探索・物理接触・攻撃適用だけを担当。動作状態と入力は所有しない。
/// </summary>
[RequireComponent(typeof(PlayerPresenter))]
public sealed class PlayerHomingAttack : MonoBehaviour
{
    [SerializeField] private PlayerMovementConfig config;
    [SerializeField] private Collider2D PlayerHomingAttackCollider;
    private readonly Collider2D[] hits = new Collider2D[64];
    private Rigidbody2D body;

    private ContactFilter2D targetFilter;

    // 攻撃前は候補、攻撃準備・追尾中は確定対象。候補なし・対象消失時はnull。
    private IEnemy target;
    private Collider2D targetCollider;


    private void Awake()
    {
        if (config == null)
            throw new InvalidOperationException("PlayerHomingAttack requires a PlayerConfig.");
        config.Validate();
        
        body = GetComponent<Rigidbody2D>();
        targetFilter = new ContactFilter2D { useTriggers = true };
        targetFilter.SetLayerMask(config.TargetLayers);
    }

    private void OnDisable() => ClearTarget();

    private void OnDrawGizmos()
    {
        if (config == null) // Inspectorで未設定の編集中は非表示。
            return;
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, config.AttackRange);
    }

    public void Sample(ref PlayerFrame frame, bool homing)
    {
        if (!isActiveAndEnabled)
            return;
        // 攻撃前は候補を更新し、準備・追尾中は同じ対象を保持する。
        if (!homing)
            FindNearestTarget();
        frame.Homing.HasHomingTarget = target != null && targetCollider != null &&
                                        !target.IsDefeated && targetCollider.enabled &&
                                       targetCollider.gameObject.activeInHierarchy;
        if (!frame.Homing.HasHomingTarget)
            return;
        ColliderDistance2D distance = PlayerHomingAttackCollider.Distance(targetCollider);
        frame.Homing.TouchingHomingTarget = distance.isOverlapped || distance.distance <= config.ContactDistance;
        Vector2 direction = ((Vector2)targetCollider.bounds.center - body.position).normalized;
        frame.Homing.HomingDirectionX = direction.x;
        frame.Homing.HomingDirectionY = direction.y;
    }

    public void Resolve(in PlayerStepResult result, bool homing)
    {
        if (result.ApplyHomingAttack)
        {
            if (target == null)
                throw new InvalidOperationException("A homing hit requires its sampled target.");
            
            target.ReceiveAttack();
        }

        // 命中した対象は解除し、次のSampleで候補を再取得する。
        if (!homing && result.ApplyHomingAttack)
            ClearTarget();
    }

    // 対象の破棄・撃破は演出中にも発生する正当な中断条件。
    public bool TryGetTargetPosition(out Vector2 position)
    {
        bool valid = isActiveAndEnabled && target != null && targetCollider != null &&
                     !target.IsDefeated && targetCollider.enabled && targetCollider.gameObject.activeInHierarchy;
        position = valid ? (Vector2)targetCollider.bounds.center : Vector2.zero;
        return valid;
    }
    public void ClearTarget()
    {
        target = null;
        targetCollider = null;
    }

    private void FindNearestTarget()
    {
        ClearTarget();
        float nearest = float.PositiveInfinity;
        int count = Physics2D.OverlapCircle(body.position, config.AttackRange, targetFilter, hits);
        for (int i = 0; i < count; i++)
        {
            Collider2D candidate = hits[i];
            if (!candidate.TryGetComponent<IEnemy>(out var enemy) || enemy.IsDefeated)
                continue;
            float distance = ((Vector2)candidate.ClosestPoint(body.position) - body.position).sqrMagnitude;
            if (!HomingAttackLogic.IsCloser(distance, nearest, config.AttackRange * config.AttackRange))
                continue;
            nearest = distance;
            target = enemy;
            targetCollider = candidate;
        }
    }

}
