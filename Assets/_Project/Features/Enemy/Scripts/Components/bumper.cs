using UnityEngine;

public sealed class bumper : HarmlessEnemyBase
{
    [Header("クールタイム")]
    [SerializeField] private float cool = 1.0f;
    private readonly EnemyFireLogic fireLogic = new EnemyFireLogic();
    private SpriteRenderer spriteRenderer;
    private bool isCool;

    protected override void Awake()
    {
        base.Awake();
         spriteRenderer = GetComponent<SpriteRenderer>();
    }

    protected override void UpdateBehavior()
    {
        if (!isCool) return;
        if (fireLogic.Tick(Time.deltaTime, 0f, 1f, cool)) 
        {
            SetCoolingDown(false);
        }
    }
    public override void ReceiveAttack() 
    {
        if (isCool) return;
        SetCoolingDown(true);
    }

    private void SetCoolingDown(bool setcool)
    {
        isCool = setcool;
        hitCollider.enabled = !setcool;
        Color color = spriteRenderer.color;
        color.a = setcool ? 0.5f : 1.0f;
        spriteRenderer.color = color;
    }


}
