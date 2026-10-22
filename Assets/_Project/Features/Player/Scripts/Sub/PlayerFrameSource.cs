using System;
using UnityEngine;

internal sealed class PlayerFrameSource
{
    private readonly PlayerMovementConfig config;
    private readonly PlayerPhysicsMotor motor;
    private readonly PlayerInput input;

    public PlayerFrameSource(PlayerMovementConfig config, PlayerPhysicsMotor motor, PlayerInput input)
    {
        this.config = config != null ? config : throw new ArgumentNullException(nameof(config));
        this.motor = motor ?? throw new ArgumentNullException(nameof(motor));
        this.input = input ?? throw new ArgumentNullException(nameof(input));
    }

    public PlayerFrame Sample(float deltaTime, int facingDirection)
    {
        Vector2 velocity = motor.Velocity;
        return new PlayerFrame
        {
            DeltaTime = deltaTime,
            Input = new PlayerInputSnapshot
            {
                HorizontalInput = Mathf.Clamp(input.Direction.x, -1f, 1f),
                VerticalInput = input.Direction.y,
                FacingDirection = facingDirection,
                JumpQueued = input.JumpQueued,
                DashQueued = input.DashQueued
            },
            Body = new PlayerBodySnapshot
            {
                VelocityX = velocity.x,
                VelocityY = velocity.y,
                PositionX = motor.Position.x,
                PositionY = motor.Position.y,
                Grounded = motor.IsGrounded() && velocity.y <= 0f,
                Gravity = -Physics2D.gravity.y * motor.NormalGravityScale
            }
        };
    }

    // 毎Tick変換し、実行中のInspector調整を従来どおり反映する。
    public PlayerSettings SampleSettings() =>
        new PlayerSettings
        {
            Movement = new PlayerMovementSettings
            {
                GroundSpeed = config.MoveSpeed,
                AirMaximumSpeed = config.AirMaximumSpeed,
                AirAcceleration = config.AirAcceleration,
                AirDamping = config.AirDamping,
                JumpSpeed = config.JumpSpeed
            },
            Wind = new PlayerWindSettings { RecaptureHeight = config.RecaptureHeight, WindMaximumSpeed = config.WindMaxSpeed, WindAcceleration = config.WindAcceleration },
            Dash = new PlayerDashSettings { AirDashSpeed = config.AirDashSpeed, AirDashDuration = config.AirDashDuration },
            Glide = new PlayerGlideSettings
            {
                GlideMaximumSpeed = config.GlideMaximumSpeed,
                GlideLiftCoefficient = config.GlideLiftCoefficient,
                GlideDragCoefficient = config.GlideDragCoefficient,
                GlideMinimumLiftSpeed = config.GlideMinimumLiftSpeed,
                GlidePullupStartSpeed = config.GlidePullupStartSpeed,
                GlidePullupStrength = config.GlidePullupStrength,
                GlideSteerAcceleration = config.GlideSteerAcceleration,
                GlideTurnLoss = config.GlideTurnLoss
            },
            Homing = new PlayerHomingSettings { HomingSpeed = config.HomingSpeed, SuccessUpwardSpeed = config.SuccessUpwardSpeed }
        };
}