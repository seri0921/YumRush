using System;
using UnityEngine;

/// <summary>Connects a biome's contents to the scene dependencies before activation.</summary>
[DisallowMultipleComponent]
public sealed class BiomeContent : MonoBehaviour
{
    [SerializeField] private string _biomeId;

    public string Id => _biomeId;

    public void Initialize(PolygonCollider2D cameraBounds, Camera mainCamera, RespawnManager respawnManager)
    {
        if (gameObject.activeInHierarchy)
            throw new InvalidOperationException("Initialize Biome contents before activating them.");
        if (cameraBounds == null)
            throw new ArgumentNullException(nameof(cameraBounds));
        if (mainCamera == null)
            throw new ArgumentNullException(nameof(mainCamera));
        if (respawnManager == null)
            throw new ArgumentNullException(nameof(respawnManager));

        foreach (BackgroundAuto background in GetComponentsInChildren<BackgroundAuto>(true))
            background.Configure(cameraBounds, mainCamera);
        foreach (ChunkRespawnController chunk in GetComponentsInChildren<ChunkRespawnController>(true))
            chunk.Configure(respawnManager);
    }
}
