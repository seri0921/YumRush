using System;
using UnityEngine;

public sealed class ChunkRespawnController : MonoBehaviour
{
    [SerializeField] private RespawnManager manager;
    [SerializeField] private ChunkRespawnConfig config;
    private SpawnPoint[] points;
    private ChunkRespawnLogic logic;

    public void Configure(RespawnManager respawnManager, ChunkRespawnConfig chunkConfig)
    {
        if (Application.isPlaying && (isActiveAndEnabled || logic != null))
            throw new InvalidOperationException("Configure Chunks before activation and initialization.");
        manager = respawnManager != null ? respawnManager : throw new ArgumentNullException(nameof(respawnManager));
        config = chunkConfig != null ? chunkConfig : throw new ArgumentNullException(nameof(chunkConfig));
    }

    public void Configure(RespawnManager respawnManager) => Configure(respawnManager, config);

    private void OnEnable()
    {
        if (manager == null || config == null)
            throw new InvalidOperationException($"{name}: RespawnManager and ChunkRespawnConfig are required.");
        logic = new ChunkRespawnLogic(config.AutomaticRespawn, config.IntervalSeconds);
        points = GetComponentsInChildren<SpawnPoint>(true);
        foreach (SpawnPoint point in points)
        {
            if (point.GetComponentInParent<ChunkRespawnController>(true) != this)
                throw new InvalidOperationException("Nested respawn Chunks are not supported.");
            point.Initialize();
        }
        manager.Register(this);
    }

    private void OnDisable()
    {
        // The manager may already have been destroyed during scene unload.
        if (manager != null)
            manager.Unregister(this);
    }

    public void Tick(float deltaTime)
    {
        if (logic.Tick(deltaTime))
        {
            RespawnAll();
            return;
        }
        foreach (SpawnPoint point in points)
            if (point.isActiveAndEnabled)
                point.Tick(deltaTime);
    }

    /// <summary>Event entry point: refills missing slots and restarts the Chunk clock.</summary>
    public void RespawnAll()
    {
        if (!isActiveAndEnabled || points == null)
            throw new InvalidOperationException("Only an initialized, active Chunk can respawn.");
        foreach (SpawnPoint point in points)
            if (point.isActiveAndEnabled)
                point.RespawnMissing();
        logic.Reset();
    }
}
