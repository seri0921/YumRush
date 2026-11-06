using UnityEngine;
using UnityEngine.Assertions;

[DisallowMultipleComponent]
public sealed class UIUpdraftTimingSoundView : MonoBehaviour
{
    [SerializeField] private AudioSource _source;
    [SerializeField] private SoundSettings _success = new SoundSettings();

    private void Awake()
    {
        Assert.IsNotNull(_source);
        _success.Validate();
    }

    public void PlaySuccess() => _source.PlayOneShot(_success.Clip, _success.Volume);

    private void OnDisable() => _source.Stop();
}
