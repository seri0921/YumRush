using System;

public sealed class EnemyFireLogic
{
    private float elapsed;
    private bool wasInRange;

    public bool Tick(float deltaTime, float distanceSquared, float range, float interval)
    {
        if (deltaTime < 0f || float.IsNaN(deltaTime) || range <= 0f ||
            interval <= 0f || float.IsNaN(range) || float.IsNaN(interval))
            throw new ArgumentOutOfRangeException(nameof(deltaTime));

        bool inRange = distanceSquared <= range * range;
        if (!inRange)
        {
            elapsed = 0f;
            wasInRange = false;
            return false;
        }

        if (!wasInRange)
        {
            wasInRange = true;
            elapsed = 0f;
        }

        elapsed += deltaTime;
        if (elapsed < interval)
            return false;
        elapsed %= interval;
        return true;
    }
}
