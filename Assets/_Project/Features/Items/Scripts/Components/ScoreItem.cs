using System;
using UnityEngine;

public sealed class ScoreItem : MonoBehaviour, ICollectibleItem
{
    [SerializeField] private ScoreItemConfig config;
    private bool collected;

    private void Awake()
    {
        if (config == null)
            throw new InvalidOperationException("ScoreItem requires a ScoreItemConfig.");
    }

    public bool TryCollect(PlayerItemCollector collector)
    {
        if (collector == null)
            throw new ArgumentNullException(nameof(collector));
        if (collected || !isActiveAndEnabled)
            return false;

        collector.AddScore(config.Points, config.GainKind);
        collected = true;
        if (TryGetComponent<ItemCollectEffectView>(out var effectView))
            effectView.Play(transform.position);
        if (TryGetComponent<RespawnTarget>(out var respawnTarget))
            respawnTarget.NotifyConsumed();
        gameObject.SetActive(false);
        Destroy(gameObject);
        return true;
    }
}
