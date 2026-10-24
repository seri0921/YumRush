using UnityEngine;
using UnityEngine.Assertions;

// 対象の選択・攻撃可否の判断は持たず、レティクルの表示だけを担当する。
public sealed class HomingReticleView : MonoBehaviour
{
    [SerializeField] private SpriteRenderer reticleRenderer;

    private void Awake()
    {
        Assert.IsNotNull(reticleRenderer);
        Assert.IsNotNull(reticleRenderer.sprite);
        Hide();
    }

    public void Render(bool visible, Vector2 targetPosition)
    {
        if (!visible)
        {
            Hide();
            return;
        }

        Transform image = reticleRenderer.transform;
        image.position = new Vector3(targetPosition.x, targetPosition.y, image.position.z);
        reticleRenderer.enabled = true;
    }

    public void Hide() => reticleRenderer.enabled = false;

    private void OnDisable() => Hide();
}
