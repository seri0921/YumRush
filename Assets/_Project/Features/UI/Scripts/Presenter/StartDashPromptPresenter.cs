using System;
using UnityEngine;

public sealed class StartDashPromptPresenter : MonoBehaviour
{
    [SerializeField] private StartCountdownPresenter _countdown;
    [SerializeField] private PlayerPresenter _player;
    [SerializeField] private StartDashPromptView _view;

    private bool _isWaitingForDash;
    private bool _hasDashed;

    private void Awake()
    {
        if (_countdown == null || _player == null || _view == null)
            throw new InvalidOperationException("Start dash prompt requires a countdown, player and view.");
    }

    private void OnEnable()
    {
        _countdown.CountdownCompleted += OnCountdownCompleted;
        _player.StateChanged += OnPlayerStateChanged;
    }

    private void OnDisable()
    {
        _countdown.CountdownCompleted -= OnCountdownCompleted;
        _player.StateChanged -= OnPlayerStateChanged;
        _isWaitingForDash = false;
        _view.Hide();
    }

    private void OnCountdownCompleted() => _isWaitingForDash = !_hasDashed;

    private void OnPlayerStateChanged(PlayerState previousState, PlayerState newState)
    {
        if (newState != PlayerState.AirDashing)
            return;

        _hasDashed = true;
        _isWaitingForDash = false;
        _view.Hide();
    }

    private void LateUpdate()
    {
        if (_isWaitingForDash)
            _view.Render(_player.transform.position, Camera.main);
    }
}
