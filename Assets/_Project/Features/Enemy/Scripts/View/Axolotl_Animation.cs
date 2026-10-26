using UnityEngine;
using UnityEngine.Assertions;

public sealed class Axolotl_Animation : EnemyAnimationViewBase
{
    private static readonly int StateBarrier = Animator.StringToHash("Base Layer.Axolotl_Bubble");
    private static readonly int StateBreak = Animator.StringToHash("Base Layer.Axolotl_Pop");
    private static readonly int StateNoBarrier = Animator.StringToHash("Base Layer.Axolotl_NoBubble");

    public void Render(bool hasBarrier, bool isBreaking)
    {
        ApplyBaseColor();

        int nextAnimation = ResolveAnimation(hasBarrier, isBreaking);
        PlayAnimation(nextAnimation);
    }
    private int ResolveAnimation(bool hasBarrier, bool isBreaking)
    {
        if (isBreaking) return StateBreak;
        if (hasBarrier) return StateBarrier;
        return StateNoBarrier;
    }
}
