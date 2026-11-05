using UnityEngine;
using System.Collections.Generic;
using System;
public class BonusEnemyView : EnemyAnimationViewBase
{
    [Serializable]
    public struct HpSpriteStep
    {
        [Tooltip("HPËáílÇ∆âÊëú")]
        public int hpThreshold;
        public Sprite sprite;
    }

    [Space(8)]
    [Header("HPçÇÇ¢èáÇ…âÊëú")]
    [SerializeField] private List<HpSpriteStep> hpSpriteSteps = new List<HpSpriteStep>();

    [Space(8)]
    [Header("óhÇÍê›íË")] [SerializeField, Min(0.01f)]
    private float wobbleAmount = 0.25f;

    [Tooltip("óhÇÍÇÃë¨Ç≥")] [SerializeField, Min(1f)]
    private float wobbleSpeed = 30f;

    [Tooltip("éùë±éûä‘")] [SerializeField, Min(0.01f)]
    private float wobbleDuration = 0.3f;

    private float remainingWobbleTime;
    private Vector3 defaultScale = Vector3.one;

    protected override void Awake()
    {
        base.Awake();
        if (transform != null)
            defaultScale = transform.localScale;
    }

    protected override void Update()
    {
        base.Update();

        if (remainingWobbleTime > 0f)
        {
            remainingWobbleTime -= Time.deltaTime;

            float progress = remainingWobbleTime / wobbleDuration;
            float wave = Mathf.Sin((wobbleDuration - remainingWobbleTime) * wobbleSpeed) * progress;

            float scaleX = defaultScale.x * (1f + wave * wobbleAmount);
            float scaleY = defaultScale.y * (1f - wave * wobbleAmount);

            transform.localScale = new Vector3(scaleX, scaleY, defaultScale.z);

            if (remainingWobbleTime <= 0f)
            {
                transform.localScale = defaultScale;
            }
        }
    }

    public void Render(int currentHp)
    {
        ApplyBaseColor();

        if (spriteRenderer == null || hpSpriteSteps == null || hpSpriteSteps.Count == 0)
            return;

        Sprite targetSprite = spriteRenderer.sprite;
        foreach (var step in hpSpriteSteps)
        {
            if (currentHp <= step.hpThreshold)
            {
                targetSprite = step.sprite;
            }
        }

        if (targetSprite != null)
        {
            spriteRenderer.sprite = targetSprite;
        }
    }

    public void PlayWobble()
    {
        remainingWobbleTime = wobbleDuration;
    }
}
