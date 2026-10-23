using System;

public sealed class ScoreLogic
{
    public int Current { get; private set; }
    public event Action<int> Changed;

    public void Add(int points)
    {
        if (points < 0)
            throw new ArgumentOutOfRangeException(nameof(points));
        Current = checked(Current + points);
        Changed?.Invoke(Current);
    }

    public void Reset()
    {
        Current = 0;
        Changed?.Invoke(Current);
    }
}
