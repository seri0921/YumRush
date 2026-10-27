using System;
using UnityEngine;

/// <summary>Routes stage event locations to their minimap highlights.</summary>
[DisallowMultipleComponent]
public sealed class EventMapUIPresenter : MonoBehaviour
{
    [SerializeField] private StageEventPresenter _eventSource;
    [SerializeField] private EventMapUIView _view;
    [Tooltip("発光画像と同じ順番で配置地点を設定します。")]
    [SerializeField] private BiomeSlot[] _slots;

    private void Awake()
    {
        if (_eventSource == null || _view == null || _slots == null ||
            _slots.Length != _view.LocationCount)
            throw new InvalidOperationException($"{name}: Event source, View and matching Slots are required.");
        for (int index = 0; index < _slots.Length; index++)
        {
            if (_slots[index] == null)
                throw new InvalidOperationException($"{name}: A Slot reference is missing.");
            for (int previous = 0; previous < index; previous++)
                if (_slots[previous] == _slots[index])
                    throw new InvalidOperationException($"{name}: Duplicate Slots are not allowed.");
        }
    }

    private void OnEnable() => _eventSource.EventStarted += HandleEventStarted;

    private void OnDisable()
    {
        if (_eventSource != null)
            _eventSource.EventStarted -= HandleEventStarted;
    }

    private void HandleEventStarted(BiomeSlot slot)
    {
        int index = Array.IndexOf(_slots, slot);
        if (index < 0)
            throw new InvalidOperationException($"{name}: The event location has no minimap highlight.");
        _view.PlayHighlight(index);
    }
}
