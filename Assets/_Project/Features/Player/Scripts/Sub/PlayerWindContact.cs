using System;
using UnityEngine;

internal sealed class PlayerWindContact : IDisposable
{
    private readonly PlayerStateMachine machine;
    private readonly PlayerPhysicsMotor motor;

    private readonly PlayerMovementConfig config;

    // 気流外、または気流から切り離された動作中はnull。
    private UpdraftZone activeZone;
    public float MaximumLiftVelocity => activeZone == null ? 0f : activeZone.Amplitude * 2f * Mathf.PI / activeZone.Period;

    public PlayerWindContact(PlayerStateMachine machine, PlayerPhysicsMotor motor, PlayerMovementConfig config)
    {
        this.machine = machine ?? throw new ArgumentNullException(nameof(machine));
        this.motor = motor ?? throw new ArgumentNullException(nameof(motor));
        this.config = config != null ? config : throw new ArgumentNullException(nameof(config));
        
        machine.StateChanged += OnStateChanged;
    }

    public void Enter(Collider2D other)
    {
        var zone = other.GetComponentInParent<UpdraftZone>();
        if (zone == null || !machine.CanEnterWind)
            return;
        if (!machine.TryEnterWind())
            return;
        activeZone = zone;
        Vector2 velocity = motor.Velocity;
        velocity.x = 0f;
        velocity.y = Math.Max(velocity.y, -config.EntryMaxFallSpeed);
        motor.ApplyVelocity(velocity);
        motor.SetGravityScale(0f);
    }

    public void Exit(Collider2D other)
    {
        if (activeZone == null || other.GetComponentInParent<UpdraftZone>() != activeZone)
            return;
        activeZone = null;
        machine.LeaveWind(motor.IsGrounded(out bool isIce) && motor.Velocity.y <= 0f);
        motor.RestoreGravity();
    }

    public void Sample(ref PlayerFrame frame)
    {
        if (activeZone == null && machine.IsInWind)
        {
            machine.LeaveWind(frame.Body.Grounded); // 気流の破棄にも追従する。
            motor.RestoreGravity();
        }

        if (activeZone != null && !activeZone.isActiveAndEnabled)
        {
            activeZone = null;
            machine.LeaveWind(frame.Body.Grounded);
            motor.RestoreGravity();
        }

        frame.Wind.HasWind = activeZone != null;
        if (!frame.Wind.HasWind)
            return;
        frame.Wind.CenterX = activeZone.CenterX;
        frame.Wind.CenterY = activeZone.CenterY;
        frame.Wind.Period = activeZone.Period;
        frame.Wind.Amplitude = activeZone.Amplitude;
        frame.Wind.BoostMultiplier = activeZone.BoostMultiplier;
        frame.Wind.CaptureSpeed = activeZone.CaptureSpeed;
        frame.Wind.CaptureAcceleration = activeZone.CaptureAcceleration;
        frame.Wind.CaptureArrivalDistance = activeZone.CaptureArrivalDistance;
    }

    private void OnStateChanged(PlayerState previous, PlayerState current)
    {
        if (!machine.IsInWind)
            activeZone = null;
    }

    public void Dispose() => machine.StateChanged -= OnStateChanged;
}
