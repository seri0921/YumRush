using System;

public sealed class ChunkRespawnLogic
{
    private readonly bool automatic;
    private readonly float interval;
    public float RemainingSeconds { get; private set; }

    public ChunkRespawnLogic(bool automatic, float interval)
    {
        if (float.IsNaN(interval) || float.IsInfinity(interval) || interval <= 0f)
            throw new ArgumentOutOfRangeException(nameof(interval));
        this.automatic = automatic;
        this.interval = interval;
        Reset();
    }

    public bool Tick(float deltaTime)
    {
        if (float.IsNaN(deltaTime) || float.IsInfinity(deltaTime) || deltaTime < 0f)
            throw new ArgumentOutOfRangeException(nameof(deltaTime));
        if (!automatic)
            return false;
        RemainingSeconds = Math.Max(0f, RemainingSeconds - deltaTime);
        return RemainingSeconds <= 0f;
    }

    public void Reset() => RemainingSeconds = interval;
}
