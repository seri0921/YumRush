using System;

public static class KeyEvents
{
    public static event Action<TreasureKey> OnKeySpawned;
    public static event Action<TreasureKey> OnKeyDestroyed;

    public static void ReportSpawned(TreasureKey key) => OnKeySpawned?.Invoke(key);
    public static void ReportDestroyed(TreasureKey key) => OnKeyDestroyed?.Invoke(key);
}