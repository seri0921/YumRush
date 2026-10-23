using UnityEngine;

[CreateAssetMenu(menuName = "Respawn/Individual Config")]
public sealed class RespawnConfig : ScriptableObject
{
    [SerializeField] private bool automaticRespawn = true;
    [SerializeField, Min(0f)] private float delaySeconds = 10f;
    public bool AutomaticRespawn => automaticRespawn;
    public float DelaySeconds => delaySeconds;
}
