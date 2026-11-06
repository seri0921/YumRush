using System;
using System.Collections;
using UnityEngine;

public sealed class StartCountdownPresenter : MonoBehaviour
{
    [SerializeField] private GameManager _gameManager;
    [SerializeField] private PlayerPresenter _player;
    [SerializeField] private StartCountdownView _view;
    [SerializeField, Min(0f)] private float _threeAt;
    [SerializeField, Min(0f)] private float _twoAt = 1f;
    [SerializeField, Min(0f)] private float _oneAt = 2f;
    [SerializeField, Min(0f)] private float _goAt = 3f;
    [SerializeField, Min(0.01f)] private float _goDuration = 1f;

    private IDisposable _inputLock;

    private void Awake()
    {
        if (_gameManager == null || _player == null || _view == null)
            throw new InvalidOperationException("Start countdown requires a game manager, player and view.");
        if (!(_threeAt >= 0f && _threeAt < _twoAt && _twoAt < _oneAt && _oneAt < _goAt &&
              float.IsFinite(_goAt) && _goDuration > 0f && float.IsFinite(_goDuration)))
            throw new InvalidOperationException("Countdown timestamps must be finite and increasing, with a positive GO duration.");
    }

    private IEnumerator Start()
    {
        _inputLock = _player.LockOperation();
        while (SceneTransitionPresenter.IsTransitioning)
            yield return null;

        yield return _view.PrepareSound();

        // Allow the audio thread to prepare playback before the shared start time.
        double startedAt = AudioSettings.dspTime + 0.1d;
        _view.ScheduleSound(startedAt);
        while (AudioSettings.dspTime - startedAt < _goAt)
        {
            double elapsed = AudioSettings.dspTime - startedAt;
            if (elapsed >= _oneAt)
                _view.Show("1");
            else if (elapsed >= _twoAt)
                _view.Show("2");
            else if (elapsed >= _threeAt)
                _view.Show("3");

            yield return null;
        }

        _view.Show("GO!");
        _gameManager.BeginSession();
        ReleaseInput();

        yield return new WaitForSecondsRealtime(_goDuration);
        _view.Hide();
    }

    private void OnDestroy() => ReleaseInput();

    private void ReleaseInput()
    {
        _inputLock?.Dispose();
        _inputLock = null;
    }
}
