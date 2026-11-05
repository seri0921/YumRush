using System;
using UnityEngine;

/// <summary>
/// 物理処理を担当するクラス
/// </summary>
public class PlayerPhysicsMotor
{
    private readonly Rigidbody2D body;
    private readonly Collider2D playerCollider;
    private readonly RaycastHit2D[] groundHits = new RaycastHit2D[8];
    private readonly ContactFilter2D groundFilter;
    private readonly float groundProbeDistance;
    private RigidbodyType2D originalBodyType;
    private Vector2 originalVelocity;
    private float originalGravityScale;
    private bool _isPassingThroughTerrain;
    private bool _originalIsTrigger;
    public bool IsScriptedMovement { get; private set; }
    
    public float NormalGravityScale { get; }
    public Vector2 Velocity => body.linearVelocity;
    public Vector2 Position => body.position;
    
    public PlayerPhysicsMotor(Rigidbody2D body, Collider2D playerCollider,
        LayerMask groundLayers, float groundProbeDistance)
    {
        this.body = body;
        this.playerCollider = playerCollider;
        this.groundProbeDistance = groundProbeDistance;
        NormalGravityScale = body.gravityScale;
        var filter = new ContactFilter2D { useTriggers = false };
        filter.SetLayerMask(groundLayers);
        groundFilter = filter;
    }
    
    public void ApplyVelocity(Vector2 velocity)
    {
        if (!IsScriptedMovement)
            body.linearVelocity = velocity;
    }

    public void SetTerrainPassThrough(bool enabled)
    {
        if (_isPassingThroughTerrain == enabled)
            return;

        if (enabled)
        {
            _originalIsTrigger = playerCollider.isTrigger;
            playerCollider.isTrigger = true;
        }
        else if (playerCollider != null)
        {
            playerCollider.isTrigger = _originalIsTrigger;
        }

        _isPassingThroughTerrain = enabled;
    }

    public void BeginScriptedMovement()
    {
        if (IsScriptedMovement)
            throw new InvalidOperationException("Scripted movement is already active.");

        originalBodyType = body.bodyType;
        originalVelocity = body.linearVelocity;
        originalGravityScale = body.gravityScale;
        body.linearVelocity = Vector2.zero;
        body.bodyType = RigidbodyType2D.Kinematic;
        IsScriptedMovement = true;
    }

    public void SetScriptedPosition(Vector2 position)
    {
        if (!IsScriptedMovement)
            throw new InvalidOperationException("Scripted movement is not active.");

        body.position = position;
    }

    public void EndScriptedMovement(bool restoreMotion = false)
    {
        if (!IsScriptedMovement)
            throw new InvalidOperationException("Scripted movement is not active.");

        // The player may have been destroyed before its movement handle is released.
        if (body != null)
        {
            body.bodyType = originalBodyType;
            body.linearVelocity = restoreMotion ? originalVelocity : Vector2.zero;
            body.gravityScale = restoreMotion ? originalGravityScale : NormalGravityScale;
        }
        IsScriptedMovement = false;
    }
    public void SetGravityScale(float scale) => body.gravityScale = scale;
    public void RestoreGravity() => SetGravityScale(NormalGravityScale);
    
    public bool IsGrounded(out bool isIce)
    {
        isIce = false;
        bool grounded = false;
        int count = playerCollider.Cast(Vector2.down, groundFilter, groundHits, groundProbeDistance);
        for (int i = 0; i < count; i++)
            if (groundHits[i].normal.y > 0.65f)
            {
                grounded = true;
                var col = groundHits[i].collider;
                if (col != null && col.CompareTag("Ice"))
                {
                    isIce = true;
                }
            }
        return grounded;
    }
}
