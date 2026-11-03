using UnityEngine;

public class TreasureKey : HarmlessEnemyBase
{
    [Header("光の線コンポーネント")]
    [SerializeField] private TreasureKeyLink _energyLink;

    private TreasureItem _targetBarrier;

    protected override void Awake()
    {
        base.Awake();
        if (_energyLink == null)
            TryGetComponent<TreasureKeyLink>(out _energyLink);
    }
    protected override void UpdateBehavior(){}


    public void Setup(TreasureItem barrier)
    {
        _targetBarrier = barrier;
        _targetBarrier.RegisterKey(gameObject);

        if (_energyLink != null && _targetBarrier != null)
        {
            _energyLink.SetTargets(transform, _targetBarrier.transform);
        }
    }

    public override void ReceiveAttack()
    {
        if (IsDefeated) return;
        defeatLogic.TryDefeat();


        if (_targetBarrier != null)
        {
            _targetBarrier.OnKeyDestroyed(gameObject);
        }
        Destroy(gameObject);
    }
}
