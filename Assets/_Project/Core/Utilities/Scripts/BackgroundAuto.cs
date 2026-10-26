using UnityEngine;
using Unity.Cinemachine;

public class BackgroundAuto : MonoBehaviour
{
    [Header("バイオームコライダー")]
    [SerializeField] private PolygonCollider2D confinerCollider;
    [SerializeField] private Camera mainCam;

    [Header("スムーズ速度")]
    [SerializeField] private float smoothTime = 0.2f; // カメラ追従の滑らかさ

    private SpriteRenderer spriteRenderer;
    private Vector3 currentVelocity;
    private CinemachineBrain _cameraBrain;
    private bool _needsInitialPosition = true;

    public void Configure(PolygonCollider2D cameraBounds, Camera camera)
    {
        confinerCollider = cameraBounds != null ? cameraBounds : throw new System.ArgumentNullException(nameof(cameraBounds));
        mainCam = camera != null ? camera : throw new System.ArgumentNullException(nameof(camera));
        _cameraBrain = mainCam.GetComponent<CinemachineBrain>();
        ResetFollowState();
    }

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void OnEnable()
    {
        if (mainCam == null) mainCam = Camera.main;
        _cameraBrain = mainCam != null ? mainCam.GetComponent<CinemachineBrain>() : null;
        ResetFollowState();
        CinemachineCore.CameraUpdatedEvent.AddListener(OnCameraUpdated);
    }

    private void OnDisable()
    {
        CinemachineCore.CameraUpdatedEvent.RemoveListener(OnCameraUpdated);
    }

    private void ResetFollowState()
    {
        _needsInitialPosition = true;
        currentVelocity = Vector3.zero;
    }

    private void OnCameraUpdated(CinemachineBrain brain)
    {
        if (brain == _cameraBrain && brain.OutputCamera == mainCam)
            UpdatePosition();
    }

    private void LateUpdate()
    {
        // Cinemachine supplies the final camera pose through OnCameraUpdated.
        if (_cameraBrain == null || !_cameraBrain.isActiveAndEnabled)
            UpdatePosition();
    }

    private void UpdatePosition()
    {
        if (mainCam == null || confinerCollider == null || spriteRenderer == null) return;

        Vector3 targetPosition = CalculateTargetPosition();
        if (_needsInitialPosition)
        {
            // Match the first rendered camera pose without easing from the Prefab's saved position.
            transform.position = targetPosition;
            currentVelocity = Vector3.zero;
            _needsInitialPosition = false;
            return;
        }

        transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref currentVelocity, smoothTime);
    }

    private Vector3 CalculateTargetPosition()
    {

        // --------------------------------------------------
        // 1. ポリゴンコライダーの外枠限界 (Bounds) を取得
        // --------------------------------------------------
        Bounds cBounds = confinerCollider.bounds;

        // --------------------------------------------------
        // 2. カメラの画面サイズの半分を取得
        // --------------------------------------------------
        float camHalfHeight = mainCam.orthographicSize;
        float camHalfWidth = camHalfHeight * mainCam.aspect;

        // --------------------------------------------------
        // 3. カメラ中心が実際に移動できる限界座標を計算
        // --------------------------------------------------
        float camMinX = cBounds.min.x + camHalfWidth;
        float camMaxX = cBounds.max.x - camHalfWidth;
        float camMinY = cBounds.min.y + camHalfHeight;
        float camMaxY = cBounds.max.y - camHalfHeight;

        // --------------------------------------------------
        // 4. 現在のカメラ位置から進行度 (0.0 ～ 1.0) を正規化
        // --------------------------------------------------
        // ※画面の中にコライダーが収まり切っている場合は中央(0.5)に固定
        float progressX = (camMinX < camMaxX) ? Mathf.InverseLerp(camMinX, camMaxX, mainCam.transform.position.x) : 0.5f;
        float progressY = (camMinY < camMaxY) ? Mathf.InverseLerp(camMinY, camMaxY, mainCam.transform.position.y) : 0.5f;

        // --------------------------------------------------
        // 5. 背景画像（Scale適用後）の半分のサイズを取得
        // --------------------------------------------------
        float bgHalfWidth = spriteRenderer.bounds.size.x / 2f;
        float bgHalfHeight = spriteRenderer.bounds.size.y / 2f;

        // --------------------------------------------------
        // 6. 進行度 0.0 と 1.0 の時に背景の中心が来るべき目標座標
        // --------------------------------------------------
        float bgTargetXAtMin = cBounds.min.x + bgHalfWidth; // progress = 0 (左端) の時
        float bgTargetXAtMax = cBounds.max.x - bgHalfWidth; // progress = 1 (右端) の時

        float bgTargetYAtMin = cBounds.min.y + bgHalfHeight; // progress = 0 (下端) の時
        float bgTargetYAtMax = cBounds.max.y - bgHalfHeight; // progress = 1 (上端) の時

        // --------------------------------------------------
        // 7. 正規化された進行度に応じて背景位置を補間 (Lerp)
        // --------------------------------------------------
        float targetX = Mathf.Lerp(bgTargetXAtMin, bgTargetXAtMax, progressX);
        float targetY = Mathf.Lerp(bgTargetYAtMin, bgTargetYAtMax, progressY);

        return new Vector3(targetX, targetY, transform.position.z);
    }
}
