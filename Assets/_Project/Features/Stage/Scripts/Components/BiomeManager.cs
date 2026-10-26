using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>Coordinates the initial layout and its Unity scene representation.</summary>
[DisallowMultipleComponent]
[DefaultExecutionOrder(-100)]
public sealed class BiomeManager : MonoBehaviour
{
    [Tooltip("OFF keeps the Biomes placed in the Editor. Read once when the game starts.")]
    [SerializeField] private bool _randomizeOnStart = true;
    [SerializeField] private BiomeContent[] _availableBiomes;
    [SerializeField] private BiomeSlot[] _slots;
    [SerializeField] private RespawnManager _respawnManager;
    [SerializeField] private Camera _mainCamera;

    private void Awake()
    {
        if (!_randomizeOnStart)
            return;
        if (_respawnManager == null || _mainCamera == null || _availableBiomes == null || _slots == null)
            throw new InvalidOperationException($"{name}: Biome candidates, Slots, RespawnManager and Main Camera are required.");

        var slotIds = new HashSet<string>(StringComparer.Ordinal);
        foreach (BiomeSlot slot in _slots)
        {
            if (slot == null)
                throw new InvalidOperationException($"{name}: A Slot reference is missing.");
            slot.ValidateConfiguration();
            if (!slotIds.Add(slot.Id))
                throw new InvalidOperationException($"{name}: Slot IDs must be unique.");
        }

        var candidateIds = new string[_availableBiomes.Length];
        for (int index = 0; index < _availableBiomes.Length; index++)
        {
            if (_availableBiomes[index] == null)
                throw new InvalidOperationException($"{name}: A Biome candidate is missing.");
            candidateIds[index] = _availableBiomes[index].Id;
        }
        string[] layout = new BiomeLogic().CreateLayout(candidateIds, _slots.Length, new System.Random());
        var prefabsById = new Dictionary<string, BiomeContent>(StringComparer.Ordinal);
        foreach (BiomeContent prefab in _availableBiomes)
            prefabsById.Add(prefab.Id, prefab);

        // An inactive parent prevents OnEnable from running before scene references are assigned.
        var staging = new GameObject("BiomeInitialization");
        staging.SetActive(false);
        staging.transform.SetParent(transform, false);
        try
        {
            var instances = new BiomeContent[_slots.Length];
            for (int index = 0; index < _slots.Length; index++)
            {
                BiomeContent prefab = prefabsById[layout[index]];
                BiomeContent instance = Instantiate(prefab, staging.transform, false);
                instance.name = prefab.name;
                instance.Initialize(_slots[index].CameraBounds, _mainCamera, _respawnManager);
                instances[index] = instance;
            }
            // Prepare every replacement successfully before removing any Editor-placed Biome.
            for (int index = 0; index < _slots.Length; index++)
                _slots[index].Place(instances[index]);
        }
        finally
        {
            Destroy(staging);
        }
    }
}
