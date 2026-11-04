using System;
using UnityEngine;

public sealed class EndCountdownPresenter : MonoBehaviour
{
    [SerializeField] private PlayerPresenter _player;
    [SerializeField] private EndCountdownView _view;
    [SerializeField, Min(1)] private int _countdownSeconds = 3;
    [SerializeField, Min(0.01f)] private float _timeUpDuration = 1f;

    private int _displayedSeconds;
    private IDisposable _inputLock;

    private void Awake()
    {
        if (_player == null || _view == null)
            throw new InvalidOperationException("End countdown requires a player and view.");
    }

    public void RenderRemaining(double remainingSeconds)
    {
        if (remainingSeconds > _countdownSeconds)
            return;

        int seconds = (int)Math.Ceiling(remainingSeconds);
        if (seconds <= 0 || seconds == _displayedSeconds)
            return;

        _displayedSeconds = seconds;
        _view.ShowCount(seconds);
    }

    public WaitForSecondsRealtime ShowTimeUp()
    {
        _inputLock = _player.LockOperation();
        _view.ShowTimeUp();
        return new WaitForSecondsRealtime(_timeUpDuration);
    }

    private void OnDestroy() => _inputLock?.Dispose();
}
