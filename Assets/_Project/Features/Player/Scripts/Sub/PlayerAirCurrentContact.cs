using System;
using System.Collections.Generic;
using UnityEngine;

// 接触を被弾中も保持し、操作制限の終了後に範囲内で再捕捉する。
internal sealed class PlayerAirCurrentContact
{
    private readonly List<Contact> contacts = new List<Contact>();
    private readonly PlayerStateMachine machine;
    private readonly PlayerPhysicsMotor motor;
    // 捕捉していない間はnull。気流の切り替わりを検出するために保持する。
    private AirCurrentZone activeZone;

    public PlayerAirCurrentContact(PlayerStateMachine machine, PlayerPhysicsMotor motor)
    {
        this.machine = machine ?? throw new ArgumentNullException(nameof(machine));
        this.motor = motor ?? throw new ArgumentNullException(nameof(motor));
    }

    public void Enter(Collider2D other)
    {
        var zone = other.GetComponentInParent<AirCurrentZone>();
        if (zone == null)
            return; // 通常のトリガーは対象外。
        for (int i = 0; i < contacts.Count; i++)
            if (contacts[i].Collider == other)
                return;
        contacts.Add(new Contact { Collider = other, Zone = zone });
    }

    public void Exit(Collider2D other)
    {
        for (int i = contacts.Count - 1; i >= 0; i--)
            if (contacts[i].Collider == other)
                contacts.RemoveAt(i);
    }
    public void Clear()
    {
        contacts.Clear();
        activeZone = null;
    }

    public void Sample(ref PlayerFrame frame)
    {
        AirCurrentZone selected = null; // 有効な接触がなければnull。
        for (int i = contacts.Count - 1; i >= 0; i--)
        {
            Collider2D contact = contacts[i].Collider;
            if (contact == null || !contact.enabled || !contact.gameObject.activeInHierarchy)
            {
                contacts.RemoveAt(i);
                continue;
            }
            var zone = contacts[i].Zone;
            if (zone != null && zone.IsAvailable && selected == null)
                selected = zone;
        }

        if (motor.IsScriptedMovement)
        {
            activeZone = null;
            return;
        }
        bool entering = selected != activeZone || machine.State != PlayerState.AirCurrent;
        if (selected != null && machine.CanEnterAirCurrent)
            machine.ChangeState(PlayerState.AirCurrent);
        else if (selected == null && machine.State == PlayerState.AirCurrent)
            machine.ChangeState(frame.Body.Grounded && frame.Body.VelocityY <= 0f ? PlayerState.Grounded : PlayerState.Airborne);

        if (selected == null || machine.State != PlayerState.AirCurrent)
        {
            activeZone = null;
            return;
        }
        activeZone = selected;
        Vector2 direction = selected.Direction;
        Vector2 center = selected.Center;
        frame.AirCurrent = new PlayerAirCurrentSnapshot
        {
            HasCurrent = true, DirectionX = direction.x, DirectionY = direction.y,
            Acceleration = selected.Acceleration, MaximumSpeed = selected.MaximumSpeed,
            ResetLateralVelocity = entering, CenterX = center.x, CenterY = center.y,
            CaptureSpeed = selected.CaptureSpeed, CaptureAcceleration = selected.CaptureAcceleration
        };
    }

    private struct Contact
    {
        public Collider2D Collider;
        public AirCurrentZone Zone;
    }
}
