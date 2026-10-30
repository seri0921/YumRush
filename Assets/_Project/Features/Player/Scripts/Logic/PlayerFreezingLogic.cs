using UnityEngine;

public sealed class PlayerFreezingLogic
{
    private float freezeTimer;

    public bool IsFreezing => freezeTimer > 0f;

    public void Freeze(float duration)
    {
        freezeTimer = duration;
    }

    public void Tick(float deltaTime)
    {
        if (freezeTimer > 0f)
        {
            freezeTimer -= deltaTime;
            if (freezeTimer < 0f)
                freezeTimer = 0f;
        }
    }

    public void Reset()
    {
        freezeTimer = 0f;
    }
}