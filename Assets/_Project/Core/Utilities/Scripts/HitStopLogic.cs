using System;

// Tracks a real-time stop duration without depending on Unity's clock.
public sealed class HitStopLogic
{
    public double RemainingSeconds { get; private set; }
    public bool IsActive => RemainingSeconds > 0d;

    public void Request(double durationSeconds)
    {
        if (double.IsNaN(durationSeconds) || double.IsInfinity(durationSeconds) || durationSeconds < 0d)
            throw new ArgumentOutOfRangeException(nameof(durationSeconds));

        // Overlapping requests keep the longer remaining stop instead of accumulating.
        RemainingSeconds = Math.Max(RemainingSeconds, durationSeconds);
    }

    public void Tick(double unscaledDeltaSeconds)
    {
        if (double.IsNaN(unscaledDeltaSeconds) || double.IsInfinity(unscaledDeltaSeconds) || unscaledDeltaSeconds < 0d)
            throw new ArgumentOutOfRangeException(nameof(unscaledDeltaSeconds));

        RemainingSeconds = Math.Max(0d, RemainingSeconds - unscaledDeltaSeconds);
    }

    public void Clear() => RemainingSeconds = 0d;
}
