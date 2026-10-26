using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public sealed class PlayerFanWindContact
{
    private readonly PlayerStateMachine machine;
    private readonly List<IFanField> activeFields = new();
    public PlayerFanWindContact(PlayerStateMachine machine)
    {
        this.machine = machine ?? throw new ArgumentNullException(nameof(machine));
    }

    public void Enter(Collider2D other)
    {
        var field = other.GetComponentInParent<IFanField>();
        if (field != null && !activeFields.Contains(field))
        {
            activeFields.Add(field);
        }
    }

    public void Exit(Collider2D other)
    {
        var field = other.GetComponentInParent<IFanField>();
        if (field != null)
        {
            activeFields.Remove(field);
        }
    }

    public void Sample(ref PlayerFrame frame)
    {
        IFanField currentField = activeFields
            .Where(f => f != null)
            .OrderByDescending(f => f.Priority)
            .FirstOrDefault();

        frame.FanWind.HasFanWind = currentField != null;
        if (currentField != null)
        {
            Vector2 v = currentField.WindVector;
            frame.FanWind.WindVectorX = v.x;
            frame.FanWind.WindVectorY = v.y;
            frame.FanWind.UpdraftMultiplier = currentField.UpdraftMultiplier;
        }
        else
        {
            frame.FanWind.WindVectorX = 0f;
            frame.FanWind.WindVectorY = 0f;
            frame.FanWind.UpdraftMultiplier = 1f;
        }
    }

    public void Clear()
    {
        activeFields.Clear();
    }
}
