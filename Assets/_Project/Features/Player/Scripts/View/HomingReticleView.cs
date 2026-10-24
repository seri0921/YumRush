using UnityEngine;
using UnityEngine.Assertions;

// 対象の選択・攻撃可否の判断は持たず、レティクルの表示だけを担当する。
public sealed class HomingReticleView : MonoBehaviour
{
    [SerializeField] private SpriteRenderer reticleRenderer;
    [SerializeField, Min(1f)] private float acquireScaleMultiplier = 2.5f;
    [SerializeField, Min(0.01f)] private float acquireDuration = 0.12f;

    private Vector3 normalScale;
    private float acquireElapsed;

    private void Awake()
    {
        Assert.IsNotNull(reticleRenderer);
        Assert.IsNotNull(reticleRenderer.sprite);
        Assert.IsTrue(acquireScaleMultiplier >= 1f);
        Assert.IsTrue(acquireDuration > 0f);
        normalScale = reticleRenderer.transform.localScale;
        Hide();
    }

    public void Render(bool visible, Vector2 targetPosition, bool targetChanged, float deltaTime)
    {
        if (!visible)
        {
            Hide();
            return;
        }

        Transform image = reticleRenderer.transform;
        image.position = new Vector3(targetPosition.x, targetPosition.y, image.position.z);
        if (targetChanged || !reticleRenderer.enabled)
            acquireElapsed = 0f;
        else
            acquireElapsed = Mathf.Min(acquireElapsed + deltaTime, acquireDuration);

        // Ease-out cubic: 素早く縮小し、通常サイズに向かって減速する。
        float remaining = 1f - acquireElapsed / acquireDuration;
        float multiplier = 1f + (acquireScaleMultiplier - 1f) * remaining * remaining * remaining;
        image.localScale = normalScale * multiplier;
        reticleRenderer.enabled = true;
    }

    public void Hide()
    {
        reticleRenderer.enabled = false;
        reticleRenderer.transform.localScale = normalScale;
        acquireElapsed = 0f;
    }

    private void OnDisable() => Hide();
}
