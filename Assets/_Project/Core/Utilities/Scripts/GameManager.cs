using System;
using UnityEngine;

[DisallowMultipleComponent]
public sealed class GameManager : MonoBehaviour
{
    [SerializeField, Min(0.01f)] private float timeLimitSeconds = 180f;
    private readonly GameSessionLogic session = new GameSessionLogic();

    public GameStatus Status => session.Status;
    public double TimeLimitSeconds => timeLimitSeconds;
    public double ElapsedSeconds => session.ElapsedSeconds;
    public double RemainingSeconds => session.RemainingSeconds;

    public event Action<double> TimeAdvanced
    {
        add => session.TimeAdvanced += value;
        remove => session.TimeAdvanced -= value;
    }

    public event Action GameOver
    {
        add => session.GameOver += value;
        remove => session.GameOver -= value;
    }

    private void Awake() => session.GameOver += HandleGameOver;
    private void Start() => session.Start(timeLimitSeconds);
    private void Update() => session.Tick(Time.deltaTime);
    private void OnDestroy() => session.GameOver -= HandleGameOver;

    // These pause the session clock only. Global gameplay pause belongs to a separate controller.
    public void PauseTimer() => session.Pause();
    public void ResumeTimer() => session.Resume();

    private void HandleGameOver()
    {
        Debug.Log("Game over: time limit reached.", this);
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}
