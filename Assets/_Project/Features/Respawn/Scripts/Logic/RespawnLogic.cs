using System;

/// <summary>Runtime state for one slot; independent of Unity and shared assets.</summary>
public sealed class RespawnLogic
{
    public bool IsAlive { get; private set; } = true;
    public float RemainingSeconds { get; private set; }
    private readonly bool automatic;
    private readonly float delay;

    public RespawnLogic(bool automatic, float delay)
    {
        ValidateTime(delay);
        this.automatic = automatic;
        this.delay = delay;
    }

    public void MarkConsumed()
    {
        if (!IsAlive)
            throw new InvalidOperationException("The spawn slot is already waiting.");
        IsAlive = false;
        RemainingSeconds = delay;
    }

    public bool Tick(float deltaTime)
    {
        ValidateTime(deltaTime);
        if (IsAlive || !automatic)
            return false;
        RemainingSeconds = Math.Max(0f, RemainingSeconds - deltaTime);
        return RemainingSeconds <= 0f;
    }

    public void MarkSpawned()
    {
        if (IsAlive)
            throw new InvalidOperationException("The spawn slot is already occupied.");
        IsAlive = true;
        RemainingSeconds = 0f;
    }

    private static void ValidateTime(float seconds)
    {
        if (float.IsNaN(seconds) || float.IsInfinity(seconds) || seconds < 0f)
            throw new ArgumentOutOfRangeException(nameof(seconds));
    }
}
