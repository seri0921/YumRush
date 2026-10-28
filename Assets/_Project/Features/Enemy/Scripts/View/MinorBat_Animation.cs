using UnityEngine;

public class MinorBat_Animation : ShooterEnemyAnimationBase
{
    [Header("ƒXƒe[ƒg–¼(ŒëŽš–hŽ~)")]
    [SerializeField] private string StateIdle = "MinorBat_Idle";
    [SerializeField] private string StateFly = "MinorBat_Fly";

    private int stateIdleHash;
    private int stateFlyHash;

    protected override void Awake()
    {
        base.Awake();
        stateIdleHash = Animator.StringToHash(StateIdle);
        stateFlyHash = Animator.StringToHash(StateFly);
    }
    public override void Render(ShooterAnimationState state)
    {
        ApplyBaseColor();

        if (spriteRenderer != null && state.FacingDirectionX != 0f)
        {
            spriteRenderer.flipX = state.FacingDirectionX > 0f;
        }

        PlayAnimation(state.IsActivated ? stateFlyHash : stateIdleHash);
    }
}

