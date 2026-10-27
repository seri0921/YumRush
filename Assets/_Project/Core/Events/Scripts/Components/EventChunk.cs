using System;
using UnityEngine;

/// <summary>Connects event lifetime to one-shot contents or a reusable boss replacement.</summary>
[DisallowMultipleComponent]
public sealed class EventChunk : MonoBehaviour
{
    [SerializeField] private StageEventType _type;
    private bool _used;
    private BossChunkReplacement _replacement;
    private readonly BossChunkStateLogic _bossState = new BossChunkStateLogic();

    public StageEventType Type => _type;
    public bool IsAvailable => (_replacement != null ? _bossState.IsAvailable : !_used) && !gameObject.activeSelf;
    public bool IsBossReplacementShowing => _replacement != null && _bossState.IsShowing;

    public void ValidateConfiguration()
    {
        _replacement = GetComponent<BossChunkReplacement>();
        if (_replacement != null) _replacement.ValidateConfiguration();
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
        if (_replacement != null)
        {
            _replacement.Show();
            _bossState.Begin();
        }
        else _used = true;
        gameObject.SetActive(true);
    }

    public void End()
    {
        if (_replacement != null)
        {
            _bossState.EndEvent();
            return;
        }
        gameObject.SetActive(false);
        Destroy(gameObject);
    }

    public void RestoreIfPlayerIsElsewhere(int location, int playerLocation)
    {
        if (_replacement != null && _bossState.TryRestore(location, playerLocation))
            RestoreReplacement();
    }

    private void RestoreReplacement()
    {
        gameObject.SetActive(false);
        _replacement.Restore();
    }
}
