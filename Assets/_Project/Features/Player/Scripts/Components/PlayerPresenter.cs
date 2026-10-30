using System;
using UnityEngine;

// プレイヤーのの進行を担当。入力を集め、状態判断を呼び出し、結果を物理へ反映する
public class PlayerPresenter : MonoBehaviour
{
    // フィールド
    [Header("プレイヤーの設定")] [SerializeField] 
    private PlayerMovementConfig config;

    [Header("プレイヤー本体のコライダー")] [SerializeField] 
    private Collider2D playerCollider;

    [SerializeField] private PlayerAnimationView animationView;
    [SerializeField] private PlayerWindEffectView windEffectView;
    
    [SerializeField] private PlayerHomingAttackView homingAttackView;
    [SerializeField] private HomingReticleView homingReticleView;
    [SerializeField] private PlayerAttackEffectView attackEffectView;

    private readonly PlayerStateMachine _machine = new PlayerStateMachine();
    private PlayerPhysicsMotor _motor;
    private PlayerInput _input;
    private PlayerFrameSource _frames;
    private PlayerWindContact _windContact;
    private PlayerAirCurrentContact _airCurrentContact;
    private PlayerDamageReceiver _damageReceiver;
    private PlayerHomingAttack _homing;
    private PlayerFanWindContact _fanWindContact;
    // nullはレティクルの対象がない状態。
    private IEnemy reticleTarget;
    // null while no external movement owns the player.
    private ScriptedMovement scriptedMovement;

    private float horizontalInput;
    
    // パブリックプロパティ
    public PlayerState State => _machine.State;
    public float CurrentLiftVelocity => _machine.LiftVelocity;
    public float MaximumLiftVelocity => _windContact.MaximumLiftVelocity;
    public bool CanWindJump => _machine.CanWindJump;
    public int FacingDirection { get; private set; } = 1;
    public Vector2 MoveInput => _machine.IsActionLocked ? Vector2.zero : new Vector2(_input.Direction.x, 0f);
    public bool IsActionLocked => _machine.IsActionLocked;
    public bool IsGliding => _machine.IsGliding;
    public bool IsHoming => _machine.IsHoming;
    public float FlightSpeed => _motor.Velocity.magnitude;
    public Vector2 FlightVelocity => _motor.Velocity;
    public float Altitude => _motor.Position.y;
    public float GlideMaximumSpeed => config.GlideMaximumSpeed;
    
    
    private void Awake()
    {
        UnityEngine.Assertions.Assert.IsNotNull(animationView);
        UnityEngine.Assertions.Assert.IsNotNull(windEffectView);
        UnityEngine.Assertions.Assert.IsNotNull(homingAttackView);
        UnityEngine.Assertions.Assert.IsNotNull(homingReticleView);
        UnityEngine.Assertions.Assert.IsNotNull(attackEffectView);
        config.Validate();
        
        _motor = new PlayerPhysicsMotor(GetComponent<Rigidbody2D>(), playerCollider,
            config.GroundLayers, config.GroundProbeDistance);
        
        _input = new PlayerInput();
        TryGetComponent(out _homing);
        _frames = new PlayerFrameSource(config, _motor, _input);
        _windContact = new PlayerWindContact(_machine, _motor, config);
        _airCurrentContact = new PlayerAirCurrentContact(_machine, _motor);
        _fanWindContact = new PlayerFanWindContact(_machine);
        _damageReceiver = new PlayerDamageReceiver(_machine, _motor, config, _input, _homing);
    }

        private void OnEnable() => _input.Enable();
        
        private void OnDisable()
        {
            scriptedMovement?.Dispose();
            _input.Disable();
            windEffectView.Hide();
            homingAttackView.Hide();
            homingReticleView.Hide();
            reticleTarget = null;
            _airCurrentContact.Clear();
            _machine.Reset();
            _homing?.ClearTarget();
            _motor.RestoreGravity();
        }
        
        private void OnDestroy()
        {
            _windContact.Dispose();
            _input.Dispose();
        }
        
        private void Update()
        {
            _input.Sample(_machine.IsActionLocked);
            if (_input.Direction.x != 0f)
                FacingDirection = _input.Direction.x > 0f ? 1 : -1;
        }

        private void FixedUpdate()
        {
            if (_motor.IsScriptedMovement)
                return;

            PlayerFrame frame = _frames.Sample(Time.fixedDeltaTime, FacingDirection);
            _airCurrentContact.Sample(ref frame);
            _windContact.Sample(ref frame);
       　 _fanWindContact.Sample(ref frame);
       　 _homing.Sample(ref frame, _machine.IsHoming);
            frame.Homing.WindupComplete = homingAttackView.IsComplete;
            PlayerSettings settings = _frames.SampleSettings();
            PlayerStepResult result = _machine.Tick(frame, settings);
            // 攻撃適用で対象が破棄される前に、その位置で準備完了の演出を再生する。
            if (result.HomingWindupCompleted && _homing.TryGetTargetPosition(out Vector2 effectPosition))
                attackEffectView.PlayWindupComplete(effectPosition);
            if (_machine.IsHoming && frame.Homing.HasHomingTarget &&
                Mathf.Abs(frame.Homing.HomingDirectionX) > 0.001f)
                FacingDirection = frame.Homing.HomingDirectionX > 0f ? 1 : -1;
            if (result.SuppressGravity)
                _motor.SetGravityScale(0f);
            else
                _motor.RestoreGravity();
            _motor.ApplyVelocity(new Vector2(result.VelocityX, result.VelocityY));
            _homing.Resolve(result, _machine.IsHoming);
            bool preparing = State == PlayerState.HomingWindup;
            Vector2 targetPosition = Vector2.zero;
            preparing = preparing && _homing.TryGetTargetPosition(out targetPosition);
            homingAttackView.Render(preparing, targetPosition);
            _input.ClearRequests();
        }

        private void LateUpdate()
        {
            bool preparing = State == PlayerState.HomingWindup;
            Vector2 targetPosition = Vector2.zero;
            preparing = preparing && _homing.TryGetTargetPosition(out targetPosition);
            homingAttackView.Render(preparing, targetPosition);
            animationView.Render(State, _motor.Velocity.x, FacingDirection);
            windEffectView.Render(State, FacingDirection);
            IEnemy currentTarget = null;
            bool showReticle = !_motor.IsScriptedMovement && State != PlayerState.Hit && State != PlayerState.AirCurrent &&
                               _homing.TryGetTarget(out currentTarget, out targetPosition);
            bool targetChanged = !ReferenceEquals(reticleTarget, currentTarget);
            homingReticleView.Render(showReticle, targetPosition, targetChanged, Time.deltaTime);
            reticleTarget = currentTarget;
        }
        
        public IDisposable LockOperation() => _input.LockOperation();

        public ScriptedMovement BeginScriptedMovement()
        {
            if (!isActiveAndEnabled)
                throw new InvalidOperationException("Scripted movement requires an active player.");
            if (scriptedMovement != null)
                throw new InvalidOperationException("Scripted movement is already active.");

            IDisposable inputLock = _input.LockOperation();
            try
            {
                homingAttackView.Hide();
                homingReticleView.Hide();
                // Keep location-independent actions alive without calling their Exit/Enter hooks.
                bool preserveMotion = State == PlayerState.Airborne || State == PlayerState.Gliding ||
                                      State == PlayerState.AirDashing;
                if (!preserveMotion)
                {
                    _machine.ChangeState(PlayerState.Airborne);
                    _motor.RestoreGravity();
                }
                _homing?.ClearTarget();
                _motor.BeginScriptedMovement();
                scriptedMovement = new ScriptedMovement(this, inputLock, preserveMotion);
                return scriptedMovement;
            }
            catch
            {
                inputLock.Dispose();
                throw;
            }
        }

        public void SetScriptedPosition(Vector2 position) => _motor.SetScriptedPosition(position);

        public bool ReceiveEnemyProjectile() => !_motor.IsScriptedMovement && _damageReceiver.Receive();
        private void OnTriggerEnter2D(Collider2D other)
        {
            _airCurrentContact.Enter(other);
     　   _fanWindContact.Enter(other);
　        if (!_motor.IsScriptedMovement)
                _windContact.Enter(other);
        }
        private void OnTriggerExit2D(Collider2D other)
        {
            _airCurrentContact.Exit(other);
  　     _fanWindContact.Exit(other);
     　   _windContact.Exit(other);
        }

        public sealed class ScriptedMovement : IDisposable
        {
            // null means this handle has already been released.
            private PlayerPresenter owner;
            private readonly IDisposable inputLock;
            private readonly bool preserveMotion;

            internal ScriptedMovement(PlayerPresenter owner, IDisposable inputLock, bool preserveMotion)
            {
                this.owner = owner;
                this.inputLock = inputLock;
                this.preserveMotion = preserveMotion;
            }

            public void Complete() => Release(true);

            public void Dispose() => Release(false);

            private void Release(bool completed)
            {
                if (ReferenceEquals(owner, null))
                    return;

                try
                {
                    if (!completed)
                        owner._machine.ChangeState(PlayerState.Airborne);
                    owner._motor.EndScriptedMovement(completed && preserveMotion);
                }
                finally
                {
                    inputLock.Dispose();
                    owner.scriptedMovement = null;
                    owner = null;
                }
            }
        }
}
