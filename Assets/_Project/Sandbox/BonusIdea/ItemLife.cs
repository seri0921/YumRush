using UnityEngine;
using System;
public class ItemLife : MonoBehaviour
{
    [Header("寿命設定")]
    [Tooltip("生成されてから消滅するまでの時間（秒）")]
    [SerializeField, Min(0.1f)] private float lifetime = 6.0f;

    [Tooltip("消滅前何秒から点滅を開始するか")]
    [SerializeField, Min(0.1f)] private float blinkDuration = 1.5f;

    [Tooltip("点滅の速さ（値が大きいほど高速点滅）")]
    [SerializeField, Min(1f)] private float blinkSpeed = 12f;

    [SerializeField]private SpriteRenderer spriteRenderer;
    private float remainingTime;
    private Color originalColor = Color.white;

    private void Awake()
    {
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            originalColor = spriteRenderer.color;
        }
        remainingTime = lifetime;
    }

    private void Update()
    {
        if (Time.timeScale == 0f) return;

        remainingTime -= Time.deltaTime;

        if (spriteRenderer != null && remainingTime <= blinkDuration)
        {
            float alpha = Mathf.PingPong(Time.time * blinkSpeed, 1.0f);
            Color blinkColor = originalColor;
            blinkColor.a = alpha;
            spriteRenderer.color = blinkColor;
        }

        if (remainingTime <= 0f)
        {
            Expire();
        }
    }

    private void Expire()
    {
        if (TryGetComponent<RespawnTarget>(out var respawnTarget))
        {
            respawnTarget.NotifyConsumed();
        }

        gameObject.SetActive(false);
        Destroy(gameObject);
    }
}
