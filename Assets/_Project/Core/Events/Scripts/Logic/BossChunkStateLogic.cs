using System;

/// <summary>Tracks a boss event's lifetime independently of its terrain's lifetime.</summary>
public sealed class BossChunkStateLogic
{
    public bool IsAvailable => !IsShowing;
    public bool IsShowing { get; private set; }
    public bool IsWaitingForRestore { get; private set; }

    public void Begin()
    {
        if (!IsAvailable)
            throw new InvalidOperationException("Restore the previous boss Chunk before starting another event.");
        IsShowing = true;
    }

    public void EndEvent()
    {
        if (IsShowing) IsWaitingForRestore = true;
    }

    public bool TryRestore(int bossLocation, int playerLocation)
    {
        if (!IsWaitingForRestore || playerLocation < 0 || playerLocation == bossLocation)
            return false;
        Reset();
        return true;
    }

    private void Reset()
    {
        IsShowing = false;
        IsWaitingForRestore = false;
    }
}
