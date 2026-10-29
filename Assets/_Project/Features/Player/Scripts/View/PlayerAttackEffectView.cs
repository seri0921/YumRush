using UnityEngine;
using UnityEngine.Assertions;

// 攻撃の出来事に対応するエフェクトを、指定されたワールド座標で再生する。
public sealed class PlayerAttackEffectView : MonoBehaviour
{
    [SerializeField] private ParticleSystem _windupCompletePrefab;

    private void Awake() => Assert.IsNotNull(_windupCompletePrefab);

    public void PlayWindupComplete(Vector2 position)
    {
        // 敵の消滅やプレイヤーの移動から独立して、最後まで再生する。
        ParticleSystem effect = Instantiate(_windupCompletePrefab, position, Quaternion.identity);
        effect.Play();
    }
}
