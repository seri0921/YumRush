using System;

public sealed class PlayerFreezingLogic
{
    private float freezeTimer;
    private float knockbackTimer;
    private float knockbackX;
    private float knockbackY;
    public bool IsFreezing => freezeTimer > 0f;
    public bool IsKnockback => !IsFreezing && knockbackTimer > 0f;

    public float KnockbackX => knockbackX;
    public float KnockbackY => knockbackY;

    public void Freeze(float freezeDuration, float knockbackDuration, float dirX, float dirY)
    {
        freezeTimer = freezeDuration;
        knockbackTimer = knockbackDuration;

        float length = (float)Math.Sqrt(dirX * dirX + dirY * dirY);
        if (length > 0.001f)
        {
            knockbackX = dirX / length;
            knockbackY = dirY / length;
        }
        else
        {
            knockbackX = 0f;
            knockbackY = 0f;
        }
    }

    public void Tick(float deltaTime)
    {
        if (freezeTimer > 0f)
        {
            freezeTimer = Math.Max(0f, freezeTimer - deltaTime);
        }
        else if (knockbackTimer > 0f)
        {
            knockbackTimer = Math.Max(0f, knockbackTimer - deltaTime);
        }
    }

    public void Reset()
    {
        freezeTimer = 0f;
        knockbackTimer = 0f;
        knockbackX = 0f;
        knockbackY = 0f;
    }
}