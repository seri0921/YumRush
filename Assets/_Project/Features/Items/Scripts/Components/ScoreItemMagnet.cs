using UnityEngine;

public class ScoreItemMagnet : MonoBehaviour
{
    private PlayerPresenter target;

    [Header("検知設定")]
    [SerializeField] private float detectRange = 4f;

    [Header("マグネット処理")]
    [SerializeField] private float initialSpeed = 3f;
    [SerializeField] private float acceleration = 20f;
    [SerializeField] private float maxSpeed = 30f;

    private float currentSpeed;
    private bool isAttracted;
    private bool HasTarget => target != null;

    private void Awake()
    {
        if (target == null)
            target = FindFirstObjectByType<PlayerPresenter>();

        currentSpeed = initialSpeed;
    }

    private void Update()
    {
        if (!HasTarget) return;

        Vector2 currentPos = transform.position;
        Vector2 targetPos = target.transform.position;

        if (!isAttracted)
        {
            float sqrDistance = (targetPos - currentPos).sqrMagnitude;
            float sqrRange = detectRange * detectRange;

            if (sqrDistance <= sqrRange)
            {
                isAttracted = true;
            }
            else
            {
                return;
            }
        }

        currentSpeed = Mathf.Min(currentSpeed + acceleration * Time.deltaTime, maxSpeed);
        Vector2 direction = (targetPos - currentPos).normalized;

        transform.position += (Vector3)(direction * currentSpeed * Time.deltaTime);

    }

    private void OnDrawGizmosSelected()
    {

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectRange);
    }

}
