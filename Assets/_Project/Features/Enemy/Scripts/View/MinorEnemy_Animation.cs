using UnityEngine;

public class MinorEnemy_Animation : ShooterEnemyAnimationBase
{
    [Header("Animatorステート名")]
    [SerializeField] private string moveStateName = "Move";
    [SerializeField] private string attackStateName = "Attack";

    [Tooltip("攻撃モーションの再生時間（秒）")]
    [SerializeField, Min(0.01f)] private float attackDuration = 0.3f;

    private int stateMoveHash;
    private int stateAttackHash;
    private float attackTimer;

    protected override void Awake()
    {
        base.Awake();
        stateMoveHash = Animator.StringToHash(moveStateName);
        stateAttackHash = Animator.StringToHash(attackStateName);
    }

    public override void Render(ShooterAnimationState state)
    {
        ApplyBaseColor();

        if (spriteRenderer != null && state.FacingDirectionX != 0f)
        {
            spriteRenderer.flipX = state.FacingDirectionX > 0f;
        }

        if (state.IsFiring)
        {
            attackTimer = attackDuration;
        }

        if (attackTimer > 0f)
        {
            attackTimer -= Time.deltaTime;
            PlayAnimation(stateAttackHash);
        }
        else
        {
            PlayAnimation(stateMoveHash);
        }
    }


}
