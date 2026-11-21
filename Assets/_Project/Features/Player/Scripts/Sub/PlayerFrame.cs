// データのコンテナ

public class PlayerFrame
{
    public float DeltaTime;
    public PlayerInputSnapshot Input;
    public PlayerBodySnapshot Body;
    public PlayerWindSnapshot Wind;
    public PlayerAirCurrentSnapshot AirCurrent;
    public PlayerHomingSnapshot Homing;
    public PlayerFanWindSnapshot FanWind;
}

public struct PlayerSettings
{
    public PlayerMovementSettings Movement;
    public PlayerWindSettings Wind;
    public PlayerDashSettings Dash;
    public PlayerGlideSettings Glide;
    public PlayerHomingSettings Homing;
    public PlayerFanWindSettings FanWind;
    public PlayerIceSettings Ice;
    public PlayerFreezeSettings Freeze;
}

public struct PlayerInputSnapshot
{
    public float HorizontalInput, VerticalInput;
    public int FacingDirection;
    public bool JumpQueued, AttackQueued, DashQueued;
}

public struct PlayerBodySnapshot
{
    public float VelocityX, VelocityY, PositionX, PositionY, Gravity;
    public bool Grounded;
    public bool IsIce;
}

public struct PlayerWindSnapshot
{
    public bool HasWind;
    public float CenterX, CenterY, Period, Amplitude, BoostMultiplier, CaptureSpeed, CaptureAcceleration, CaptureArrivalDistance;
}

public struct PlayerAirCurrentSnapshot
{
    public bool HasCurrent;
    public bool ResetLateralVelocity;
    public float DirectionX, DirectionY, Acceleration, MaximumSpeed;
    public float CenterX, CenterY, CaptureSpeed, CaptureAcceleration;
}

public struct PlayerHomingSnapshot
{
    public bool HasHomingTarget, TouchingHomingTarget, WindupComplete;
    public float HomingDirectionX, HomingDirectionY;
}

public struct PlayerFanWindSnapshot
{
    public bool HasFanWind;
    public float WindVectorX;
    public float WindVectorY;
    public float UpdraftMultiplier;
}


public struct PlayerMovementSettings
{
    public float GroundSpeed, AirMaximumSpeed, AirAcceleration, AirDamping, JumpSpeed;
}

public struct PlayerWindSettings
{
    public float RecaptureHeight, WindMaximumSpeed, WindAcceleration;
}

public struct PlayerDashSettings
{
    public float AirDashDuration, AirDashSpeed;
}

public struct PlayerGlideSettings
{
    public float GlideMaximumSpeed, GlideLiftCoefficient, GlideDragCoefficient, GlideMinimumLiftSpeed, GlidePullupStartSpeed, GlidePullupStrength, GlideSteerAcceleration, GlideTurnLoss;
}

public struct PlayerHomingSettings
{
    public float HomingSpeed, SuccessUpwardSpeed, MaximumDuration;
}

public struct PlayerFanWindSettings
{
    public float MaxSpeed;
    public float Acceleration;
    public float Resistance;
}

public struct PlayerIceSettings
{
    public float GroundSpeed;
    public float Acceleration;
    public float MaxDeceleration;
    public float FrictionFactor;
}

public struct PlayerStepResult
{
    public float VelocityX, VelocityY;
    public bool SuppressGravity, ApplyHomingAttack;
    public bool HomingWindupCompleted;
    public bool GroundJumpStarted;
    public bool JumpConsumed, DashConsumed;
}

public struct PlayerFreezeSettings
{
    public float FreezeDuration;
    public float KnockbackSpeed;
    public float KnockbackDuration;
}
