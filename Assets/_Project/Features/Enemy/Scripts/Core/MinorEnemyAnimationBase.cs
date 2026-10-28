using UnityEngine;

public readonly struct ShooterAnimationState
{
    public bool IsActivated { get; }
    public bool IsFiring { get; }
    public float FacingDirectionX { get; }

    public ShooterAnimationState(bool isActivated, bool isFiring, float facingDirectionX)
    {
        IsActivated = isActivated;
        IsFiring = isFiring;
        FacingDirectionX = facingDirectionX;
    }
}

public abstract class ShooterEnemyAnimationBase : EnemyAnimationViewBase
{
    public abstract void Render(ShooterAnimationState state);
}