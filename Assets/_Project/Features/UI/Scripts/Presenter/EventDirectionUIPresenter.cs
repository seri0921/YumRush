using UnityEngine;

[DisallowMultipleComponent]
public sealed class EventDirectionUIPresenter : MonoBehaviour
{
    [SerializeField] private StageEventPresenter _eventSource;
    [SerializeField] private EventDirectionUIView _view;
    [SerializeField, Min(0.1f)] private float _displaySeconds = 5f;

    private BiomeSlot _target;
    private float _startedAt;

    private void OnEnable() => _eventSource.EventDirectionStarted += Show;

    private void OnDisable()
    {
        _eventSource.EventDirectionStarted -= Show;
        _target = null;
        _view.Hide();
    }

    private void Show(StageEventType type, BiomeSlot target)
    {
        _target = target;
        _startedAt = Time.time;
        _view.SetEvent(type);
    }

    private void LateUpdate()
    {
        if (_target == null) return;

        float elapsed = Time.time - _startedAt;
        if (elapsed >= _displaySeconds)
        {
            _target = null;
            _view.Hide();
            return;
        }

        Camera camera = Camera.main;
        if (camera == null)
        {
            _view.Hide();
            return;
        }

        Vector3 player = camera.WorldToScreenPoint(_eventSource.Player.position);
        Vector3 destination = camera.WorldToScreenPoint(_target.CameraBounds.bounds.center);
        Vector2 direction = destination - player;
        if (player.z <= 0f || direction.sqrMagnitude < 1f)
        {
            _view.Hide();
            return;
        }

        _view.Render(player, direction.normalized, elapsed);
    }
}
