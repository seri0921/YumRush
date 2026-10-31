using Unity.Cinemachine;
using UnityEngine;

// Generates Cinemachine impulses; the main camera listener applies the displacement.
[DisallowMultipleComponent]
[RequireComponent(typeof(CinemachineExternalImpulseListener))]
public sealed class CameraShakeView : MonoBehaviour
{
    private bool _previousIgnoreTimeScale;

    private void OnEnable()
    {
        _previousIgnoreTimeScale = CinemachineImpulseManager.Instance.IgnoreTimeScale;
        CinemachineImpulseManager.Instance.IgnoreTimeScale = true;
    }

    // Cinemachine recreates its impulse manager after the scene's first OnEnable callbacks.
    private void Start() => CinemachineImpulseManager.Instance.IgnoreTimeScale = true;

    private void OnDisable()
    {
        CinemachineImpulseManager.Instance.IgnoreTimeScale = _previousIgnoreTimeScale;
    }

    public void Play(float strength, float duration)
    {
        if (!isActiveAndEnabled || strength <= 0f || duration <= 0f)
            return;

        // Each active signal keeps its own definition, so a new shake cannot change its duration.
        var impulse = new CinemachineImpulseDefinition
        {
            ImpulseChannel = 1,
            ImpulseType = CinemachineImpulseDefinition.ImpulseTypes.Uniform,
            ImpulseShape = CinemachineImpulseDefinition.ImpulseShapes.Bump,
            ImpulseDuration = duration
        };
        impulse.CreateEvent(transform.position, new Vector3(0.7f, 1f, 0f).normalized * strength);
    }
}
