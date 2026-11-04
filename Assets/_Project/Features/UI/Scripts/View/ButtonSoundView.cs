using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public sealed class ButtonSoundView : MonoBehaviour
{
    [SerializeField] private AudioClip _clickClip;
    [SerializeField] private AudioClip _selectionClip;

    private AudioSource _source;

    public bool IsPlaying => _source.isPlaying;

    private void Awake() => _source = GetComponent<AudioSource>();

    public void PlayClick() => Play(_clickClip);
    public void PlaySelection() => Play(_selectionClip);

    private void Play(AudioClip clip)
    {
        if (clip != null)
            _source.PlayOneShot(clip);
    }
}
