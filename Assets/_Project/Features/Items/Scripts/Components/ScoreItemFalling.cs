using UnityEngine;

public class ScoreItemFalling : MonoBehaviour
{
    [SerializeField] private float fallSpeed = 5f;
    [SerializeField] private float maxFallDistance = 60f;

    private float startY;

    private void Start()
    {
        startY = transform.position.y;
    }

    private void Update()
    {
        transform.position += Vector3.down * (fallSpeed * Time.deltaTime);

        if (startY - transform.position.y >= maxFallDistance)
        {
            if (TryGetComponent<RespawnTarget>(out var respawnTarget))
            {
                respawnTarget.NotifyConsumed();
            }

            Destroy(gameObject);
        }
    }
}
