using System;
using UnityEngine;

/// <summary>Connects the session clock and biome scene contents to event scheduling.</summary>
[DisallowMultipleComponent]
public sealed class StageEventPresenter : MonoBehaviour
{
    [SerializeField] private GameManager _gameManager;
    [SerializeField] private Transform _player;
    [SerializeField] private BiomeSlot[] _slots;
    [SerializeField] private TimedEventConfig[] _events;

    private EventScheduleLogic _logic;
    private EventChunk[][] _chunks;
    private EventChunk[] _activeChunks;
    private EventTiming[] _timings;
    private bool _subscribed;

    private void Start()
    {
        if (_gameManager == null || _player == null || _slots == null || _events == null)
            throw new InvalidOperationException($"{name}: GameManager, Player, Slots and Events are required.");

        _chunks = new EventChunk[_slots.Length][];
        for (int index = 0; index < _slots.Length; index++)
        {
            BiomeSlot slot = _slots[index];
            if (slot == null || slot.CameraBounds == null || slot.CurrentBiome == null)
                throw new InvalidOperationException($"{name}: Each Slot needs bounds and its current Biome.");
            for (int previous = 0; previous < index; previous++)
                if (_slots[previous] == slot)
                    throw new InvalidOperationException($"{name}: Duplicate Slot references are not allowed.");
            _chunks[index] = slot.CurrentBiome.GetComponentsInChildren<EventChunk>(true);
            foreach (EventChunk chunk in _chunks[index])
                chunk.ValidateConfiguration();
        }

        _timings = new EventTiming[_events.Length];
        for (int index = 0; index < _events.Length; index++)
        {
            if (_events[index] == null)
                throw new InvalidOperationException($"{name}: An event configuration is missing.");
            _timings[index] = _events[index].CreateTiming();
        }
        _activeChunks = new EventChunk[_events.Length];
        _logic = new EventScheduleLogic(_timings, _slots.Length, new System.Random());
        _logic.Started += HandleStarted;
        _logic.Ended += HandleEnded;
        _logic.Skipped += HandleSkipped;
        Subscribe();
        // Start executes after BiomeManager.Awake; zero-second events are supported.
        HandleTimeAdvanced(_gameManager.ElapsedSeconds);
    }

    private void OnEnable()
    {
        if (_logic != null)
        {
            Subscribe();
            HandleTimeAdvanced(_gameManager.ElapsedSeconds);
        }
    }

    private void Subscribe()
    {
        if (_subscribed) return;
        _gameManager.TimeAdvanced += HandleTimeAdvanced;
        _gameManager.GameOver += HandleGameOver;
        _subscribed = true;
    }

    private void OnDisable()
    {
        if (_subscribed && _gameManager != null)
        {
            _gameManager.TimeAdvanced -= HandleTimeAdvanced;
            _gameManager.GameOver -= HandleGameOver;
        }
        _subscribed = false;
        EndAllChunks();
    }

    private void HandleTimeAdvanced(double elapsedSeconds)
    {
        if (_gameManager.Status == GameStatus.GameOver)
        {
            EndAllChunks();
            return;
        }
        int currentLocation = -1;
        Vector2 position = _player.position;
        for (int index = 0; index < _slots.Length; index++)
        {
            if (!_slots[index].CameraBounds.OverlapPoint(position)) continue;
            // Ambiguous room boundaries are treated as unknown, never guessed.
            if (currentLocation >= 0)
            {
                currentLocation = -1;
                break;
            }
            currentLocation = index;
        }
        _logic.Advance(elapsedSeconds, currentLocation, CanHost);
    }

    private bool CanHost(int location, StageEventType type) => FindAvailableChunk(location, type) != null;

    private EventChunk FindAvailableChunk(int location, StageEventType type)
    {
        if (!_slots[location].CurrentBiome.gameObject.activeInHierarchy) return null;
        foreach (EventChunk chunk in _chunks[location])
            if (chunk != null && chunk.Type == type && chunk.IsAvailable &&
                chunk.transform.parent != null && chunk.transform.parent.gameObject.activeInHierarchy)
                return chunk;
        return null;
    }

    private void HandleStarted(int eventIndex, int location)
    {
        EventChunk chunk = FindAvailableChunk(location, _timings[eventIndex].Type);
        if (chunk == null)
            throw new InvalidOperationException("Selected event placement is no longer available.");
        _activeChunks[eventIndex] = chunk;
        chunk.Begin();
    }

    private void HandleEnded(int eventIndex, int location)
    {
        EventChunk chunk = _activeChunks[eventIndex];
        if (chunk != null) chunk.End();
        _activeChunks[eventIndex] = null;
    }

    private void HandleSkipped(int eventIndex) =>
        Debug.Log($"Stage event skipped: {_events[eventIndex].name} (no eligible location or expired window).", this);

    private void HandleGameOver() => EndAllChunks();

    private void EndAllChunks()
    {
        _logic?.EndActiveEvents();
    }
}
