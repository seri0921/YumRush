using UnityEngine;
using UnityEngine.Assertions;

public class Axolotl_Animation : MonoBehaviour
{
    [SerializeField] private Animator animator;
    [SerializeField] private SpriteRenderer spriteRenderer;

    // Animatorのステートハッシュ
    private static readonly int StateBarrier = Animator.StringToHash("Base Layer.Axolotl_Bubble");
    private static readonly int StateBreak = Animator.StringToHash("Base Layer.Axolotl_Pop");
    private static readonly int StateNoBarrier = Animator.StringToHash("Base Layer.Axolotl_NoBubble");

    private readonly EnemyFireLogic blinkTimerLogic = new EnemyFireLogic();
    private bool isBlinking;
    private float currentBlinkDuration;

    private int currentAnimation;
    private bool hasAnimation;

    private void Awake()
    {
        Assert.IsNotNull(animator);
        Assert.IsNotNull(spriteRenderer);
    }

    private void Update()
    {
        // 被弾時の点滅更新
        if (isBlinking)
        {
            if (blinkTimerLogic.Tick(Time.deltaTime, 0f, 1f, currentBlinkDuration))
            {
                isBlinking = false;
                spriteRenderer.color = Color.white;
            }
        }
    }

    //バリア状態に応じたアニメーションを描画します。
    public void Render(bool hasBarrier, bool isBreaking)
    {
        int nextAnimation = ResolveAnimation(hasBarrier, isBreaking);
        if (hasAnimation && currentAnimation == nextAnimation)
            return;

        animator.Play(nextAnimation, 0, 0f);
        currentAnimation = nextAnimation;
        hasAnimation = true;
    }

    // 被弾時の点滅演出
    public void PlayHitBlink(Color blinkColor, float duration)
    {
        isBlinking = true;
        currentBlinkDuration = duration;
        spriteRenderer.color = blinkColor;
    }

    private int ResolveAnimation(bool hasBarrier, bool isBreaking)
    {
        if (isBreaking) return StateBreak;
        if (hasBarrier) return StateBarrier;
        return StateNoBarrier;
    }
}
