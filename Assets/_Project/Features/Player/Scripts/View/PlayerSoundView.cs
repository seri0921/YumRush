using UnityEngine;
using UnityEngine.Assertions;

[DisallowMultipleComponent]
public sealed class PlayerSoundView : MonoBehaviour
{
    [SerializeField] private AudioSource _walkSource;
    [SerializeField] private AudioSource _actionSource;
    [SerializeField] private AudioSource _glideSource;
    [SerializeField] private SoundSettings _walk = new SoundSettings();
    [SerializeField] private SoundSettings _jump = new SoundSettings();
    [SerializeField] private SoundSettings _glide = new SoundSettings();
    [SerializeField] private SoundSettings _airDash = new SoundSettings();

    [SerializeField] private SoundSettings _attackReach = new SoundSettings();
    [SerializeField] private SoundSettings _attackEat = new SoundSettings();

    [SerializeField] private SoundSettings _damage = new SoundSettings();
    [SerializeField] private SoundSettings _reticle = new SoundSettings();

    private void Awake()
    {
        Assert.IsNotNull(_walkSource);
        Assert.IsNotNull(_actionSource);
        Assert.IsNotNull(_glideSource);
        _damage.Validate();
        _reticle.Validate();
        _walk.Validate();
        _jump.Validate();
        _glide.Validate();
        _airDash.Validate();
        _attackReach.Validate();
        _attackEat.Validate();
        _glideSource.clip = _glide.Clip;
        _glideSource.loop = true;
    }

    public void PlayWalk()
    {
        // Keep repeated steps from stacking when the clip is longer than the interval.
        _walkSource.clip = _walk.Clip;
        _walkSource.volume = _walk.Volume;
        _walkSource.Play();
    }

    public void StopWalking() => _walkSource.Stop();
    public void PlayJump() => _actionSource.PlayOneShot(_jump.Clip, _jump.Volume);
    public void PlayAirDash() => _actionSource.PlayOneShot(_airDash.Clip, _airDash.Volume);

    public void PlayAttackReach() => _actionSource.PlayOneShot(_attackReach.Clip, _attackReach.Volume);
    public void PlayAttackEat() => _actionSource.PlayOneShot(_attackEat.Clip, _attackEat.Volume);

    public void PlayDamage() => _actionSource.PlayOneShot(_damage.Clip, _damage.Volume);
    public void PlayReticle() => _actionSource.PlayOneShot(_reticle.Clip, _reticle.Volume);

    public void SetGliding(bool gliding)
    {
        _glideSource.volume = _glide.Volume;
        if (gliding == _glideSource.isPlaying)
            return;
        if (gliding)
            _glideSource.Play();
        else
            _glideSource.Stop();
    }

    public void StopAll()
    {
        _walkSource.Stop();
        _actionSource.Stop();
        _glideSource.Stop();
    }

    private void OnDisable() => StopAll();
}
