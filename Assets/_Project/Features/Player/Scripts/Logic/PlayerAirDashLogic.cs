using System;

public sealed class PlayerAirDashLogic
{
    private float remainingTime;

    public bool IsDashing => remainingTime > 0f;
    public bool IsAvailable { get; private set; } = true;

    public bool TryStart(bool isAirborne, float duration)
    {
        if (float.IsNaN(duration) || float.IsInfinity(duration) || duration <= 0f)
            throw new ArgumentOutOfRangeException(nameof(duration));
        
        
        if (!IsAvailable || !isAirborne)
            return false;

        IsAvailable = false;
        remainingTime = duration;
        return true;
    }

    public void Tick(float deltaTime)
    {
        if (float.IsNaN(deltaTime) || float.IsInfinity(deltaTime) || deltaTime < 0f)
            throw new ArgumentOutOfRangeException(nameof(deltaTime));
        
        
        remainingTime = Math.Max(0f, remainingTime - deltaTime);
    }

    public void Recharge()
    {
        IsAvailable = true;
        remainingTime = 0f;
    }

    public void Cancel() => remainingTime = 0f;
}
