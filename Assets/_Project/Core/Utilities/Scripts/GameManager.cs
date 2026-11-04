using System;
using System.Collections;
using UnityEngine;

[DisallowMultipleComponent]
public sealed class GameManager : MonoBehaviour
{
    [SerializeField, Min(0.01f)] private float timeLimitSeconds = 180f;
    [SerializeField] private ScorePresenter scorePresenter;
    [SerializeField] private bool waitForCountdown;
    [SerializeField] private EndCountdownPresenter endCountdown;
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
    private void Start()
    {
        if (!waitForCountdown)
            BeginSession();
    }

    private void Update()
    {
        session.Tick(Time.deltaTime);
        if (endCountdown != null && Status == GameStatus.Playing)
            endCountdown.RenderRemaining(RemainingSeconds);
    }
    private void OnDestroy() => session.GameOver -= HandleGameOver;

    public void BeginSession() => session.Start(timeLimitSeconds);

    // These pause the session clock only. Global gameplay pause belongs to a separate controller.
    public void PauseTimer() => session.Pause();
    public void ResumeTimer() => session.Resume();

    private void HandleGameOver()
    {
        Debug.Log("Game over: time limit reached.", this);
        // Only scenes wired for results should leave the current scene.
        if (scorePresenter == null)
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#endif
            return;
        }

        ResultScorePresenter.FinalScore = scorePresenter.Current;
        if (endCountdown != null)
        {
            StartCoroutine(ShowTimeUpAndLoadResult());
            return;
        }

        LoadResult();
    }

    private IEnumerator ShowTimeUpAndLoadResult()
    {
        yield return endCountdown.ShowTimeUp();
        LoadResult();
    }

    private static void LoadResult()
    {
        SceneTransitionPresenter.LoadScene("Assets/_Project/Scenes/Result.unity");
    }
}
