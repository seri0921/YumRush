using UnityEngine;
using UnityEngine.Assertions;

// 移動状態に対応する風の粒子表現を再生する。
public sealed class PlayerWindEffectView : MonoBehaviour
{
    [SerializeField] private ParticleSystem _glideParticles;
    [SerializeField] private ParticleSystem _airDashParticles;

    private void Awake()
    {
        Assert.IsNotNull(_glideParticles);
        Assert.IsNotNull(_airDashParticles);
    }

    public void Render(PlayerState state, int facingDirection)
    {
        Quaternion rotation = Quaternion.Euler(0f, 0f, facingDirection > 0 ? 0f : 180f);
        _glideParticles.transform.localRotation = rotation;
        _airDashParticles.transform.localRotation = rotation;
        SetEmitting(_glideParticles, state == PlayerState.Gliding);
        SetEmitting(_airDashParticles, state == PlayerState.AirDashing);
    }

    public void Hide()
    {
        _glideParticles.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        _airDashParticles.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
    }

    private void OnDisable() => Hide();

    private static void SetEmitting(ParticleSystem particles, bool emitting)
    {
        if (emitting == particles.isEmitting)
            return;

        if (emitting)
            particles.Play(true);
        else
            // 状態終了時も、すでに発生した粒子は寿命まで残す。
            particles.Stop(true, ParticleSystemStopBehavior.StopEmitting);
    }
}
