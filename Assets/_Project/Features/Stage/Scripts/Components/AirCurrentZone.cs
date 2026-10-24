using System;
using UnityEngine;

public sealed class AirCurrentZone : MonoBehaviour
{
    [Tooltip("ローカル上方向へ加える加速度（unit/秒²）。")]
    [SerializeField, Min(0.01f)] private float acceleration = 30f;
    [Tooltip("気流方向の最大速度（unit/秒）。進入時に超えている速度は維持します。")]
    [SerializeField, Min(0.01f)] private float maximumSpeed = 20f;
    [Header("中心線への捕捉")]
    [SerializeField, Min(0.01f)] private float captureSpeed = 10f;
    [SerializeField, Min(0.01f)] private float captureAcceleration = 60f;
    [SerializeField] private PolygonCollider2D area;

    public float Acceleration => acceleration;
    public float MaximumSpeed => maximumSpeed;
    public float CaptureSpeed => captureSpeed;
    public float CaptureAcceleration => captureAcceleration;
    public Vector2 Center => area.transform.TransformPoint(area.offset);
    public Vector2 Direction => ((Vector2)transform.up).normalized;
    public bool IsAvailable => isActiveAndEnabled && area.enabled && area.gameObject.activeInHierarchy;

    private void Reset()
    {
        area = GetComponent<PolygonCollider2D>();
        area.isTrigger = true;
    }

    private void Awake()
    {
        if (area == null)
            throw new InvalidOperationException("AirCurrentZone requires an area collider.");
        if (!area.isTrigger || !IsPositiveFinite(acceleration) || !IsPositiveFinite(maximumSpeed) ||
            !IsPositiveFinite(captureSpeed) || !IsPositiveFinite(captureAcceleration) || Direction.sqrMagnitude < 0.99f)
            throw new InvalidOperationException("AirCurrentZone の設定が不正です。Z軸回転で方向を設定してください。");
    }

    private static bool IsPositiveFinite(float value) => value > 0f && !float.IsInfinity(value);
}
