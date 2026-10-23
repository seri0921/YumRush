using System;

/// <summary>Session time in seconds. The caller chooses the clock and when to advance it.</summary>
public sealed class GameTimeLogic
{
    public double DurationSeconds { get; private set; }
    public double ElapsedSeconds { get; private set; }
    public double RemainingSeconds => Math.Max(0d, DurationSeconds - ElapsedSeconds);
    public bool IsExpired => RemainingSeconds <= 0d;
    public event Action<double> Advanced;

    public void Reset(double durationSeconds)
    {
        if (double.IsNaN(durationSeconds) || double.IsInfinity(durationSeconds) || durationSeconds <= 0d)
            throw new ArgumentOutOfRangeException(nameof(durationSeconds));

        DurationSeconds = durationSeconds;
        ElapsedSeconds = 0d;
    }

    public void Advance(double deltaSeconds)
    {
        if (double.IsNaN(deltaSeconds) || double.IsInfinity(deltaSeconds) || deltaSeconds < 0d)
            throw new ArgumentOutOfRangeException(nameof(deltaSeconds));
        if (deltaSeconds == 0d || IsExpired)
            return;

        ElapsedSeconds = Math.Min(DurationSeconds, ElapsedSeconds + deltaSeconds);
        Advanced?.Invoke(ElapsedSeconds);
    }
}
