using UnityEngine;
using UnityEngine.Assertions;

[DisallowMultipleComponent]
public sealed class StageEventSoundView : MonoBehaviour
{
    [SerializeField] private StageEventPresenter _eventSource;
    [SerializeField] private AudioClip _warningClip;

    [SerializeField] private AudioSource _audioSource;

    private void Awake()
    {
        Assert.IsNotNull(_eventSource);
        Assert.IsNotNull(_warningClip);
        _audioSource.playOnAwake = false;
        _audioSource.spatialBlend = 0f;
    }

    private void OnEnable() => _eventSource.EventStarted += PlayWarning;

    private void OnDisable()
    {
        if (_eventSource != null)
            _eventSource.EventStarted -= PlayWarning;
        _audioSource.Stop();
    }

    private void PlayWarning(BiomeSlot slot) => _audioSource.PlayOneShot(_warningClip);
}
