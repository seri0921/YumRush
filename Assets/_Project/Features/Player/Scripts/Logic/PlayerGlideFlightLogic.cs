using System;

public readonly struct PlayerGlideVelocity
{
    public readonly float X;
    public readonly float Y;
    public float Speed => (float)Math.Sqrt(X * X + Y * Y);

    public PlayerGlideVelocity(float x, float y)
    {
        X = x;
        Y = y;
    }
}

public sealed class PlayerGlideFlightLogic
{
    private float headingDirection = 1f;
    public bool IsGliding { get; private set; }

    public void Toggle(float facingDirection = 1f)
    {
        if (!Finite(facingDirection) || facingDirection == 0f)
            throw new ArgumentOutOfRangeException(nameof(facingDirection));
        
        
        if (!IsGliding)
            headingDirection = Math.Sign(facingDirection);
        IsGliding = !IsGliding;
    }

    public void Stop() => IsGliding = false;

    public PlayerGlideVelocity Step(PlayerGlideVelocity velocity, float horizontalInput,
        float gravity, float deltaTime, float maximumSpeed,
        float liftCoefficient, float dragCoefficient, float minimumLiftSpeed,
        float pullupStartSpeed, float pullupStrength, float steerAcceleration,
        float turnLoss)
    {
        if (!IsGliding)
            throw new InvalidOperationException("Glide step requires the gliding state.");
        
        
        if (!Finite(velocity.X) || !Finite(velocity.Y) || !Finite(horizontalInput) ||
            horizontalInput < -1f || horizontalInput > 1f || !Finite(gravity) || gravity <= 0f ||
            !Finite(deltaTime) || deltaTime <= 0f || !Finite(maximumSpeed) || maximumSpeed <= 0f ||
            !Finite(liftCoefficient) || liftCoefficient < 0f ||
            !Finite(dragCoefficient) || dragCoefficient < 0f ||
            !Finite(minimumLiftSpeed) || minimumLiftSpeed < 0f ||
            !Finite(pullupStartSpeed) || pullupStartSpeed < minimumLiftSpeed ||
            !Finite(pullupStrength) || pullupStrength < 0f ||
            !Finite(steerAcceleration) || steerAcceleration < 0f ||
            !Finite(turnLoss) || turnLoss < 0f)
            throw new ArgumentOutOfRangeException(nameof(velocity));

        float x = velocity.X;
        float y = velocity.Y;
        float speed = velocity.Speed;
        float dragFactor = (float)Math.Exp(-dragCoefficient * speed * deltaTime);
        x *= dragFactor;
        y = y * dragFactor - gravity * deltaTime;

        float availableSpeed = Length(x, y);
        if (speed > minimumLiftSpeed && availableSpeed > 0.0001f)
        {
            float horizontalEfficiency = Math.Min(1f, Math.Abs(velocity.X) / speed * 2.2f);
            float pullup = Math.Min(1f, Math.Max(0f, (speed - pullupStartSpeed) / Math.Max(1f, pullupStartSpeed))) *
                           Math.Min(1f, Math.Max(0f, -velocity.Y / Math.Max(1f, pullupStartSpeed)));
            float lift = liftCoefficient * (speed * speed - minimumLiftSpeed * minimumLiftSpeed) *
                         (1f + pullup * pullupStrength) * horizontalEfficiency;
            float direction = velocity.X >= 0f ? 1f : -1f;
            float liftX = -direction * velocity.Y / speed * lift;
            float liftY = direction * velocity.X / speed * lift;
            x += liftX * deltaTime;
            y += liftY * deltaTime;
            NormalizeToAtMost(ref x, ref y, availableSpeed);
        }

        float beforeSteering = Length(x, y);
        float previousX = x;
        if (horizontalInput != 0f)
            headingDirection = Math.Sign(horizontalInput);
        float automaticSteering = horizontalInput == 0f && y < 0f && speed > minimumLiftSpeed
            ? headingDirection * Math.Min(0.35f, -y / Math.Max(1f, speed))
            : 0f;
        x += (horizontalInput + automaticSteering) * steerAcceleration * deltaTime;
        NormalizeToAtMost(ref x, ref y, beforeSteering);
        if (horizontalInput != 0f && previousX * horizontalInput < 0f)
        {
            float turnFactor = (float)Math.Exp(-turnLoss * Math.Abs(horizontalInput) * deltaTime);
            x *= turnFactor;
            y *= turnFactor;
        }

        NormalizeToAtMost(ref x, ref y, maximumSpeed);
        // With semi-implicit position integration, a velocity rotation can otherwise
        // gain energy by moving upward during the same step.
        float squaredSpeed = x * x + y * y;
        if (squaredSpeed > 0f)
        {
            float gravityStep = gravity * y * deltaTime;
            float energyBudget = speed * dragFactor;
            float maximumScale = (-gravityStep + (float)Math.Sqrt(
                                     gravityStep * gravityStep + squaredSpeed * energyBudget * energyBudget)) /
                                 squaredSpeed;
            if (maximumScale < 1f)
            {
                x *= maximumScale;
                y *= maximumScale;
            }
        }

        return new PlayerGlideVelocity(x, y);
    }

    private static float Length(float x, float y) => (float)Math.Sqrt(x * x + y * y);

    private static bool Finite(float value) => !float.IsNaN(value) && !float.IsInfinity(value);

    private static void NormalizeToAtMost(ref float x, ref float y, float maximum)
    {
        float length = Length(x, y);
        if (length <= maximum || length <= 0f)
            return;
        float scale = maximum / length;
        x *= scale;
        y *= scale;
    }
}
