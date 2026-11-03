using UnityEngine;

[DisallowMultipleComponent]
public sealed class EnemySoundView : MonoBehaviour
{
    [SerializeField] private SoundSettings _death = new SoundSettings();

    [SerializeField] private SoundSettings _attack = new SoundSettings();
    [SerializeField] private AudioSource _attackSource;

    [SerializeField] private SoundSettings _barrierBreak = new SoundSettings();

    [SerializeField] private SoundSettings _wingsOpen = new SoundSettings();

    private void Awake()
    {
        _death.Validate();
        _attack.Validate();
        UnityEngine.Assertions.Assert.IsNotNull(_attackSource);
    }

    public void PlayAttack() => _attackSource.PlayOneShot(_attack.Clip, _attack.Volume);
    public void PlayBarrierBreak()
    {
        _barrierBreak.Validate();
        _attackSource.PlayOneShot(_barrierBreak.Clip, _barrierBreak.Volume);
    }

    public void PlayWingsOpen()
    {
        _wingsOpen.Validate();
        _attackSource.PlayOneShot(_wingsOpen.Clip, _wingsOpen.Volume);
    }

    private void OnDisable() => _attackSource.Stop();

    public void PlayDeath()
    {
        // The playback object must outlive the enemy that is about to disappear.
        var playback = new GameObject("EnemyDeathSound");
        playback.transform.position = transform.position;
        AudioSource source = playback.AddComponent<AudioSource>();
        source.playOnAwake = false;
        source.spatialBlend = 0f;
        source.clip = _death.Clip;
        source.volume = _death.Volume;
        source.Play();
        Destroy(playback, _death.Clip.length);
    }
}
