using UnityEngine;

public sealed class SceneBgmPresenter : MonoBehaviour
{
    [SerializeField] private AudioSource _source;
    [SerializeField] private AudioClip _titleClip;
    [SerializeField, Tooltip("Leave empty until a Game BGM is available.")] private AudioClip _gameClip;
    [SerializeField] private AudioClip _resultClip;

    private float _normalVolume;

    public void Initialize(string sceneName)
    {
        if (_source == null || _titleClip == null || _resultClip == null)
            throw new System.InvalidOperationException("Scene BGM requires an AudioSource and clips for Title and Result.");

        _normalVolume = _source.volume;
        SelectTrack(sceneName);
    }

    public void SelectTrack(string sceneName)
    {
        AudioClip clip = sceneName switch
        {
            "Title" => _titleClip,
            "Game" => _gameClip,
            "Result" => _resultClip,
            _ => null
        };

        if (_source.clip == clip)
            return;

        _source.Stop();
        _source.clip = clip;
        if (clip != null)
            _source.Play();
    }

    public void SetAudibleFraction(float fraction) => _source.volume = _normalVolume * fraction;

    public void FadeOut(float progress) => SetAudibleFraction(1f - progress);
}
