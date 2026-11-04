using UnityEngine;

public class StaticSpriteAnimation : EnemyAnimationViewBase
{
    public void Render(float facingDirectionX)
    {
        ApplyBaseColor();

        // Œü‚«‚Ì”½“]ˆ—‚Ì‚Ýs‚¤
        if (spriteRenderer != null && facingDirectionX != 0f)
        {
            spriteRenderer.flipX = facingDirectionX > 0f;
        }
    }
}
