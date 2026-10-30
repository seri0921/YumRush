using UnityEngine;
using UnityEngine.Assertions;

public sealed class Owl_Animation : EnemyAnimationViewBase
{
    // Animatorのステートハッシュ
    [SerializeField] private string idleStateName = "Owl_Idle";
    [SerializeField] private string attackStateName = "Owl_Attack";

    private int stateIdleHash;
    private int stateAttackHash;

    protected override void Awake()
    {
        base.Awake();
        stateIdleHash = Animator.StringToHash(idleStateName);
        stateAttackHash = Animator.StringToHash(attackStateName);
 
    }

    public void Render(bool isAttacking, float facingDirectionX)
    {
        ApplyBaseColor();

        if (spriteRenderer != null && facingDirectionX != 0f)
        {
            spriteRenderer.flipX = facingDirectionX > 0f;
        }

        PlayAnimation(isAttacking ? stateAttackHash : stateIdleHash);
    }


}
