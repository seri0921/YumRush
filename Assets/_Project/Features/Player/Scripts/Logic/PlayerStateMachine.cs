using System;


public enum PlayerState
{
    Grounded,
    Airborne,
    Capturing,
    Floating,
    WindJump,
    Gliding,
    AirDashing,
    Homing,
    Hit,
    HomingWindup,
    AirCurrent
}

public interface IPlayerState
{
    PlayerState Id { get; }
    void Enter();
    void Tick(in PlayerFrame frame, in PlayerSettings settings, ref PlayerStepResult result);
    void Exit();
}

// プレイヤーの動作状態はここだけで保持する。同じStateへの遷移は再入場しない。
public sealed class PlayerStateMachine
{
    private readonly IPlayerState[] states;
    private readonly PlayerUpdraftLogic wind;
    private readonly PlayerGlideFlightLogic glide = new PlayerGlideFlightLogic();
    private readonly PlayerAirDashLogic dash = new PlayerAirDashLogic();
    private readonly PlayerHitLogic hit = new PlayerHitLogic();
    private bool transitioning;
    private float dashX, dashY;
    private int facing = 1;

    public IPlayerState Current { get; private set; }
    public PlayerState State => Current.Id;
    public bool IsActionLocked => State == PlayerState.Hit || IsHoming || State == PlayerState.AirCurrent;
    public bool IsHoming => State == PlayerState.Homing || State == PlayerState.HomingWindup;
    public bool IsGliding => State == PlayerState.Gliding;
    public bool CanWindJump => State == PlayerState.Floating && wind.CanWindJump;
    public float LiftVelocity => wind.LiftVelocity;
    public bool CanEnterWind => !IsHoming && State != PlayerState.Hit && State != PlayerState.AirCurrent;
    public bool CanEnterAirCurrent => State != PlayerState.Hit;

    public bool IsInWind =>
        State == PlayerState.Capturing || State == PlayerState.Floating ||
        State == PlayerState.WindJump;
    
    public bool IsInvulnerable() => hit.IsInvulnerable || IsHoming;

    public event Action<PlayerState, PlayerState> StateChanged;

    public PlayerStateMachine() : this(new PlayerUpdraftLogic())
    {
    }

    public PlayerStateMachine(PlayerUpdraftLogic wind)
    {
        this.wind = wind ?? throw new ArgumentNullException(nameof(wind));

        states = new IPlayerState[] { new GroundedState(this), new AirborneState(this), new CapturingState(this, wind), new FloatingState(this, wind), new WindJumpState(this, wind), new GlidingState(this, glide), new AirDashingState(this, dash), new HomingState(this), new HitState(this, hit), new HomingWindupState(this), new AirCurrentState(this) };
        Current = states[(int)PlayerState.Airborne];
        Current.Enter();
    }

    public void ChangeState(PlayerState id)
    {
        if ((uint)id >= (uint)states.Length)
            throw new ArgumentOutOfRangeException(nameof(id));

        ChangeState(states[(int)id]);
    }

    // ライフサイクルの検証にも利用する。ゲーム側は要求API経由で遷移する。
    public void ChangeState(IPlayerState next)
    {
        if (next == null)
            throw new ArgumentNullException(nameof(next));
        if (transitioning)
            throw new InvalidOperationException("Recursive state transitions are not supported.");

        if (ReferenceEquals(Current, next))
            return;
        transitioning = true;
        try
        {
            PlayerState previous = State;
            Current.Exit();
            Current = next;
            Current.Enter();
            if (IsWindState(previous) && !IsInWind)
                wind.LeaveWind();
            StateChanged?.Invoke(previous, State);
        }
        finally
        {
            transitioning = false;
        }
    }

    private static bool IsWindState(PlayerState state) =>
        state == PlayerState.Capturing ||
        state == PlayerState.Floating || state == PlayerState.WindJump;

    public bool TryEnterWind(float phase = 0f)
    {
        if (!CanEnterWind)
            return false;
        wind.EnterWind(phase);
        dash.Recharge();
        ChangeState(PlayerState.Capturing);
        return true;
    }

    public void LeaveWind(bool grounded)
    {
        if (!IsInWind)
            throw new InvalidOperationException("Leaving wind requires a wind state.");

        ChangeState(grounded ? PlayerState.Grounded : PlayerState.Airborne);
    }

    public bool TryReceiveHit(float lockDuration, float invulnerableDuration)
    {
        if (IsInvulnerable())
            return false;
        hit.BeingHit(lockDuration, invulnerableDuration);
        ChangeState(PlayerState.Hit);
        return true;
    }

    public void Launch() => ChangeState(PlayerState.Airborne);

    public void Reset()
    {
        ChangeState(PlayerState.Airborne);
        hit.Reset();
        dash.Recharge();
        wind.LeaveWind();
    }

    public PlayerStepResult Tick(in PlayerFrame frame, in PlayerSettings settings)
    {
        if (frame.DeltaTime < 0f || float.IsNaN(frame.DeltaTime) || float.IsInfinity(frame.DeltaTime))
            throw new ArgumentOutOfRangeException(nameof(frame.DeltaTime));

        var result = new PlayerStepResult { VelocityX = frame.Body.VelocityX, VelocityY = frame.Body.VelocityY };
        hit.Tick(frame.DeltaTime); // 無敵時間はどの動作状態でも進める。
        facing = frame.Input.FacingDirection == 0 ? facing : frame.Input.FacingDirection;
        if (frame.Body.Grounded || IsInWind)
            dash.Recharge();

        if (!IsActionLocked && (frame.Input.JumpQueued && !result.JumpConsumed) && frame.Homing.HasHomingTarget)
            ChangeState(PlayerState.HomingWindup);
        else if ((State == PlayerState.Airborne || IsGliding) && !frame.Body.Grounded &&
                 !frame.Wind.HasWind && (frame.Input.DashQueued && !result.DashConsumed) && dash.TryStart(true, settings.Dash.AirDashDuration))
        {
            float lengthSquared = frame.Input.HorizontalInput * frame.Input.HorizontalInput + frame.Input.VerticalInput * frame.Input.VerticalInput;
            float length = (float)Math.Sqrt(lengthSquared);
            dashX = lengthSquared > 0.0625f ? frame.Input.HorizontalInput / length : facing;
            dashY = lengthSquared > 0.0625f ? frame.Input.VerticalInput / length : 0f;
            ChangeState(PlayerState.AirDashing);
        }

        if (IsGliding && !frame.Body.Grounded && (frame.Input.JumpQueued && !result.JumpConsumed) && !frame.Homing.HasHomingTarget)
        {
            ChangeState(PlayerState.Airborne);
            result.JumpConsumed = true;
        }

        Current.Tick(frame, settings, ref result);
        return result;
    }

    internal void BeginGlide() => glide.Toggle(facing);

    internal void ApplyDash(in PlayerFrame frame, in PlayerSettings settings, ref PlayerStepResult result)
    {
        result.SuppressGravity = true;
        result.VelocityX = dashX * settings.Dash.AirDashSpeed;
        result.VelocityY = dashY * settings.Dash.AirDashSpeed;
        dash.Tick(frame.DeltaTime);
    }

    internal void RechargeDash() => dash.Recharge();
}

public abstract class PlayerStateBase : IPlayerState
{
    protected readonly PlayerStateMachine Machine;
    public abstract PlayerState Id { get; }

    protected PlayerStateBase(PlayerStateMachine machine)
    {
        Machine = machine ?? throw new ArgumentNullException(nameof(machine));
    }

    public virtual void Enter()
    {
    }

    public abstract void Tick(in PlayerFrame frame, in PlayerSettings settings, ref PlayerStepResult result);

    public virtual void Exit()
    {
    }

    protected static void StepHorizontal(in PlayerFrame frame,
        in PlayerMovementSettings settings, ref PlayerStepResult result)
    {
        result.VelocityX = PlayerMovementLogic.StepHorizontal(result.VelocityX,
            frame.Input.HorizontalInput, frame.Body.Grounded, settings.GroundSpeed,
            settings.AirMaximumSpeed, settings.AirAcceleration, settings.AirDamping, frame.DeltaTime);
    }

    protected static void RequireWind(in PlayerFrame frame)
    {
        if (!frame.Wind.HasWind)
            throw new InvalidOperationException("Wind state requires an active wind zone.");
    }
}

public sealed class GroundedState : PlayerStateBase
{
    public override PlayerState Id => PlayerState.Grounded;

    public GroundedState(PlayerStateMachine machine) : base(machine)
    {
    }

    public override void Tick(in PlayerFrame frame, in PlayerSettings settings, ref PlayerStepResult result)
    {
        StepHorizontal(frame, settings.Movement, ref result);
        if (!frame.Body.Grounded)
            Machine.ChangeState(PlayerState.Airborne);
        else if (frame.Input.JumpQueued && !result.JumpConsumed)
        {
            float baseJumpSpeed = settings.Movement.JumpSpeed;

            if (frame.FanWind.HasFanWind)
            {
                float windY = frame.FanWind.WindVectorY;
                float updraftMult = frame.FanWind.UpdraftMultiplier;

                if (windY > 0f)
                {
                    baseJumpSpeed += windY * updraftMult * 0.5f; 
                }
                else if (windY < 0f)
                {
                    float suppressedSpeed = baseJumpSpeed + (windY * updraftMult * 0.5f);
                    baseJumpSpeed = Math.Max(settings.Movement.JumpSpeed * 0.3f, suppressedSpeed); 
                }
            }

            result.VelocityY = baseJumpSpeed;
            Machine.ChangeState(PlayerState.Airborne);
        }
    }
}

public sealed class AirborneState : PlayerStateBase
{
    public override PlayerState Id => PlayerState.Airborne;

    public AirborneState(PlayerStateMachine machine) : base(machine)
    {
    }

    public override void Tick(in PlayerFrame frame, in PlayerSettings settings, ref PlayerStepResult result)
    {
        if (!frame.Body.Grounded && !frame.Wind.HasWind && (frame.Input.JumpQueued && !result.JumpConsumed))
        {
            Machine.ChangeState(PlayerState.Gliding);
            Machine.Current.Tick(frame, settings, ref result);
            return;
        }

        StepHorizontal(frame, settings.Movement, ref result);
        if (!frame.Body.Grounded)
            return;
        Machine.ChangeState(PlayerState.Grounded);
        if ((frame.Input.JumpQueued && !result.JumpConsumed))
        {
            result.VelocityY = settings.Movement.JumpSpeed;
            Machine.ChangeState(PlayerState.Airborne);
        }
    }
}

public sealed class CapturingState : PlayerStateBase
{
    private readonly PlayerUpdraftLogic wind;
    public override PlayerState Id => PlayerState.Capturing;

    public CapturingState(PlayerStateMachine machine, PlayerUpdraftLogic wind) : base(machine)
    {
        this.wind = wind;
    }

    public override void Tick(in PlayerFrame frame, in PlayerSettings settings, ref PlayerStepResult result)
    {
        RequireWind(frame);
        result.SuppressGravity = true;
        bool arrived = PlayerUpdraftLogic.HasCompletedCapture(frame.Wind.CenterX - frame.Body.PositionX,
            frame.Wind.CenterY - frame.Body.PositionY, result.VelocityX, result.VelocityY,
            frame.Wind.CaptureAcceleration * frame.DeltaTime, frame.Wind.CaptureArrivalDistance);
        (result.VelocityX, result.VelocityY) = wind.StepCapture(frame.Body.PositionX, frame.Body.PositionY,
            result.VelocityX, result.VelocityY, frame.Wind.CenterX, frame.Wind.CenterY, frame.DeltaTime,
            frame.Wind.CaptureSpeed, frame.Wind.CaptureAcceleration, frame.Wind.CaptureArrivalDistance);
        if (arrived)
            Machine.ChangeState(PlayerState.Floating);
    }
}

public sealed class FloatingState : PlayerStateBase
{
    private readonly PlayerUpdraftLogic wind;
    public override PlayerState Id => PlayerState.Floating;

    public FloatingState(PlayerStateMachine machine, PlayerUpdraftLogic wind) : base(machine)
    {
        this.wind = wind;
    }

    public override void Enter() => wind.ResumeFloating();

    public override void Tick(in PlayerFrame frame, in PlayerSettings settings, ref PlayerStepResult result)
    {
        RequireWind(frame);
        StepHorizontal(frame, settings.Movement, ref result);
        float targetY = frame.Wind.CenterY + wind.StepWind(frame.DeltaTime, frame.Wind.Period, frame.Wind.Amplitude);
        FollowWind(Machine, wind, frame, settings, ref result, targetY);
    }

    internal static void FollowWind(PlayerStateMachine machine, PlayerUpdraftLogic wind,
        in PlayerFrame frame, in PlayerSettings settings, ref PlayerStepResult result, float targetY)
    {
        result.SuppressGravity = true;
        result.VelocityY = PlayerMovementLogic.StepWindFollow(result.VelocityY, frame.Body.PositionY,
            targetY, settings.Wind.WindMaximumSpeed, settings.Wind.WindAcceleration, frame.DeltaTime);
        if ((frame.Input.JumpQueued && !result.JumpConsumed) && wind.TryWindJump(settings.Movement.JumpSpeed, frame.Wind.BoostMultiplier, out float speed))
        {
            result.SuppressGravity = false;
            result.VelocityY = speed;
            machine.ChangeState(PlayerState.WindJump);
        }
    }
}

public sealed class WindJumpState : PlayerStateBase
{
    private readonly PlayerUpdraftLogic wind;
    public override PlayerState Id => PlayerState.WindJump;

    public WindJumpState(PlayerStateMachine machine, PlayerUpdraftLogic wind) : base(machine)
    {
        this.wind = wind;
    }

    public override void Tick(in PlayerFrame frame, in PlayerSettings settings, ref PlayerStepResult result)
    {
        RequireWind(frame);
        StepHorizontal(frame, settings.Movement, ref result);
        float targetY = frame.Wind.CenterY + wind.StepWind(frame.DeltaTime, frame.Wind.Period, frame.Wind.Amplitude);
        if (result.VelocityY <= 0f && Math.Abs(frame.Body.PositionY - targetY) <= settings.Wind.RecaptureHeight)
        {
            Machine.ChangeState(PlayerState.Floating);
            FloatingState.FollowWind(Machine, wind, frame, settings, ref result, targetY);
        }
    }
}

public sealed class GlidingState : PlayerStateBase
{
    private readonly PlayerGlideFlightLogic glide;
    public override PlayerState Id => PlayerState.Gliding;

    public GlidingState(PlayerStateMachine machine, PlayerGlideFlightLogic glide) : base(machine)
    {
        this.glide = glide;
    }

    public override void Enter() => Machine.BeginGlide();
    public override void Exit() => glide.Stop();

    public override void Tick(in PlayerFrame frame, in PlayerSettings settings, ref PlayerStepResult result)
    {
        // 入場フレームのJumpQueuedは開始要求なので、解除判定は次Tickから行う。
        if (frame.Body.Grounded)
        {
            Machine.ChangeState(PlayerState.Grounded);
            Machine.Current.Tick(frame, settings, ref result);
            return;
        }

        PlayerGlideVelocity currentVel = new PlayerGlideVelocity(result.VelocityX, result.VelocityY);
        PlayerGlideVelocity nextVel;

        if (frame.FanWind.HasFanWind)
        {
            // 風エリア内：風補正付きの滑空ロジックを実行
            nextVel = glide.StepWithFanWind(
                currentVel,
                frame.Input.HorizontalInput,
                frame.Body.Gravity,
                frame.DeltaTime,
                settings.Glide,
                settings.FanWind,
                frame.FanWind);
        }
        else
        {
            nextVel = glide.Step(new PlayerGlideVelocity(result.VelocityX, result.VelocityY), frame.Input.HorizontalInput,
                frame.Body.Gravity, frame.DeltaTime, settings.Glide.GlideMaximumSpeed, settings.Glide.GlideLiftCoefficient,
                settings.Glide.GlideDragCoefficient, settings.Glide.GlideMinimumLiftSpeed, settings.Glide.GlidePullupStartSpeed,
                settings.Glide.GlidePullupStrength, settings.Glide.GlideSteerAcceleration, settings.Glide.GlideTurnLoss);
        }
        result.SuppressGravity = true;
        result.VelocityX = nextVel.X;
        result.VelocityY = nextVel.Y;
    }
}

public sealed class AirDashingState : PlayerStateBase
{
    private readonly PlayerAirDashLogic dash;
    public override PlayerState Id => PlayerState.AirDashing;

    public AirDashingState(PlayerStateMachine machine, PlayerAirDashLogic dash) : base(machine)
    {
        this.dash = dash;
    }

    public override void Exit() => dash.Cancel();

    public override void Tick(in PlayerFrame frame, in PlayerSettings settings, ref PlayerStepResult result)
    {
        if (!dash.IsDashing)
        {
            Machine.ChangeState(frame.Body.Grounded ? PlayerState.Grounded : PlayerState.Airborne);
            result.JumpConsumed = true;
            Machine.Current.Tick(frame, settings, ref result);
            return;
        }

        Machine.ApplyDash(frame, settings, ref result);
    }
}

public sealed class HomingWindupState : PlayerStateBase
{
    public override PlayerState Id => PlayerState.HomingWindup;
    public HomingWindupState(PlayerStateMachine machine) : base(machine) { }

    public override void Tick(in PlayerFrame frame, in PlayerSettings settings, ref PlayerStepResult result)
    {
        result.JumpConsumed = true;
        result.DashConsumed = true;
        result.VelocityX = 0f;
        result.VelocityY = 0f;
        result.SuppressGravity = true;
        if (!frame.Homing.HasHomingTarget)
        {
            result.SuppressGravity = false;
            Machine.ChangeState(PlayerState.Airborne);
        }
        else if (frame.Homing.WindupComplete)
        {
            Machine.ChangeState(PlayerState.Homing);
            Machine.Current.Tick(frame, settings, ref result);
        }
    }
}
public sealed class HomingState : PlayerStateBase
{
    public override PlayerState Id => PlayerState.Homing;
    private float _elapsedTime;

    public HomingState(PlayerStateMachine machine) : base(machine)
    {
    }

    public override void Enter() => _elapsedTime = 0f;

    public override void Tick(in PlayerFrame frame, in PlayerSettings settings, ref PlayerStepResult result)
    {
        _elapsedTime += frame.DeltaTime;
        if (!frame.Homing.HasHomingTarget || frame.Homing.TouchingHomingTarget ||
            frame.Homing.BlockedByObstacle || _elapsedTime >= settings.Homing.MaximumDuration)
        {
            result.ApplyHomingAttack = frame.Homing.HasHomingTarget && frame.Homing.TouchingHomingTarget;
            result.VelocityX = 0f;
            result.VelocityY = result.ApplyHomingAttack ? settings.Homing.SuccessUpwardSpeed : 0f;
            result.SuppressGravity = false;
            if (result.ApplyHomingAttack)
                Machine.RechargeDash();
            Machine.ChangeState(PlayerState.Airborne);
            return;
        }

        result.SuppressGravity = true;
        result.VelocityX = frame.Homing.HomingDirectionX * settings.Homing.HomingSpeed;
        result.VelocityY = frame.Homing.HomingDirectionY * settings.Homing.HomingSpeed;
    }
}

public sealed class HitState : PlayerStateBase
{
    private readonly PlayerHitLogic hit;
    public override PlayerState Id => PlayerState.Hit;

    public HitState(PlayerStateMachine machine, PlayerHitLogic hit) : base(machine)
    {
        this.hit = hit;
    }

    public override void Tick(in PlayerFrame frame, in PlayerSettings settings, ref PlayerStepResult result)
    {
        if (hit.IsLocked)
            return;
        Machine.ChangeState(frame.Body.Grounded ? PlayerState.Grounded : PlayerState.Airborne);
        result.JumpConsumed = true;
        result.DashConsumed = true;
        Machine.Current.Tick(frame, settings, ref result);
    }
}
public sealed class AirCurrentState : PlayerStateBase
{
    public override PlayerState Id => PlayerState.AirCurrent;

    public AirCurrentState(PlayerStateMachine machine) : base(machine) { }

    public override void Tick(in PlayerFrame frame, in PlayerSettings settings, ref PlayerStepResult result)
    {
        if (!frame.AirCurrent.HasCurrent)
            throw new InvalidOperationException("Air current state requires an active zone.");
        result.SuppressGravity = true;
        result.JumpConsumed = true;
        result.DashConsumed = true;
        PlayerAirCurrentLogic.Step(frame.AirCurrent, frame.Body.PositionX, frame.Body.PositionY,
            frame.DeltaTime, ref result.VelocityX, ref result.VelocityY);
    }
}

