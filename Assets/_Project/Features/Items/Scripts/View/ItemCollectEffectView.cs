using System;
using UnityEngine;

public sealed class ItemCollectEffectView : MonoBehaviour
{
    [SerializeField] private ItemCollectEffectPlayback _effectPrefab;

    private void Awake()
    {
        if (_effectPrefab == null)
            throw new InvalidOperationException("ItemCollectEffectView requires an effect prefab.");
    }

    public void Play(Vector3 position)
    {
        // The effect must survive the collected item's destruction.
        Instantiate(_effectPrefab, position, Quaternion.identity);
    }
}
