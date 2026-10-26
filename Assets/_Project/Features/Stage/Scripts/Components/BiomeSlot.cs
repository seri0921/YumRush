using System;
using UnityEngine;

/// <summary>Owns one placement location and the biome currently occupying it.</summary>
[DisallowMultipleComponent]
public sealed class BiomeSlot : MonoBehaviour
{
    [SerializeField] private string _slotId;
    [SerializeField] private Transform _content;
    [SerializeField] private PolygonCollider2D _cameraBounds;
    [SerializeField] private BiomeContent _currentBiome;

    public string Id => _slotId;
    public PolygonCollider2D CameraBounds => _cameraBounds;
    public BiomeContent CurrentBiome => _currentBiome;

    internal void ValidateConfiguration()
    {
        if (string.IsNullOrWhiteSpace(_slotId) || _content == null || _cameraBounds == null || _currentBiome == null)
            throw new InvalidOperationException($"{name}: Slot ID, Content, CameraBounds and current Biome are required.");
        if (_content.parent != transform || _cameraBounds.transform.parent != transform ||
            _content.childCount != 1 || _currentBiome.transform.parent != _content)
            throw new InvalidOperationException($"{name}: Expected one Biome under this Slot's Content.");
    }

    internal void Place(BiomeContent biome)
    {
        if (biome == null)
            throw new ArgumentNullException(nameof(biome));
        if (biome.gameObject.activeInHierarchy)
            throw new InvalidOperationException("Place the initialized Biome before activating it.");

        // Destroy is deferred; disable the old contents immediately to stop physics and respawning.
        _currentBiome.gameObject.SetActive(false);
        Destroy(_currentBiome.gameObject);
        biome.gameObject.SetActive(false);
        biome.transform.SetParent(_content, false);
        biome.transform.localPosition = Vector3.zero;
        biome.transform.localRotation = Quaternion.identity;
        biome.transform.localScale = Vector3.one;
        _currentBiome = biome;
        biome.gameObject.SetActive(true);
    }
}
