using UnityEngine;

public class MinorBat_Animation : EnemyAnimationViewBase
{
    private static readonly int StateIdle = Animator.StringToHash("MinorBat_Idle");
    private static readonly int StateFly = Animator.StringToHash("MinorBat_Fly");

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

