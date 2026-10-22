using System;
using UnityEngine;
using UnityEngine.Assertions;

// 攻撃準備の表示だけを管理する。状態遷移と攻撃判定は持たない。
public sealed class PlayerHomingAttackView : MonoBehaviour
{
    [SerializeField] private Animator attackAnimator;
    [SerializeField] private SpriteRenderer attackRenderer;
    [SerializeField, Min(0.01f)] private float referenceLength = 4.9f;
    [SerializeField, Min(0.01f)] private float thickness = 1f;
    private static readonly int Attack = Animator.StringToHash("Base Layer.Attack");
    private bool playing;

    public bool IsComplete => playing &&
        attackAnimator.GetCurrentAnimatorStateInfo(0).fullPathHash == Attack &&
        attackAnimator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1f;

    private void Awake()
    {
        Assert.IsNotNull(attackAnimator);
        Assert.IsNotNull(attackRenderer);
        Assert.AreEqual(attackAnimator.gameObject, attackRenderer.gameObject);
        Assert.IsTrue(attackAnimator.gameObject != gameObject);
        Hide();
    }

    public void Render(bool preparing, Vector2 targetPosition)
    {
        if (!preparing)
        {
            Hide();
            return;
        }

        Transform image = attackAnimator.transform;
        Vector2 direction = targetPosition - (Vector2)image.position;
        image.rotation = Quaternion.Euler(0f, 0f, Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg);
        Vector3 parentScale = image.parent.lossyScale;
        if (parentScale.x <= 0f || parentScale.y <= 0f || !Mathf.Approximately(parentScale.x, parentScale.y))
            throw new InvalidOperationException("AttackImage requires a positive uniform parent scale.");
        image.localScale = new Vector3(Mathf.Max(direction.magnitude, 0.001f) / referenceLength / parentScale.x,
            thickness / parentScale.y, 1f);
        attackRenderer.flipX = false;
        if (playing)
            return;

        attackAnimator.gameObject.SetActive(true);
        if (!attackAnimator.HasState(0, Attack))
            throw new InvalidOperationException("Attack Animator requires Base Layer.Attack.");
        attackAnimator.Play(Attack, 0, 0f);
        attackAnimator.Update(0f);
        playing = true;
    }

    public void Hide()
    {
        playing = false;
        attackAnimator.gameObject.SetActive(false);
    }

    private void OnDisable() => Hide();
}
