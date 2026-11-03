using UnityEngine;
using UnityEngine.Assertions;

[DisallowMultipleComponent]
public sealed class ScoreSoundView : MonoBehaviour
{
    [SerializeField] private AudioSource _source;
    [SerializeField] private SoundSettings _gain = new SoundSettings();

    private void Awake()
    {
        Assert.IsNotNull(_source);
        _gain.Validate();
    }

    public void PlayGain() => _source.PlayOneShot(_gain.Clip, _gain.Volume);
    private void OnDisable() => _source.Stop();
}
