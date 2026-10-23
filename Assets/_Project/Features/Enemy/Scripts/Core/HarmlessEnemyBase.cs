using UnityEngine;

public abstract class HarmlessEnemyBase : MonoBehaviour, IEnemy
{
    protected readonly EnemyDefeatLogic defeatLogic = new EnemyDefeatLogic();
    protected Collider2D hitCollider;
    public bool IsDefeated => defeatLogic.IsDefeated;

    protected virtual void Awake()
    {
        hitCollider = GetComponent<Collider2D>();
    }

    protected virtual void Update()
    {
        if (IsDefeated)
            return;

        UpdateBehavior();
    }

    protected abstract void UpdateBehavior();
    public abstract void ReceiveAttack();
}
