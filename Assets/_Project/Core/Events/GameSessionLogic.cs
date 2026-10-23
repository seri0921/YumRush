using System;

public enum GameStatus
{
    Ready,
    Playing,
    Paused,
    GameOver
}

/// <summary>Owns session transitions; presentation and Unity operations belong to the caller.</summary>
public sealed class GameSessionLogic
{
    private readonly GameTimeLogic time = new GameTimeLogic();
    public GameStatus Status { get; private set; } = GameStatus.Ready;
    public double ElapsedSeconds => time.ElapsedSeconds;
    public double RemainingSeconds => time.RemainingSeconds;
    public event Action<double> TimeAdvanced;
    public event Action<GameStatus> StatusChanged;
    public event Action GameOver;

    public void Start(double durationSeconds)
    {
        if (Status != GameStatus.Ready && Status != GameStatus.GameOver)
            throw new InvalidOperationException("A session can only start from Ready or GameOver.");

        time.Reset(durationSeconds);
        ChangeStatus(GameStatus.Playing);
    }

    public void Tick(double deltaSeconds)
    {
        if (double.IsNaN(deltaSeconds) || double.IsInfinity(deltaSeconds) || deltaSeconds < 0d)
            throw new ArgumentOutOfRangeException(nameof(deltaSeconds));
        if (Status != GameStatus.Playing || deltaSeconds == 0d)
            return;

        time.Advance(deltaSeconds);
        // Commit the terminal state before notifying listeners.
        bool expired = time.IsExpired;
        if (expired)
            Status = GameStatus.GameOver;

        TimeAdvanced?.Invoke(time.ElapsedSeconds);
        if (!expired)
            return;

        StatusChanged?.Invoke(Status);
        GameOver?.Invoke();
    }

    public void Pause()
    {
        RequireStatus(GameStatus.Playing);
        ChangeStatus(GameStatus.Paused);
    }

    public void Resume()
    {
        RequireStatus(GameStatus.Paused);
        ChangeStatus(GameStatus.Playing);
    }

    private void RequireStatus(GameStatus expected)
    {
        if (Status != expected)
            throw new InvalidOperationException($"Expected {expected}, but the session is {Status}.");
    }

    private void ChangeStatus(GameStatus status)
    {
        Status = status;
        StatusChanged?.Invoke(status);
    }
}
