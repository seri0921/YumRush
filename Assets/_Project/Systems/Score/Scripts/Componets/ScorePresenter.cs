using System;
using UnityEngine;

public sealed class ScorePresenter : MonoBehaviour
{
    [SerializeField] private ScoreView view;
    private readonly ScoreLogic logic = new ScoreLogic();
    public int Current => logic.Current;
    public event Action<int> Gained;

    private void Awake()
    {
        if (view == null)
            throw new InvalidOperationException("ScorePresenter requires a ScoreView.");
    }

    private void OnEnable()
    {
        logic.Changed += view.Render;
        view.Render(logic.Current);
    }

    private void Start() => logic.Reset();
    private void OnDisable() => logic.Changed -= view.Render;
    public void Add(int points)
    {
        logic.Add(points);
        if (points > 0)
            Gained?.Invoke(points);
    }
}
