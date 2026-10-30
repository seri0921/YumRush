using System;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
public sealed class EnemyBullet : MonoBehaviour
{
    [Space(8)]
    [Header("寿命")]
    [Tooltip("発射後、弾が消滅するまでの時間（秒）。")]
    [SerializeField, Min(0.01f)] private float lifetime = 5f;
    [Header("回転オフセット")]
    [SerializeField] private float offsetAngle = 0f;
    [Header("ランダムビジュアル")]
    [SerializeField] private List<Sprite> candidateSprites = new List<Sprite>();
    [Space(8)]
    [Header("弾の属性")]
    [Tooltip("氷弾orNot")]
    [SerializeField] private bool isIceBullet = false;

    private Rigidbody2D body;
    private Vector2 velocity;
    private float remainingLifetime;
    private bool launched;

    private void Awake()
    {
        if (lifetime <= 0f)
            throw new InvalidOperationException("UpdraftEnemyProjectile: Lifetimeは正の値にしてください。");
        body = GetComponent<Rigidbody2D>();
        if (body.bodyType != RigidbodyType2D.Kinematic || !GetComponent<Collider2D>().isTrigger)
            throw new InvalidOperationException("UpdraftEnemyProjectile: Kinematic Rigidbody2DとTrigger Collider2Dが必要です。");
        SetupRandomVisual();
        remainingLifetime = lifetime;
    }

    public void Launch(Vector2 direction, float speed)
    {
        if (direction.sqrMagnitude < 0.99f || direction.sqrMagnitude > 1.01f || speed <= 0f)
            throw new ArgumentOutOfRangeException(nameof(direction));
        
        velocity = direction * speed;

        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        angle += offsetAngle;
        transform.rotation = Quaternion.Euler(0f, 0f, angle);

        launched = true;
    }

    private void FixedUpdate()
    {
        if (!launched)
            return;
        body.MovePosition(body.position + velocity * Time.fixedDeltaTime);
        remainingLifetime -= Time.fixedDeltaTime;
        if (remainingLifetime <= 0f)
            Destroy(gameObject);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.TryGetComponent<PlayerPresenter>(out var player))
            return;

        if (isIceBullet)
        {
            player.ReceiveEnemyFreeze();
        }
        else
        {
            player.ReceiveEnemyProjectile();
        }

        Destroy(gameObject);
    }

    private void SetupRandomVisual()
    {
        if (candidateSprites != null && candidateSprites.Count > 0)
        {
            var defaultRenderer = GetComponentInChildren<SpriteRenderer>();
            if (defaultRenderer != null)
            {
                int selectedIndex = UnityEngine.Random.Range(0, candidateSprites.Count);
                defaultRenderer.sprite = candidateSprites[selectedIndex];
            }
            return;
        }
    }

}
