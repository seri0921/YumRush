using System;

public sealed class PlayerHitLogic
{
    private float lockRemaining;
    private float invulnerableRemaining;

    public bool IsLocked => lockRemaining > 0f;
    public bool IsInvulnerable => invulnerableRemaining > 0f;

    public void BeingHit(float lockDuration, float invulnerableDuration)
    {
        if (lockDuration <= 0f || invulnerableDuration <= 0f ||
            float.IsNaN(lockDuration) || float.IsNaN(invulnerableDuration))
            throw new ArgumentOutOfRangeException(nameof(lockDuration));
        
        
        lockRemaining = lockDuration;
        invulnerableRemaining = invulnerableDuration;
    }

    public void SetInvulnerable(float duration)
    {
        if (duration <= 0f || float.IsNaN(duration))
            throw new ArgumentOutOfRangeException(nameof(duration));

        invulnerableRemaining = duration;
    }

    public void Tick(float deltaTime)
    {
        if (deltaTime < 0f || float.IsNaN(deltaTime))
            throw new ArgumentOutOfRangeException(nameof(deltaTime));
        
        lockRemaining = Math.Max(0f, lockRemaining - deltaTime);
        invulnerableRemaining = Math.Max(0f, invulnerableRemaining - deltaTime);
    }

    public void Reset()
    {
        lockRemaining = 0f;
        invulnerableRemaining = 0f;
    }
}
