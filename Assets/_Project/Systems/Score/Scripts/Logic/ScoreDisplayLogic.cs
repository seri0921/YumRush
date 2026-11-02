using System;

public sealed class ScoreDisplayLogic
{
    public double HideThresholdSeconds { get; }

    public ScoreDisplayLogic(double hideThresholdSeconds)
    {
        if (double.IsNaN(hideThresholdSeconds) || double.IsInfinity(hideThresholdSeconds) || hideThresholdSeconds < 0d)
            throw new ArgumentOutOfRangeException(nameof(hideThresholdSeconds));

        HideThresholdSeconds = hideThresholdSeconds;
    }

    public bool ShouldHide(double remainingSeconds) => remainingSeconds <= HideThresholdSeconds;
}
