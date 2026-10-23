using System;
using System.Collections.Generic;
using UnityEngine;

public sealed class RespawnManager : MonoBehaviour
{
    private readonly List<ChunkRespawnController> chunks = new List<ChunkRespawnController>();

    public void Register(ChunkRespawnController chunk)
    {
        if (chunk == null)
            throw new ArgumentNullException(nameof(chunk));
        if (chunks.Contains(chunk))
            throw new InvalidOperationException("Chunk is already registered.");
        chunks.Add(chunk);
    }

    public void Unregister(ChunkRespawnController chunk) => chunks.Remove(chunk);

    public void RespawnChunk(ChunkRespawnController chunk)
    {
        if (chunk == null)
            throw new ArgumentNullException(nameof(chunk));
        if (!chunks.Contains(chunk))
            throw new InvalidOperationException("Chunk is not registered with this manager.");
        chunk.RespawnAll();
    }

    private void Update()
    {
        // Scaled time freezes both clocks when timeScale is zero.
        if (Time.deltaTime <= 0f)
            return;
        for (int i = chunks.Count - 1; i >= 0; i--)
            chunks[i].Tick(Time.deltaTime);
    }
}
