using System;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public sealed class PlayerItemCollector : MonoBehaviour
{
    [SerializeField] private ScorePresenter scorePresenter;

    private void Awake()
    {
        if (scorePresenter == null)
            throw new InvalidOperationException("PlayerItemCollector requires a ScorePresenter.");
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Wind zones and other non-item triggers are valid contacts.
        ICollectibleItem item = other.GetComponentInParent<ICollectibleItem>();
        if (item != null)
            item.TryCollect(this);
    }

    public void AddScore(int points) => scorePresenter.Add(points);
}
