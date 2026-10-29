using System;
using UnityEngine;
using UnityEngine.Assertions;

// ゲーム状態を表示へ変換する。移動や状態遷移は行わない。
public sealed class PlayerAnimationView : MonoBehaviour
{
    [SerializeField] private Animator animator;
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private SpriteRenderer glideEffectRenderer;
    [Tooltip("元画像が右向きならオン、左向きならオフにします。")]
    [SerializeField] private bool spriteFacesRight = true;
    [SerializeField, Min(0f)] private float walkSpeedThreshold = 0.1f;

    private static readonly int Idle = Animator.StringToHash("Base Layer.Idle");
    private static readonly int Walk = Animator.StringToHash("Base Layer.Walk");
    private static readonly int Jump = Animator.StringToHash("Base Layer.Jump");
    private static readonly int AirDash = Animator.StringToHash("Base Layer.AirDash");
    private static readonly int Glide = Animator.StringToHash("Base Layer.Glide");
    private static readonly int Damage = Animator.StringToHash("Base Layer.Damage");
    private static readonly int Homing = Animator.StringToHash("Base Layer.Homing");
    
    private int currentAnimation;
    private bool hasAnimation;

    private void Awake()
    {
        Assert.IsNotNull(animator);
        Assert.IsNotNull(spriteRenderer);
        Assert.IsNotNull(glideEffectRenderer);
        if (!animator.HasState(0, Idle) || !animator.HasState(0, Walk) ||
            !animator.HasState(0, Jump) || !animator.HasState(0, AirDash) ||
            !animator.HasState(0, Glide) || !animator.HasState(0, Damage) ||
            !animator.HasState(0, Homing))
            throw new InvalidOperationException("Player Animator requires Idle, Walk, Jump, AirDash, Glide, Damage and Homing in Base Layer.");
    }

    private void OnEnable()
    {
        hasAnimation = false;
        glideEffectRenderer.enabled = false;
    }

    private void OnDisable() => glideEffectRenderer.enabled = false;

    public void Render(PlayerState state, float horizontalVelocity, int facingDirection)
    {
        int nextAnimation = ResolveAnimation(state, horizontalVelocity);
        bool facesRight = facingDirection > 0;
        spriteRenderer.flipX = facesRight != spriteFacesRight;
        glideEffectRenderer.flipX = spriteRenderer.flipX;
        glideEffectRenderer.enabled = state == PlayerState.Gliding;
        if (hasAnimation && currentAnimation == nextAnimation)
            return;
        
        animator.Play(nextAnimation, 0, 0f);
        currentAnimation = nextAnimation;
        hasAnimation = true;
    }

    private int ResolveAnimation(PlayerState state, float horizontalVelocity)
    {
        switch (state)
        {
            case PlayerState.Grounded:
            case PlayerState.IceGrounded:
                return Mathf.Abs(horizontalVelocity) > walkSpeedThreshold ? Walk : Idle;
            case PlayerState.AirDashing:
                return AirDash;
            case PlayerState.Gliding:
                return Glide;
            case PlayerState.Hit:
                return Damage;
            case PlayerState.HomingWindup:
            case PlayerState.Homing:
                return Homing;
            case PlayerState.AirCurrent:
            case PlayerState.Airborne:
            case PlayerState.WindJump:
            // 専用クリップが用意されるまでは空中ポーズを使用する。
            case PlayerState.Capturing:
            case PlayerState.Floating:
                return Jump;
            default:
                throw new ArgumentOutOfRangeException(nameof(state), state, null);
        }
    }
}
