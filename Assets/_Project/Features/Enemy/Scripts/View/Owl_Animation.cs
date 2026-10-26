using UnityEngine;
using UnityEngine.Assertions;

public sealed class Owl_Animation : EnemyAnimationViewBase
{
    // Animatorのステートハッシュ
    private static readonly int StateIdle = Animator.StringToHash("Base Layer.Owl_Idle");
    private static readonly int StateAttack = Animator.StringToHash("Base Layer.Owl_Attack");

    public void Render(bool isAttacking)
    {
        if (!isAttacking) ApplyBaseColor(); 
        PlayAnimation(isAttacking ? StateAttack : StateIdle);
    }
}
