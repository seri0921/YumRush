using System;
using UnityEngine;

public sealed class TimerPresenter : MonoBehaviour
{
    [SerializeField] private GameManager gameManager;
    [SerializeField] private TimerView view;

    private void Awake()
    {
        if (gameManager == null)
            throw new InvalidOperationException("TimerPresenter requires a GameManager.");
        if (view == null)
            throw new InvalidOperationException("TimerPresenter requires a TimerView.");
    }

    private void OnEnable()
    {
        gameManager.TimeAdvanced += HandleTimeAdvanced;
        Refresh();
    }

    private void OnDisable() => gameManager.TimeAdvanced -= HandleTimeAdvanced;
    private void HandleTimeAdvanced(double elapsedSeconds) => Refresh();

    private void Refresh()
    {
        // OnEnable may run before GameManager.Start initializes the session.
        double remaining = gameManager.Status == GameStatus.Ready
            ? gameManager.TimeLimitSeconds
            : gameManager.RemainingSeconds;
        view.Render(remaining);
    }
}
