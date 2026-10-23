using System;
using UnityEngine;
using UnityEngine.Assertions;

public class Bat_Animation : MonoBehaviour
{
    [SerializeField] private Animator animator;
    [SerializeField] private SpriteRenderer spriteRenderer;

    // Animatorのステートハッシュ（Base Layer 内のステート名に合わせます）
    private static readonly int BatIdle = Animator.StringToHash("Base Layer.Bat_Idle");
    private static readonly int BatOpen = Animator.StringToHash("Base Layer.Bat_Open");

    private readonly EnemyFireLogic blinkTimerLogic = new EnemyFireLogic();
    private bool isBlinking;
    private float currentBlinkDuration;
    private Color baseColor = Color.white;

    private int currentAnimation;
    private bool hasAnimation;

    private void Awake()
    {
        Assert.IsNotNull(animator);
        Assert.IsNotNull(spriteRenderer);
    }

    private void Update()
    {
        if (isBlinking)
        {
            if (blinkTimerLogic.Tick(Time.deltaTime, 0f, 1f, currentBlinkDuration))
            {
                isBlinking = false;
                ApplyBaseColor();
            }
        }
    }

    public void Render(bool isProtect, bool isAttacking)
    {
        // ベースカラーの設定（Protect時は青シールド色、通常時は白）
        baseColor = isProtect ? new Color(0.5f, 0.8f, 1.0f, 0.8f) : Color.white;
        if (!isBlinking)
        {
            ApplyBaseColor();
        }

        // アニメーション制御（2つのステートから切り替え）
        int nextAnimation = ResolveAnimation(isProtect);
        if (hasAnimation && currentAnimation == nextAnimation)
            return;

        animator.Play(nextAnimation, 0, 0f);
        currentAnimation = nextAnimation;
        hasAnimation = true;
    }

    public void PlayHitBlink(Color blinkColor, float duration)
    {
        isBlinking = true;
        currentBlinkDuration = duration;
        spriteRenderer.color = blinkColor;
    }

    private void ApplyBaseColor()
    {
        if (spriteRenderer != null)
            spriteRenderer.color = baseColor;
    }

    /// <summary>
    /// Protect（防御・羽閉じ）状態なら Bat_Idle、それ以外（羽開き状態）なら Bat_Open を返します。
    /// </summary>
    private int ResolveAnimation(bool isProtect)
    {
        return isProtect ? BatIdle : BatOpen;
    }
}
