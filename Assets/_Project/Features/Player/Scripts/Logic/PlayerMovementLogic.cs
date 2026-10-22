using System;

public static class PlayerMovementLogic
{
    public static float StepHorizontal(float velocity, float input, bool grounded,
        float groundSpeed, float airMaximumSpeed, float airAcceleration,
        float airDamping, float deltaTime)
    {
        // 不正チェック
        ValidateFinite(velocity, nameof(velocity));
        ValidateFinite(input, nameof(input));
        ValidatePositive(groundSpeed, nameof(groundSpeed));
        ValidatePositive(airMaximumSpeed, nameof(airMaximumSpeed));
        ValidatePositive(airAcceleration, nameof(airAcceleration));
        ValidateFinite(airDamping, nameof(airDamping));
        if (airDamping < 0f)
            throw new ArgumentOutOfRangeException(nameof(airDamping));
        ValidatePositive(deltaTime, nameof(deltaTime));
        ///////////////////////////////////////////////////////////////

        input = Math.Max(-1f, Math.Min(1f, input));
        float target = input * (grounded ? groundSpeed : airMaximumSpeed);
        velocity = MoveTowards(velocity, target,
            (grounded ? groundSpeed * 20f : airAcceleration) * deltaTime);
        return !grounded && input == 0f
            ? velocity * (float)Math.Exp(-airDamping * deltaTime)
            : velocity;
    }

    public static float StepWindFollow(float velocity, float position, float target,
        float maximumSpeed, float acceleration, float deltaTime)
    {
        // 不正チェック
        ValidateFinite(velocity, nameof(velocity));
        ValidateFinite(position, nameof(position));
        ValidateFinite(target, nameof(target));
        ValidatePositive(maximumSpeed, nameof(maximumSpeed));
        ValidatePositive(acceleration, nameof(acceleration));
        ValidatePositive(deltaTime, nameof(deltaTime));
        ///////////////////////////////////////////////////////////////////////////////
        
        float desired = Math.Max(-maximumSpeed, Math.Min(maximumSpeed, (target - position) * 3f));
        return MoveTowards(velocity, desired, acceleration * deltaTime);
    }

    private static float MoveTowards(float current, float target, float maximumDelta)
    {
        float difference = target - current;
        return Math.Abs(difference) <= maximumDelta
            ? target
            : current + Math.Sign(difference) * maximumDelta;
    }

    //不正チェック用のメソッド
    private static void ValidatePositive(float value, string name)
    {
        ValidateFinite(value, name);
        if (value <= 0f)
            throw new ArgumentOutOfRangeException(name);
    }

    private static void ValidateFinite(float value, string name)
    {
        if (float.IsNaN(value) || float.IsInfinity(value))
            throw new ArgumentOutOfRangeException(name);
    }
}