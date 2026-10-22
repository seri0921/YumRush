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
    
    private readonly PlayerStateMachine _machine = new PlayerStateMachine();
    private PlayerPhysicsMotor _motor;
    private PlayerInput _input;
    private PlayerFrameSource _frames;
    private PlayerWindContact _windContact;
    private PlayerDamageReceiver _damageReceiver;
    private PlayerHomingAttack _homing;
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
        config.Validate();
        
        _motor = new PlayerPhysicsMotor(GetComponent<Rigidbody2D>(), playerCollider,
            config.GroundLayers, config.GroundProbeDistance);
        
        _input = new PlayerInput();
        TryGetComponent(out _homing);
        _frames = new PlayerFrameSource(config, _motor, _input);
        _windContact = new PlayerWindContact(_machine, _motor, config);
        _damageReceiver = new PlayerDamageReceiver(_machine, _motor, config, _input, _homing);
    }

        private void OnEnable() => _input.Enable();
        
        private void OnDisable()
        {
            scriptedMovement?.Dispose();
            _input.Disable();
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
            _windContact.Sample(ref frame);
            _homing.Sample(ref frame, _machine.IsHoming);
            PlayerSettings settings = _frames.SampleSettings();
            PlayerStepResult result = _machine.Tick(frame, settings);
            if (result.SuppressGravity)
                _motor.SetGravityScale(0f);
            else
                _motor.RestoreGravity();
            _motor.ApplyVelocity(new Vector2(result.VelocityX, result.VelocityY));
            _homing.Resolve(result, _machine.IsHoming);
            _input.ClearRequests();
        }
        
        public IDisposable LockOperation() => _input.LockOperation();

        public IDisposable BeginScriptedMovement()
        {
            if (!isActiveAndEnabled)
                throw new InvalidOperationException("Scripted movement requires an active player.");
            if (scriptedMovement != null)
                throw new InvalidOperationException("Scripted movement is already active.");

            IDisposable inputLock = _input.LockOperation();
            try
            {
                _machine.ChangeState(PlayerState.Airborne);
                _homing?.ClearTarget();
                _motor.RestoreGravity();
                _motor.BeginScriptedMovement();
                scriptedMovement = new ScriptedMovement(this, inputLock);
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
        private void OnTriggerEnter2D(Collider2D other) => _windContact.Enter(other);
        private void OnTriggerExit2D(Collider2D other) => _windContact.Exit(other);

        private sealed class ScriptedMovement : IDisposable
        {
            // null means this handle has already been released.
            private PlayerPresenter owner;
            private readonly IDisposable inputLock;

            public ScriptedMovement(PlayerPresenter owner, IDisposable inputLock)
            {
                this.owner = owner;
                this.inputLock = inputLock;
            }

            public void Dispose()
            {
                if (ReferenceEquals(owner, null))
                    return;

                try
                {
                    owner._motor.EndScriptedMovement();
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
