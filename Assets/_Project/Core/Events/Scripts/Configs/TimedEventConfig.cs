using UnityEngine;

[CreateAssetMenu(menuName = "Stage Events/Timed Event")]
public sealed class TimedEventConfig : ScriptableObject
{
    [SerializeField] private StageEventType _type;
    [SerializeField, Min(0f)] private float _startSeconds = 60f;
    [SerializeField, Min(0.01f)] private float _durationSeconds = 15f;

    public StageEventType Type => _type;
    public EventTiming CreateTiming() => new EventTiming(_type, _startSeconds, _durationSeconds);
}
