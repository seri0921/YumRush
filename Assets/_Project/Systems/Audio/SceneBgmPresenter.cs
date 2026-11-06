using UnityEngine;

public sealed class SceneBgmPresenter : MonoBehaviour
{
    [SerializeField] private AudioSource _source;
    [SerializeField] private AudioClip _titleClip;
    [SerializeField, Tooltip("Leave empty until a Game BGM is available.")] private AudioClip _gameClip;
    [SerializeField] private AudioClip _resultClip;
    [SerializeField, Range(1f, 3f)] private float _endTempoMultiplier = 1.2f;

    private float _normalVolume;
    private float _normalPitch;
    private ScorePresenter _scorePresenter;
    private bool _resultPlaybackPending;

    public void Initialize(string sceneName)
    {
        if (_source == null || _titleClip == null || _resultClip == null)
            throw new System.InvalidOperationException("Scene BGM requires an AudioSource and clips for Title and Result.");

        _normalVolume = _source.volume;
        _normalPitch = _source.pitch;
        SelectTrack(sceneName);
    }

    public void SelectTrack(string sceneName)
    {
        _resultPlaybackPending = sceneName == "Result";
        UnsubscribeScore();
        SetEndTempo(false);
        if (sceneName == "Game")
        {
            _scorePresenter = FindFirstObjectByType<ScorePresenter>();
            if (_scorePresenter != null)
            {
                _scorePresenter.VisibilityChanged += SetEndTempo;
                SetEndTempo(_scorePresenter.IsScoreHidden);
            }
        }

        AudioClip clip = sceneName switch
        {
            "Title" => _titleClip,
            "Game" => _gameClip,
            "Result" => _resultClip,
            _ => null
        };

        if (_source.clip == clip && !_resultPlaybackPending)
            return;

        _source.Stop();
        _source.clip = clip;
        if (clip != null && !_resultPlaybackPending)
            _source.Play();
    }

    public void PlayResultBgm()
    {
        if (!_resultPlaybackPending)
            return;

        _resultPlaybackPending = false;
        _source.Play();
    }

    public void SetAudibleFraction(float fraction) => _source.volume = _normalVolume * fraction;

    public void FadeOut(float progress) => SetAudibleFraction(1f - progress);

    private void SetEndTempo(bool isScoreHidden)
    {
        _source.pitch = Mathf.Min(3f, _normalPitch * (isScoreHidden ? _endTempoMultiplier : 1f));
    }

    private void UnsubscribeScore()
    {
        if (_scorePresenter != null)
            _scorePresenter.VisibilityChanged -= SetEndTempo;
        _scorePresenter = null;
    }

    private void OnDestroy() => UnsubscribeScore();
}
