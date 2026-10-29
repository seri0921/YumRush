using System;

public static class PlayerIceGroundLogic
{
    public static float StepHorizontal(
        float currentVelocityX,
        float horizontalInput,
        in PlayerIceSettings settings,
        float deltaTime)
    {
        if (horizontalInput != 0f)
        {
            float targetVelocity = Math.Sign(horizontalInput) * settings.GroundSpeed;
            return MoveTowards(currentVelocityX, targetVelocity, settings.Acceleration * deltaTime);
        }

        float speed = Math.Abs(currentVelocityX);
        if (speed < 0.01f)
            return 0f;

        float quadraticDeceleration = speed * speed * settings.FrictionFactor;

        float appliedDeceleration = Math.Min(quadraticDeceleration, settings.MaxDeceleration);

        appliedDeceleration = Math.Max(appliedDeceleration, 0.5f);

        return MoveTowards(currentVelocityX, 0f, appliedDeceleration * deltaTime);
    }

    private static float MoveTowards(float current, float target, float maxDelta)
    {
        float difference = target - current;
        return Math.Abs(difference) <= maxDelta
            ? target
            : current + Math.Sign(difference) * maxDelta;
    }
}
