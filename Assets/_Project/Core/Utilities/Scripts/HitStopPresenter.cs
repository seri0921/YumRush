using UnityEngine;

// Owns the temporary global time stop; effects and audio use their own playback clocks.
[DisallowMultipleComponent]
[DefaultExecutionOrder(-1000)]
public sealed class HitStopPresenter : MonoBehaviour
{
    [SerializeField, Min(0f)] private float _durationSeconds = 0.08f;
    private readonly HitStopLogic _logic = new HitStopLogic();
    private static HitStopPresenter _active;
    private float _previousTimeScale;
    private double _lastTickTime;

    public static bool IsStopping => _active != null;

    public void Request()
    {
        if (!isActiveAndEnabled || _durationSeconds <= 0f)
            return;

        // A second player must not capture zero as the scale to restore.
        if (_active != null && _active != this)
        {
            _active._logic.Request(_durationSeconds);
            return;
        }
        if (_active == null)
        {
            if (Time.timeScale == 0f)
                return;
            _previousTimeScale = Time.timeScale;
            _lastTickTime = Time.realtimeSinceStartupAsDouble;
            _active = this;
            Time.timeScale = 0f;
        }
        _logic.Request(_durationSeconds);
    }

    private void Update()
    {
        if (_active != this)
            return;

        double now = Time.realtimeSinceStartupAsDouble;
        _logic.Tick(now - _lastTickTime);
        _lastTickTime = now;
        if (!_logic.IsActive)
            Release();
    }

    private void OnDisable() => Release();

    private void Release()
    {
        if (_active != this)
            return;

        // Disabling or destroying the owner must never leave the game stopped.
        Time.timeScale = _previousTimeScale;
        _active = null;
        _logic.Clear();
    }
}
