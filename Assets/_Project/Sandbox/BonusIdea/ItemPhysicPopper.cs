using UnityEngine;
using System;

public class ItemPhysicPopper : MonoBehaviour
{
    private Rigidbody2D body;
    private ICollectibleItem collectibleItem;

    private void Awake()
    {
        body = GetComponent<Rigidbody2D>();
        collectibleItem = GetComponent<ICollectibleItem>();

        if (collectibleItem == null)
            throw new InvalidOperationException($"{gameObject.name}: ICollectibleItem を実装したコンポーネントが必要です。");
    }

    public void Pop(Vector2 force)
    {
        if (body != null)
        {
            body.AddForce(force, ForceMode2D.Impulse);
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        CheckAndCollect(collision.gameObject);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        CheckAndCollect(other.gameObject);
    }

    private void CheckAndCollect(GameObject target)
    {
        if (target.TryGetComponent<PlayerItemCollector>(out var collector))
        {
            collectibleItem.TryCollect(collector);
        }
    }
}
