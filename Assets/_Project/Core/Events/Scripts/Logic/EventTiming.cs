using System;

/// <summary>Immutable runtime settings, independent of Unity assets.</summary>
public readonly struct EventTiming
{
    public StageEventType Type { get; }
    public double StartSeconds { get; }
    public double EndSeconds { get; }

    public EventTiming(StageEventType type, double startSeconds, double durationSeconds)
    {
        if (!Enum.IsDefined(typeof(StageEventType), type))
            throw new ArgumentOutOfRangeException(nameof(type));
        if (double.IsNaN(startSeconds) || double.IsInfinity(startSeconds) || startSeconds < 0d)
            throw new ArgumentOutOfRangeException(nameof(startSeconds));
        if (double.IsNaN(durationSeconds) || double.IsInfinity(durationSeconds) || durationSeconds <= 0d ||
            double.IsInfinity(startSeconds + durationSeconds) || startSeconds + durationSeconds <= startSeconds)
            throw new ArgumentOutOfRangeException(nameof(durationSeconds));
        Type = type;
        StartSeconds = startSeconds;
        EndSeconds = startSeconds + durationSeconds;
    }
}
