using System;
using UnityEngine;

/// <summary>Owns the normal terrain's saved activation state and a fresh boss Chunk instance.</summary>
[DisallowMultipleComponent]
[RequireComponent(typeof(EventChunk))]
public sealed class BossChunkReplacement : MonoBehaviour
{
    [SerializeField] private GameObject[] _normalChunks;
    [SerializeField] private GameObject _bossChunkPrefab;
    private GameObject _instance;
    private bool[] _normalActiveStates;

    public void ValidateConfiguration()
    {
        BiomeContent biome = GetComponentInParent<BiomeContent>(true);
        if (biome == null || _bossChunkPrefab == null || _bossChunkPrefab.scene.IsValid() ||
            _normalChunks == null || _normalChunks.Length == 0)
            throw new InvalidOperationException($"{name}: A Biome, boss Chunk prefab asset and normal Chunks are required.");
        if (GetComponent<EventChunk>().Type != StageEventType.Boss || transform.childCount != 0)
            throw new InvalidOperationException($"{name}: Use an empty Boss EventChunk as the replacement anchor.");
        if (_bossChunkPrefab.GetComponentsInChildren<EventChunk>(true).Length != 0 ||
            _bossChunkPrefab.GetComponentsInChildren<ChunkRespawnController>(true).Length != 0 ||
            _bossChunkPrefab.GetComponentsInChildren<SpawnPoint>(true).Length != 0 ||
            _bossChunkPrefab.GetComponentsInChildren<RespawnTarget>(true).Length != 0)
            throw new InvalidOperationException($"{name}: Boss prefab contents must not contain event or respawn components.");
        for (int index = 0; index < _normalChunks.Length; index++)
        {
            GameObject chunk = _normalChunks[index];
            if (chunk == null || chunk.GetComponentInParent<BiomeContent>(true) != biome ||
                chunk == biome.gameObject || transform.IsChildOf(chunk.transform) ||
                chunk.transform.IsChildOf(transform) || chunk.GetComponentsInChildren<EventChunk>(true).Length != 0)
                throw new InvalidOperationException($"{name}: Normal Chunks must be separate roots in the same Biome, outside event anchors.");
            for (int previous = 0; previous < index; previous++)
                if (chunk.transform.IsChildOf(_normalChunks[previous].transform) ||
                    _normalChunks[previous].transform.IsChildOf(chunk.transform))
                    throw new InvalidOperationException($"{name}: Normal Chunk references must not overlap.");
        }
    }

    public void Show()
    {
        if (_normalActiveStates != null)
            throw new InvalidOperationException("The boss Chunk is already showing.");
        // The anchor is still inactive: Awake/OnEnable run only after normal terrain is hidden.
        _instance = Instantiate(_bossChunkPrefab, transform, false);
        _instance.SetActive(true);
        _normalActiveStates = new bool[_normalChunks.Length];
        for (int index = 0; index < _normalChunks.Length; index++)
        {
            _normalActiveStates[index] = _normalChunks[index].activeSelf;
            _normalChunks[index].SetActive(false);
        }
    }

    public void Restore()
    {
        if (_normalActiveStates == null) return;
        if (_instance != null)
        {
            _instance.SetActive(false);
            Destroy(_instance);
        }
        _instance = null;
        for (int index = 0; index < _normalChunks.Length; index++)
            if (_normalChunks[index] != null)
                _normalChunks[index].SetActive(_normalActiveStates[index]);
        _normalActiveStates = null;
    }
}
