using System;
using System.Collections.Generic;

/// <summary>Schedules one-shot events and reserves at most one event per biome slot.</summary>
public sealed class EventScheduleLogic
{
    private readonly EventTiming[] _timings;
    private readonly int[] _locations;
    private readonly bool[] _occupied;
    private readonly List<Transition> _transitions = new List<Transition>();
    private readonly List<int> _candidates = new List<int>();
    private readonly Random _random;
    private int _nextTransition;
    private double _elapsedSeconds;

    public event Action<int, int> Started;
    public event Action<int, int> Ended;
    public event Action<int> Skipped;

    public EventScheduleLogic(IReadOnlyList<EventTiming> timings, int locationCount, Random random)
    {
        if (timings == null)
            throw new ArgumentNullException(nameof(timings));
        if (locationCount < 0)
            throw new ArgumentOutOfRangeException(nameof(locationCount));
        _random = random ?? throw new ArgumentNullException(nameof(random));
        _timings = new EventTiming[timings.Count];
        _locations = new int[timings.Count];
        _occupied = new bool[locationCount];
        for (int index = 0; index < timings.Count; index++)
        {
            EventTiming timing = timings[index];
            // Also reject default(EventTiming), which bypasses its constructor.
            _timings[index] = new EventTiming(timing.Type, timing.StartSeconds, timing.EndSeconds - timing.StartSeconds);
            _locations[index] = -1;
            _transitions.Add(new Transition(index, timing.StartSeconds, true));
            _transitions.Add(new Transition(index, timing.EndSeconds, false));
        }
        _transitions.Sort((left, right) =>
        {
            int comparison = left.Seconds.CompareTo(right.Seconds);
            if (comparison != 0) return comparison;
            // Free locations before starting another event at the same timestamp.
            comparison = left.IsStart.CompareTo(right.IsStart);
            return comparison != 0 ? comparison : left.EventIndex.CompareTo(right.EventIndex);
        });
    }

    public void Advance(double elapsedSeconds, int currentLocation, Func<int, StageEventType, bool> canHost)
    {
        if (double.IsNaN(elapsedSeconds) || double.IsInfinity(elapsedSeconds) || elapsedSeconds < _elapsedSeconds)
            throw new ArgumentOutOfRangeException(nameof(elapsedSeconds));
        if (currentLocation < -1 || currentLocation >= _occupied.Length)
            throw new ArgumentOutOfRangeException(nameof(currentLocation));
        if (canHost == null)
            throw new ArgumentNullException(nameof(canHost));
        _elapsedSeconds = elapsedSeconds;

        while (_nextTransition < _transitions.Count && _transitions[_nextTransition].Seconds <= elapsedSeconds)
        {
            Transition transition = _transitions[_nextTransition++];
            int eventIndex = transition.EventIndex;
            if (!transition.IsStart)
            {
                int location = _locations[eventIndex];
                if (location >= 0)
                {
                    _occupied[location] = false;
                    _locations[eventIndex] = -1;
                    Ended?.Invoke(eventIndex, location);
                }
                continue;
            }

            _candidates.Clear();
            // A fully missed window must not briefly spawn objects after its end.
            if (currentLocation >= 0 && elapsedSeconds < _timings[eventIndex].EndSeconds)
                for (int location = 0; location < _occupied.Length; location++)
                    if (location != currentLocation && !_occupied[location] && canHost(location, _timings[eventIndex].Type))
                        _candidates.Add(location);

            if (_candidates.Count == 0)
            {
                Skipped?.Invoke(eventIndex);
                continue;
            }
            int selectedLocation = _candidates[_random.Next(_candidates.Count)];
            _locations[eventIndex] = selectedLocation;
            _occupied[selectedLocation] = true;
            Started?.Invoke(eventIndex, selectedLocation);
        }
    }

    public void EndActiveEvents()
    {
        for (int eventIndex = 0; eventIndex < _locations.Length; eventIndex++)
        {
            int location = _locations[eventIndex];
            if (location < 0) continue;
            _locations[eventIndex] = -1;
            _occupied[location] = false;
            Ended?.Invoke(eventIndex, location);
        }
    }

    private readonly struct Transition
    {
        public int EventIndex { get; }
        public double Seconds { get; }
        public bool IsStart { get; }

        public Transition(int eventIndex, double seconds, bool isStart)
        {
            EventIndex = eventIndex;
            Seconds = seconds;
            IsStart = isStart;
        }
    }
}
