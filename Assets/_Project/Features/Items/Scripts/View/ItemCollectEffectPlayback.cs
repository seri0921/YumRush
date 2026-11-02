using UnityEngine;

[RequireComponent(typeof(Animator), typeof(SpriteRenderer))]
public sealed class ItemCollectEffectPlayback : MonoBehaviour
{
    // Called by the final animation event so cleanup follows the clip's duration.
    public void CompletePlayback() => Destroy(gameObject);
}
