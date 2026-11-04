using UnityEngine;
using System.Collections.Generic;
public class KeyIndicatorManager : MonoBehaviour
{
    [Header("éQè∆")]
    [SerializeField] private Camera _mainCamera;
    [SerializeField] private Transform _playerTransform;
    [SerializeField] private KeyIndicatorUI _indicatorPrefab;
    [SerializeField] private RectTransform _canvasRect;

    [Header("ï\é¶ê›íË")]
    [SerializeField] private float _screenMargin = 0.05f;

    private readonly Dictionary<TreasureKey, KeyIndicatorUI> _indicatorMap = new Dictionary<TreasureKey, KeyIndicatorUI>();

    private void Awake()
    {
        if (_mainCamera == null) _mainCamera = Camera.main;
        if (_canvasRect == null) _canvasRect = GetComponentInParent<Canvas>().GetComponent<RectTransform>();
    }

    private void OnEnable()
    {
        KeyEvents.OnKeySpawned += HandleKeySpawned;
        KeyEvents.OnKeyDestroyed += HandleKeyDestroyed;
    }

    private void OnDisable()
    {
        KeyEvents.OnKeySpawned -= HandleKeySpawned;
        KeyEvents.OnKeyDestroyed -= HandleKeyDestroyed;
    }

    private void HandleKeySpawned(TreasureKey key)
    {
        if (key == null || _indicatorMap.ContainsKey(key)) return;
        KeyIndicatorUI indicator = Instantiate(_indicatorPrefab, transform);
        _indicatorMap.Add(key, indicator);
    }

    private void HandleKeyDestroyed(TreasureKey key)
    {
        if (key == null) return;
        if (_indicatorMap.TryGetValue(key, out var indicator))
        {
            if (indicator != null) Destroy(indicator.gameObject);
            _indicatorMap.Remove(key);
        }
    }

    private void LateUpdate()
    {
        if (_mainCamera == null || _playerTransform == null || _indicatorMap.Count == 0) return;

        Vector3 playerViewportPos = _mainCamera.WorldToViewportPoint(_playerTransform.position);

        if (playerViewportPos.z < 0) playerViewportPos = -playerViewportPos;

        Vector2 origin = new Vector2(
            Mathf.Clamp(playerViewportPos.x, _screenMargin, 1f - _screenMargin),
            Mathf.Clamp(playerViewportPos.y, _screenMargin, 1f - _screenMargin)
        );

        foreach (var pair in _indicatorMap)
        {
            TreasureKey key = pair.Key;
            KeyIndicatorUI indicator = pair.Value;

            if (key == null) continue;

            Vector3 keyViewportPos = _mainCamera.WorldToViewportPoint(key.transform.position);

            bool isBehindCamera = keyViewportPos.z < 0;
            if (isBehindCamera)
            {
                keyViewportPos.x = 1f - keyViewportPos.x;
                keyViewportPos.y = 1f - keyViewportPos.y;
            }

            float minX = _screenMargin;
            float maxX = 1f - _screenMargin;
            float minY = _screenMargin;
            float maxY = 1f - _screenMargin;

            bool isOffScreen = isBehindCamera ||
                               keyViewportPos.x < minX || keyViewportPos.x > maxX ||
                               keyViewportPos.y < minY || keyViewportPos.y > maxY;

            if (isOffScreen)
            {
                Vector2 targetPos = (Vector2)keyViewportPos;
                Vector2 dir = targetPos - origin;

                if (dir.sqrMagnitude < 0.0001f) dir = Vector2.up;

                Vector2 intersectionViewport = GetRayRectIntersection(origin, dir.normalized, minX, maxX, minY, maxY);

                Vector2 screenPoint = new Vector2(
                    intersectionViewport.x * Screen.width,
                    intersectionViewport.y * Screen.height
                );

                if (RectTransformUtility.ScreenPointToLocalPointInRectangle(_canvasRect, screenPoint, _mainCamera, out Vector2 localPoint))
                {
                    Vector2 screenDir = new Vector2(dir.x * Screen.width, dir.y * Screen.height);
                    indicator.UpdatePositionAndRotation(localPoint, screenDir, true);
                }
            }
            else
            {
                indicator.UpdatePositionAndRotation(Vector2.zero, Vector2.zero, false);
            }
        }
    }

    private Vector2 GetRayRectIntersection(Vector2 origin, Vector2 dir, float minX, float maxX, float minY, float maxY)
    {
        float tMin = float.MaxValue;

        if (dir.x > 0f)
        {
            float t = (maxX - origin.x) / dir.x;
            if (t >= 0f && t < tMin) tMin = t;
        }
        else if (dir.x < 0f)
        {
            float t = (minX - origin.x) / dir.x;
            if (t >= 0f && t < tMin) tMin = t;
        }

        if (dir.y > 0f)
        {
            float t = (maxY - origin.y) / dir.y;
            if (t >= 0f && t < tMin) tMin = t;
        }
        else if (dir.y < 0f)
        {
            float t = (minY - origin.y) / dir.y;
            if (t >= 0f && t < tMin) tMin = t;
        }

        return origin + dir * tMin;
    }
}
