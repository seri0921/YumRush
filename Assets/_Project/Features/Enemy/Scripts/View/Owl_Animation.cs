using UnityEngine;
using UnityEngine.Assertions;

public sealed class Owl_Animation : MonoBehaviour
{
    [SerializeField] private Animator animator;
    [SerializeField] private SpriteRenderer spriteRenderer;

    // Animatorのステートハッシュ
    private static readonly int StateIdle = Animator.StringToHash("Base Layer.Owl_Idle");
    private static readonly int StateAttack = Animator.StringToHash("Base Layer.Owl_Attack");

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
        if (isBlinking)
        {
            if (blinkTimerLogic.Tick(Time.deltaTime, 0f, 1f, currentBlinkDuration))
            {
                isBlinking = false;
                spriteRenderer.color = Color.white;
            }
        }
    }

    public void Render(bool isAttacking)
    {
        int nextAnimation = isAttacking ? StateAttack : StateIdle;
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

}
