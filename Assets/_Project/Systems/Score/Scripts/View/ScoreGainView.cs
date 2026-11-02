using System;
using DG.Tweening;
using TMPro;
using UnityEngine;

public sealed class ScoreGainView : MonoBehaviour
{
    [SerializeField] private TMP_Text _text;
    [SerializeField, Min(0f)] private float _fadeInDuration = 0.15f;
    [SerializeField, Min(0f)] private float _holdDuration = 0.5f;
    [SerializeField, Min(0f)] private float _fadeOutDuration = 0.3f;

    private Sequence _sequence;

    private void Awake()
    {
        if (_text == null)
            throw new InvalidOperationException("ScoreGainView requires TMP text.");
    }

    public void Show(int points)
    {
        _sequence?.Kill();
        _text.text = points.ToString("N0");
        _text.alpha = 0f;
        _sequence = DOTween.Sequence()
            .Append(DOTween.To(() => _text.alpha, alpha => _text.alpha = alpha, 1f, _fadeInDuration))
            .AppendInterval(_holdDuration)
            .Append(DOTween.To(() => _text.alpha, alpha => _text.alpha = alpha, 0f, _fadeOutDuration))
            .OnComplete(() => Destroy(gameObject));
    }

    private void OnDestroy() => _sequence?.Kill();
}
