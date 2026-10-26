using System;

public sealed class PlayerFanGlideLogic
{
    public bool IsActive { get; private set; }

    public void Start() => IsActive = true;
    public void Stop() => IsActive = false;

    public void Step(in PlayerFrame frame, in PlayerSettings settings, ref PlayerStepResult result)
    {
        float deltaTime = frame.DeltaTime;
        float inputX = frame.Input.HorizontalInput;
        float currentVx = result.VelocityX;
        float currentVy = result.VelocityY;

        float windX = frame.FanWind.WindVectorX;
        float windY = frame.FanWind.WindVectorY;
        float updraftMult = frame.FanWind.UpdraftMultiplier;

        float maxSpeed = settings.FanWind.MaxSpeed;
        float acceleration = settings.FanWind.Acceleration;
        float resistance = settings.FanWind.Resistance;

        float moveDirX = inputX != 0f ? Math.Sign(inputX) : Math.Sign(currentVx);
        float windDirX = Math.Sign(windX);

        float targetVx = inputX * settings.Movement.AirMaximumSpeed;

        if (windX != 0f && moveDirX != 0f)
        {
            // ‡•—–ž”¿
            if (moveDirX == windDirX)
            {
                float boostedMaxSpeed = Math.Max(maxSpeed, settings.Movement.AirMaximumSpeed + Math.Abs(windX));
                targetVx = Math.Sign(moveDirX) * boostedMaxSpeed;
                float boostedAccel = acceleration + Math.Abs(windX) * 2f;
                currentVx = MoveTowards(currentVx, targetVx, boostedAccel * deltaTime);
            }
            // ‹t•—
            else
            {
                float resistanceForce = (acceleration + Math.Abs(windX) * resistance);
                currentVx = MoveTowards(currentVx, targetVx, resistanceForce * deltaTime);
            }
        }
        else
        {
            currentVx = MoveTowards(currentVx, targetVx, acceleration * deltaTime);
        }

        float targetVy = currentVy + windY * updraftMult * deltaTime;

        if (frame.Input.JumpQueued && !result.JumpConsumed)
        {
            float baseJump = settings.Movement.JumpSpeed;

            float jumpMultiplier = windY > 0f
                ? (1f + (windY * 0.1f * updraftMult))
                : Math.Max(0.2f, 1f + (windY * 0.1f));

            targetVy = baseJump * jumpMultiplier;
            result.JumpConsumed = true; 
        }

        result.VelocityX = currentVx;
        result.VelocityY = targetVy;
    }

    private static float MoveTowards(float current, float target, float maxDelta)
    {
        float difference = target - current;
        return Math.Abs(difference) <= maxDelta
            ? target
            : current + Math.Sign(difference) * maxDelta;
    }
}
