using UnityEngine;

public class ScoreItemMagnet : MonoBehaviour
{
    private PlayerPresenter target;

    [Header("マグネット処理")]
    [SerializeField] private float initialSpeed = 3f;
    [SerializeField] private float acceleration = 20f;
    [SerializeField] private float maxSpeed = 30f;

    private float currentSpeed;
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
        currentSpeed = Mathf.Min(currentSpeed + acceleration * Time.deltaTime, maxSpeed);
        Vector2 currentPos = transform.position;
        Vector2 targetPos = target.transform.position;
        Vector2 direction = (targetPos - currentPos).normalized;

        transform.position += (Vector3)(direction * currentSpeed * Time.deltaTime);
    }
}
