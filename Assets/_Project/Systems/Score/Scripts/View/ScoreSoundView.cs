using UnityEngine;
using UnityEngine.Assertions;

[DisallowMultipleComponent]
public sealed class ScoreSoundView : MonoBehaviour
{
    [SerializeField] private AudioSource _source;
    [SerializeField] private SoundSettings _gain = new SoundSettings();
    [SerializeField] private SoundSettings _treasureGain = new SoundSettings();

    private void Awake()
    {
        Assert.IsNotNull(_source);
        _gain.Validate();
        _treasureGain.Validate();
    }

    public void PlayGain() => _source.PlayOneShot(_gain.Clip, _gain.Volume);
    public void PlayTreasureGain() => _source.PlayOneShot(_treasureGain.Clip, _treasureGain.Volume);
    private void OnDisable() => _source.Stop();
}
