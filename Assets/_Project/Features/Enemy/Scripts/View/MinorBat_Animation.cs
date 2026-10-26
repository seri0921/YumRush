using UnityEngine;

public class MinorBat_Animation : EnemyAnimationViewBase
{
    private static readonly int StateIdle = Animator.StringToHash("Base Layer.MinotBat_Idle");
    private static readonly int StateFly = Animator.StringToHash("Base Layer.MinorBat_Fly");

    public void Render(bool isFlying, float facingDirectionX)
    {
        ApplyBaseColor();

        if (spriteRenderer != null && facingDirectionX != 0f)
        {
            spriteRenderer.flipX = facingDirectionX > 0f;
        }

        PlayAnimation(isFlying ? StateFly : StateIdle);
    }
}

