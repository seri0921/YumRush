using System;
using UnityEngine;

/// <summary>Added to generated roots. Unmanaged items/enemies need no component.</summary>
public sealed class RespawnTarget : MonoBehaviour
{
    private SpawnPoint owner;
    private bool consumed;

    public void Initialize(SpawnPoint spawnPoint)
    {
        if (spawnPoint == null)
            throw new ArgumentNullException(nameof(spawnPoint));
        if (owner != null)
            throw new InvalidOperationException("RespawnTarget is already bound.");
        owner = spawnPoint;
    }

    public void NotifyConsumed()
    {
        if (owner == null)
            throw new InvalidOperationException("RespawnTarget requires a SpawnPoint.");
        if (consumed)
            return;
        consumed = true;
        owner.NotifyConsumed(this);
    }
}
