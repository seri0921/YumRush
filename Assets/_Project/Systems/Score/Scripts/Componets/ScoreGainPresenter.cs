using System;
using System.Collections.Generic;
using UnityEngine;

public sealed class ScoreGainPresenter : MonoBehaviour
{
    [SerializeField] private ScorePresenter _scorePresenter;
    [SerializeField] private ScoreGainView _gainPrefab;
    [SerializeField] private Vector3 _positionOffset = new Vector3(0f, 1.5f, 0f);
    [SerializeField] private Vector3 _stackOffset = new Vector3(0.25f, 0.8f, 0f);

    private readonly List<ScoreGainView> _activeViews = new List<ScoreGainView>();

    private void Awake()
    {
        if (_scorePresenter == null)
            _scorePresenter = FindFirstObjectByType<ScorePresenter>();
        if (_scorePresenter == null)
            throw new InvalidOperationException("ScoreGainPresenter requires a ScorePresenter in the scene.");
        if (_gainPrefab == null)
            throw new InvalidOperationException("ScoreGainPresenter requires a ScoreGainView prefab.");
    }

    private void OnEnable() => _scorePresenter.Gained += ShowGain;

    private void OnDisable()
    {
        _scorePresenter.Gained -= ShowGain;
        foreach (ScoreGainView view in _activeViews)
        {
            if (view != null)
                Destroy(view.gameObject);
        }
        _activeViews.Clear();
    }

    private void ShowGain(int points)
    {
        // Reuse vacant display slots so simultaneous gains have distinct positions.
        int slot = _activeViews.FindIndex(view => view == null);
        if (slot < 0)
        {
            slot = _activeViews.Count;
            _activeViews.Add(null);
        }

        Vector3 position = transform.position + _positionOffset + _stackOffset * slot;
        ScoreGainView view = Instantiate(_gainPrefab, position, Quaternion.identity);
        _activeViews[slot] = view;
        view.Show(points);
    }
}
