using System;
using UnityEngine;
using UnityEngine.Assertions;

[DisallowMultipleComponent]
public sealed class ScoreSoundPresenter : MonoBehaviour
{
    [SerializeField] private ScorePresenter _scorePresenter;
    [SerializeField] private ScoreSoundView _view;

    private void Awake()
    {
        if (_scorePresenter == null)
            _scorePresenter = FindFirstObjectByType<ScorePresenter>();
        if (_scorePresenter == null)
            throw new InvalidOperationException("ScoreSoundPresenter requires a ScorePresenter in the scene.");
        Assert.IsNotNull(_view);
    }

    private void OnEnable() => _scorePresenter.Gained += HandleGained;

    private void OnDisable()
    {
        if (_scorePresenter != null)
            _scorePresenter.Gained -= HandleGained;
    }

    private void HandleGained(int points) => _view.PlayGain();
}
