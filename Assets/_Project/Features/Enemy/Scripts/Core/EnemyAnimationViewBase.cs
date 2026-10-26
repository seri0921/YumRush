using UnityEngine;
using UnityEngine.Assertions;

public abstract class EnemyAnimationViewBase : MonoBehaviour
{
    [SerializeField] protected Animator animator;
    [SerializeField] protected SpriteRenderer spriteRenderer;

    private readonly EnemyFireLogic blinkTimerLogic = new EnemyFireLogic();
    private bool isBlinking;
    private float currentBlinkDuration;
    protected Color baseColor = Color.white;

    private int currentAnimationHash;
    private bool hasAnimation;

    protected virtual void Awake()
    {
        Assert.IsNotNull(animator, $"{gameObject.name}: アニメーターどこ");
        Assert.IsNotNull(spriteRenderer, $"{gameObject.name}:レンダラーどこ");
    }

    protected virtual void Update()
    {
        // 点滅処理
        if (isBlinking)
        {
            if (blinkTimerLogic.Tick(Time.deltaTime, 0f, 1f, currentBlinkDuration))
            {
                isBlinking = false;
                ApplyBaseColor();
            }
        }
    }

    protected void PlayAnimation(int stateHash)
    {
        if (hasAnimation && currentAnimationHash == stateHash)
            return;

        animator.Play(stateHash, 0, 0f);
        currentAnimationHash = stateHash;
        hasAnimation = true;
    }

    public virtual void PlayHitBlink(Color blinkColor, float duration)
    {
        isBlinking = true;
        currentBlinkDuration = duration;
        if (spriteRenderer != null)
            spriteRenderer.color = blinkColor;
    }

    protected void ApplyBaseColor()
    {
        if (isBlinking) return;
        if (spriteRenderer != null)
            spriteRenderer.color = baseColor;
    }

}
