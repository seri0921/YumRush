using System;
using UnityEngine;

/// <summary>Animates independent minimap highlight images.</summary>
[DisallowMultipleComponent]
public sealed class EventMapUIView : MonoBehaviour
{
    [SerializeField] private UnityEngine.UI.Image[] _highlights;
    [SerializeField, Min(1)] private int _repeatCount = 3;
    [Tooltip("透明から最大アルファまでの秒数。消灯にも同じ時間を使います。")]
    [SerializeField, Min(0.01f)] private float _fadeDuration = 0.5f;
    [SerializeField, Range(0f, 1f)] private float _maximumAlpha = 0.8f;

    private float[] _elapsed;
    private bool[] _playing;

    public int LocationCount => _highlights.Length;

    private void Awake()
    {
        if (_highlights == null || _highlights.Length == 0 || _repeatCount < 1 ||
            !(_fadeDuration > 0f) || float.IsInfinity(_fadeDuration) ||
            !(_maximumAlpha >= 0f && _maximumAlpha <= 1f))
            throw new InvalidOperationException($"{name}: Valid highlight images and animation settings are required.");
        for (int index = 0; index < _highlights.Length; index++)
        {
            if (_highlights[index] == null)
                throw new InvalidOperationException($"{name}: A highlight Image is missing.");
            for (int previous = 0; previous < index; previous++)
                if (_highlights[previous] == _highlights[index])
                    throw new InvalidOperationException($"{name}: Duplicate highlight Images are not allowed.");
            SetAlpha(index, 0f);
        }
        _elapsed = new float[_highlights.Length];
        _playing = new bool[_highlights.Length];
    }

    public void PlayHighlight(int location)
    {
        _elapsed[location] = 0f;
        _playing[location] = true;
        SetAlpha(location, 0f);
    }

    private void Update()
    {
        float cycleDuration = _fadeDuration * 2f;
        for (int index = 0; index < _highlights.Length; index++)
        {
            if (!_playing[index]) continue;
            _elapsed[index] += Time.deltaTime;
            if (_elapsed[index] >= cycleDuration * _repeatCount)
            {
                _playing[index] = false;
                SetAlpha(index, 0f);
                continue;
            }
            float phase = (_elapsed[index] % cycleDuration) / _fadeDuration;
            SetAlpha(index, (phase <= 1f ? phase : 2f - phase) * _maximumAlpha);
        }
    }

    private void OnDisable()
    {
        if (_playing == null) return;
        for (int index = 0; index < _playing.Length; index++)
        {
            _playing[index] = false;
            if (_highlights[index] != null) SetAlpha(index, 0f);
        }
    }

    private void SetAlpha(int index, float alpha)
    {
        Color color = _highlights[index].color;
        color.a = alpha;
        _highlights[index].color = color;
    }
}
