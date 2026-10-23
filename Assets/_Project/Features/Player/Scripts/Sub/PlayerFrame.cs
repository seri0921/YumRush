// データのコンテナ

public class PlayerFrame
{
    public float DeltaTime;
    public PlayerInputSnapshot Input;
    public PlayerBodySnapshot Body;
    public PlayerWindSnapshot Wind;
    public PlayerHomingSnapshot Homing;
}

public struct PlayerSettings
{
    public PlayerMovementSettings Movement;
    public PlayerWindSettings Wind;
    public PlayerDashSettings Dash;
    public PlayerGlideSettings Glide;
    public PlayerHomingSettings Homing;
}

public struct PlayerInputSnapshot
{
    public float HorizontalInput, VerticalInput;
    public int FacingDirection;
    public bool JumpQueued, DashQueued;
}

public struct PlayerBodySnapshot
{
    public float VelocityX, VelocityY, PositionX, PositionY, Gravity;
    public bool Grounded;
}

public struct PlayerWindSnapshot
{
    public bool HasWind;
    public float CenterX, CenterY, Period, Amplitude, BoostMultiplier, CaptureSpeed, CaptureAcceleration, CaptureArrivalDistance;
}

public struct PlayerHomingSnapshot
{
    public bool HasHomingTarget, TouchingHomingTarget, WindupComplete;
    public float HomingDirectionX, HomingDirectionY;
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
    public float HomingSpeed, SuccessUpwardSpeed;
}

public struct PlayerStepResult
{
    public float VelocityX, VelocityY;
    public bool SuppressGravity, ApplyHomingAttack;
    public bool JumpConsumed, DashConsumed;
}
