using System;

public static class PlayerAirCurrentLogic
{
    public static void Step(in PlayerAirCurrentSnapshot current, float positionX, float positionY,
        float deltaTime, ref float velocityX, ref float velocityY)
    {
        float lateralX = current.DirectionY;
        float lateralY = -current.DirectionX;
        float previousLateralSpeed = velocityX * lateralX + velocityY * lateralY;
        float offset = (current.CenterX - positionX) * lateralX +
                       (current.CenterY - positionY) * lateralY;
        float lateralSpeed = StepCapture(offset,
            current.ResetLateralVelocity ? 0f : previousLateralSpeed,
            current.CaptureSpeed, current.CaptureAcceleration, deltaTime);
        velocityX += lateralX * (lateralSpeed - previousLateralSpeed);
        velocityY += lateralY * (lateralSpeed - previousLateralSpeed);
        PlayerAirCurrentLogic.Step(current.DirectionX, current.DirectionY,
            current.Acceleration, current.MaximumSpeed, deltaTime,
            ref velocityX, ref velocityY);
    }

    public static void Step(float directionX, float directionY, float acceleration,
        float maximumSpeed, float deltaTime, ref float velocityX, ref float velocityY)
    {
        if (!IsFinite(directionX) || !IsFinite(directionY) ||
            Math.Abs(directionX * directionX + directionY * directionY - 1f) > 0.001f)
            throw new ArgumentException("Air current direction must be normalized.");
        if (!IsFinite(acceleration) || acceleration <= 0f || !IsFinite(maximumSpeed) || maximumSpeed <= 0f ||
            !IsFinite(deltaTime) || deltaTime < 0f || !IsFinite(velocityX) || !IsFinite(velocityY))
            throw new ArgumentOutOfRangeException(nameof(acceleration), "Air current values must be finite and within range.");

        float speed = velocityX * directionX + velocityY * directionY;
        float increase = Math.Min(acceleration * deltaTime, Math.Max(0f, maximumSpeed - speed));
        velocityX += directionX * increase;
        velocityY += directionY * increase;
    }

    public static float StepCapture(float offset, float velocity, float maximumSpeed, float acceleration, float deltaTime)
    {
        if (!IsFinite(offset) || !IsFinite(velocity) || !IsFinite(maximumSpeed) || maximumSpeed <= 0f ||
            !IsFinite(acceleration) || acceleration <= 0f || !IsFinite(deltaTime) || deltaTime < 0f)
            throw new ArgumentOutOfRangeException(nameof(offset), "Capture values must be finite and within range.");
        if (deltaTime == 0f)
            return velocity;

        float desiredSpeed = Math.Min(maximumSpeed, (float)Math.Sqrt(2.0 * acceleration * Math.Abs(offset)));
        float target = Math.Sign(offset) * desiredSpeed;
        float change = target - velocity;
        float next = velocity + Math.Sign(change) * Math.Min(Math.Abs(change), acceleration * deltaTime);
        // このステップで中心線を越える場合は、ちょうど中心線に到達させる。
        if (next * offset >= 0f && Math.Abs(next * deltaTime) >= Math.Abs(offset))
            return offset / deltaTime;
        return next;
    }

    private static bool IsFinite(float value) => !float.IsNaN(value) && !float.IsInfinity(value);
}
