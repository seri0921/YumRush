using System;

public readonly struct UpdraftTimingPresentation
{
    public readonly bool Visible;
    public readonly bool Ready;
    public readonly bool Optimal;
    public readonly float Fill;

    public UpdraftTimingPresentation(bool visible, bool ready, bool optimal, float fill)
    {
        Visible = visible;
        Ready = ready;
        Optimal = optimal;
        Fill = fill;
    }
}

public sealed class UIUpdraftTimingLogic
{
    private readonly float optimalThreshold;

    public UIUpdraftTimingLogic(float optimalThreshold)
    {
        if (optimalThreshold <= 0f || optimalThreshold > 1f ||
            float.IsNaN(optimalThreshold))
            throw new ArgumentOutOfRangeException(nameof(optimalThreshold));
        this.optimalThreshold = optimalThreshold;
    }

    public UpdraftTimingPresentation Evaluate(PlayerState state,
        float liftVelocity, float maximumLiftVelocity, bool canJump)
    {
        if (float.IsNaN(liftVelocity) || float.IsInfinity(liftVelocity) ||
            float.IsNaN(maximumLiftVelocity) || float.IsInfinity(maximumLiftVelocity))
            throw new ArgumentOutOfRangeException(nameof(liftVelocity));

        bool visible = state == PlayerState.Capturing ||
                       state == PlayerState.Floating;
        if (!visible)
            return new UpdraftTimingPresentation(false, false, false, 0f);
        if (maximumLiftVelocity <= 0f)
            throw new ArgumentOutOfRangeException(nameof(maximumLiftVelocity));

        float fill = Math.Clamp(liftVelocity / maximumLiftVelocity, 0f, 1f);
        bool ready = state == PlayerState.Floating && canJump;
        return new UpdraftTimingPresentation(true, ready,
            ready && fill >= optimalThreshold, fill);
    }
}