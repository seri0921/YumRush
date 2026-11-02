using System;
using UnityEngine;

public sealed class ScorePresenter : MonoBehaviour
{
    [SerializeField] private ScoreView view;
    [SerializeField] private GameManager gameManager;
    [SerializeField, Min(0f)] private float hideThresholdSeconds = 10f;
    private readonly ScoreLogic logic = new ScoreLogic();
    private ScoreDisplayLogic displayLogic;
    public int Current => logic.Current;
    public event Action<int> Gained;

    private void Awake()
    {
        if (view == null)
            throw new InvalidOperationException("ScorePresenter requires a ScoreView.");
        if (gameManager == null)
            gameManager = FindFirstObjectByType<GameManager>();
        if (gameManager == null)
            throw new InvalidOperationException("ScorePresenter requires a GameManager.");

        displayLogic = new ScoreDisplayLogic(hideThresholdSeconds);
    }

    private void OnEnable()
    {
        logic.Changed += HandleScoreChanged;
        gameManager.TimeAdvanced += HandleTimeAdvanced;
        Refresh();
    }

    private void Start() => logic.Reset();
    private void OnDisable()
    {
        logic.Changed -= HandleScoreChanged;
        gameManager.TimeAdvanced -= HandleTimeAdvanced;
    }

    private void HandleScoreChanged(int score) => Refresh();
    private void HandleTimeAdvanced(double elapsedSeconds) => Refresh();

    private void Refresh()
    {
        // OnEnable may run before GameManager.Start initializes the session.
        double remaining = gameManager.Status == GameStatus.Ready
            ? gameManager.TimeLimitSeconds
            : gameManager.RemainingSeconds;
        view.Render(logic.Current, displayLogic.ShouldHide(remaining));
    }

    public void Add(int points)
    {
        logic.Add(points);
        if (points > 0)
            Gained?.Invoke(points);
    }
}
