using UnityEngine;
using UnityEngine.Assertions;

// Maps confirmed player events to independently tunable shake settings.
[DisallowMultipleComponent]
[RequireComponent(typeof(CameraShakeView))]
public sealed class CameraShakePresenter : MonoBehaviour
{
    [SerializeField] private PlayerPresenter _player;
    [Header("Attack")]
    [SerializeField, Min(0f)] private float _attackStrength = 0.18f;
    [SerializeField, Min(0f)] private float _attackDuration = 0.12f;
    [Header("Damage")]
    [SerializeField, Min(0f)] private float _damageStrength = 0.32f;
    [SerializeField, Min(0f)] private float _damageDuration = 0.22f;

    private CameraShakeView _view;

    private void Awake()
    {
        Assert.IsNotNull(_player);
        _view = GetComponent<CameraShakeView>();
    }

    private void OnEnable()
    {
        _player.AttackEffectPlayed += HandleAttackEffectPlayed;
        _player.DamageReceived += HandleDamageReceived;
    }

    private void OnDisable()
    {
        // Scene teardown may destroy the player before the camera.
        if (_player == null)
            return;
        _player.AttackEffectPlayed -= HandleAttackEffectPlayed;
        _player.DamageReceived -= HandleDamageReceived;
    }

    private void HandleAttackEffectPlayed() => _view.Play(_attackStrength, _attackDuration);
    private void HandleDamageReceived() => _view.Play(_damageStrength, _damageDuration);
}
