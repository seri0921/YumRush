using System;
using UnityEngine;

public sealed class SpawnPoint : MonoBehaviour
{
    [SerializeField] private GameObject prefab;
    [SerializeField] private RespawnConfig config;
    private RespawnLogic logic;
    private RespawnTarget current;

    public void Configure(GameObject spawnPrefab, RespawnConfig respawnConfig)
    {
        if (Application.isPlaying)
            throw new InvalidOperationException("Configure SpawnPoints before Play Mode.");
        prefab = spawnPrefab != null ? spawnPrefab : throw new ArgumentNullException(nameof(spawnPrefab));
        config = respawnConfig != null ? respawnConfig : throw new ArgumentNullException(nameof(respawnConfig));
    }

    public void Initialize()
    {
        if (logic != null)
            return; // Re-enabling a Chunk preserves existing slots.
        if (prefab == null || config == null)
            throw new InvalidOperationException($"{name}: Prefab and RespawnConfig are required.");
        logic = new RespawnLogic(config.AutomaticRespawn, config.DelaySeconds);
        CreateInstance();
    }

    public void NotifyConsumed(RespawnTarget target)
    {
        if (target != current)
            throw new InvalidOperationException("Consumption came from an unowned instance.");
        logic.MarkConsumed();
        current = null;
    }

    public void Tick(float deltaTime)
    {
        if (logic.Tick(deltaTime))
            RespawnMissing();
    }

    public void RespawnMissing()
    {
        if (logic.IsAlive)
            return; // Refill deliberately preserves living instances.
        CreateInstance();
        logic.MarkSpawned();
    }

    private void CreateInstance()
    {
        GameObject instance = Instantiate(prefab, transform.position, transform.rotation, transform);
        // The slot owns the full placement transform, including scale.
        instance.transform.localScale = Vector3.one;
        if (!instance.TryGetComponent(out current))
            current = instance.AddComponent<RespawnTarget>();
        current.Initialize(this);
    }
}
