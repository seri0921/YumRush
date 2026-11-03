using UnityEngine;
using UnityEngine.Assertions;

[DisallowMultipleComponent]
public sealed class PlayerSoundPresenter : MonoBehaviour
{
    [SerializeField] private PlayerPresenter _player;
    [SerializeField] private PlayerSoundView _view;
    [SerializeField, Min(0.01f)] private float _walkInterval = 0.35f;
    [SerializeField, Min(0f)] private float _walkSpeedThreshold = 0.1f;

    [SerializeField] private PlayerHomingAttackView _attackView;

    [SerializeField] private HomingReticleView _reticleView;

    private float _walkTimer;

    private void Awake()
    {
        Assert.IsNotNull(_player);
        Assert.IsNotNull(_view);
        Assert.IsNotNull(_attackView);
        Assert.IsNotNull(_reticleView);
    }

    private void OnEnable()
    {
        _walkTimer = 0f;
        _player.GroundJumpStarted += HandleGroundJumpStarted;
        _player.StateChanged += HandleStateChanged;
        _player.AttackEffectPlayed += HandleAttackEffectPlayed;
        _attackView.AnimationStarted += HandleAttackAnimationStarted;
        _player.DamageReceived += HandleDamageReceived;
        _reticleView.Acquired += HandleReticleAcquired;
    }

    private void OnDisable()
    {
        if (_player != null)
        {
            _player.GroundJumpStarted -= HandleGroundJumpStarted;
            _player.StateChanged -= HandleStateChanged;
            _player.AttackEffectPlayed -= HandleAttackEffectPlayed;
            _player.DamageReceived -= HandleDamageReceived;
        }
        if (_attackView != null)
            _attackView.AnimationStarted -= HandleAttackAnimationStarted;
        if (_reticleView != null)
            _reticleView.Acquired -= HandleReticleAcquired;
        if (_view != null)
            _view.StopAll();
    }

    private void LateUpdate()
    {
        if (!_player.isActiveAndEnabled || _player.IsScriptedMovement)
        {
            _walkTimer = 0f;
            _view.StopAll();
            return;
        }

        if (HitStopPresenter.IsStopping || Time.timeScale == 0f)
        {
            _walkTimer = 0f;
            _view.StopWalking();
            _view.SetGliding(false);
            return;
        }

        _view.SetGliding(_player.IsGliding);
        bool grounded = _player.State == PlayerState.Grounded || _player.State == PlayerState.IceGrounded;
        bool walking = grounded && Mathf.Abs(_player.MoveInput.x) > 0f &&
                       Mathf.Abs(_player.FlightVelocity.x) > _walkSpeedThreshold;
        if (!walking)
        {
            _walkTimer = 0f;
            _view.StopWalking();
            return;
        }

        _walkTimer -= Time.deltaTime;
        if (_walkTimer <= 0f)
        {
            _view.PlayWalk();
            _walkTimer = _walkInterval;
        }
    }

    private void HandleDamageReceived() => _view.PlayDamage();
    private void HandleReticleAcquired() => _view.PlayReticle();

    private void HandleAttackAnimationStarted() => _view.PlayAttackReach();
    private void HandleAttackEffectPlayed() => _view.PlayAttackEat();

    private void HandleGroundJumpStarted()
    {
        if (_player.isActiveAndEnabled && !_player.IsScriptedMovement)
            _view.PlayJump();
    }

    private void HandleStateChanged(PlayerState previousState, PlayerState newState)
    {
        if (newState != PlayerState.Gliding)
            _view.SetGliding(false);
        if (newState != PlayerState.Grounded && newState != PlayerState.IceGrounded)
            _view.StopWalking();
        if (_player.isActiveAndEnabled && !_player.IsScriptedMovement && newState == PlayerState.AirDashing)
            _view.PlayAirDash();
    }
}
