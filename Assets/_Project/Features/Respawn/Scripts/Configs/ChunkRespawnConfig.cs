using UnityEngine;

[CreateAssetMenu(menuName = "Respawn/Chunk Config")]
public sealed class ChunkRespawnConfig : ScriptableObject
{
    [SerializeField] private bool automaticRespawn;
    [SerializeField, Min(0.01f)] private float intervalSeconds = 60f;
    public bool AutomaticRespawn => automaticRespawn;
    public float IntervalSeconds => intervalSeconds;
}
