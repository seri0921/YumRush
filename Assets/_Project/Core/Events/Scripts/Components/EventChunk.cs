using System;
using UnityEngine;

/// <summary>Owns one preplaced, one-shot group of event enemies or items.</summary>
[DisallowMultipleComponent]
public sealed class EventChunk : MonoBehaviour
{
    [SerializeField] private StageEventType _type;
    private bool _used;

    public StageEventType Type => _type;
    public bool IsAvailable => !_used && this != null && !gameObject.activeSelf;

    public void ValidateConfiguration()
    {
        if (gameObject.activeSelf || !Enum.IsDefined(typeof(StageEventType), _type))
            throw new InvalidOperationException($"{name}: EventChunk must have a valid type and start inactive.");
        if (GetComponentsInChildren<ChunkRespawnController>(true).Length != 0 ||
            GetComponentsInChildren<SpawnPoint>(true).Length != 0 ||
            GetComponentsInChildren<RespawnTarget>(true).Length != 0)
            throw new InvalidOperationException($"{name}: Place items and bosses directly, without respawn components.");
        if (GetComponentsInChildren<EventChunk>(true).Length != 1)
            throw new InvalidOperationException($"{name}: Nested EventChunks are not supported.");
    }

    public void Begin()
    {
        if (!IsAvailable)
            throw new InvalidOperationException($"{name}: EventChunk has already been used or is active.");
        _used = true;
        gameObject.SetActive(true);
    }

    public void End()
    {
        gameObject.SetActive(false);
        Destroy(gameObject);
    }
}
