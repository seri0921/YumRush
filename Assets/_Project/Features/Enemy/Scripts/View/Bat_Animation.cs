using System;
using UnityEngine;
using UnityEngine.Assertions;

public sealed class Bat_Animation : EnemyAnimationViewBase
{

    private static readonly int BatIdle = Animator.StringToHash("Base Layer.Bat_Idle");
    private static readonly int BatOpen = Animator.StringToHash("Base Layer.Bat_Open");

    public void Render(bool isProtect, bool isAttacking)
    {
        baseColor = isProtect ? new Color(0.5f, 0.8f, 1.0f, 0.8f) : Color.white;
        ApplyBaseColor();
        int nextAnimation = isProtect ? BatIdle : BatOpen;
        PlayAnimation(nextAnimation);
    }
}
