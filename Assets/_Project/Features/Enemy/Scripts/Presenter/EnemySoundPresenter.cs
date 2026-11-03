using UnityEngine;
using UnityEngine.Assertions;

[DisallowMultipleComponent]
public sealed class EnemySoundPresenter : MonoBehaviour
{
    [SerializeField] private EnemyBase _enemy;
    [SerializeField] private EnemySoundView _view;

    private void Awake()
    {
        Assert.IsNotNull(_enemy);
        Assert.IsNotNull(_view);
    }

    private void OnEnable()
    {
        _enemy.Defeated += HandleDefeated;
        _enemy.AttackFired += HandleAttackFired;
        if (_enemy is Boss_Axolotl boss)
            boss.BarrierBroken += HandleBarrierBroken;
        if (_enemy is Boss_Bat bat)
            bat.WingsOpened += HandleWingsOpened;
    }

    private void OnDisable()
    {
        if (_enemy != null)
        {
            _enemy.Defeated -= HandleDefeated;
            _enemy.AttackFired -= HandleAttackFired;
            if (_enemy is Boss_Axolotl boss)
                boss.BarrierBroken -= HandleBarrierBroken;
            if (_enemy is Boss_Bat bat)
                bat.WingsOpened -= HandleWingsOpened;
        }
    }

    private void HandleAttackFired() => _view.PlayAttack();

    private void HandleBarrierBroken() => _view.PlayBarrierBreak();

    private void HandleWingsOpened() => _view.PlayWingsOpen();

    private void HandleDefeated() => _view.PlayDeath();
}
