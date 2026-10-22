
public sealed class EnemyDefeatLogic
{
    public bool IsDefeated { get; private set; }

    public bool TryDefeat()
    {
        if (IsDefeated)
            return false;
        IsDefeated = true;
        return true;
    }
}
