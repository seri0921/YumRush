using System;
using UnityEngine;

public sealed class UIUpdraftTimingPresenter : MonoBehaviour
{
    [Space(8)] 
    [Header("参照")] [Tooltip("気流の状態とジャンプ可否を読み取るプレイヤー。")] [SerializeField] 
    private PlayerPresenter player;
    
    [Tooltip("タイミングゲージを描画するView。")] [SerializeField] 
    private UIUpdraftTimingView view;

    [SerializeField] private UIUpdraftTimingSoundView _soundView;
    
    [Space(8)]
    [Header("判定")] [Tooltip("最大上昇速度に対する最適タイミングのしきい値（0〜1）。")] [SerializeField, Range(0.01f, 1f)] 
    private float optimalThreshold = 0.8f;

    private UIUpdraftTimingLogic logic;
    private Collider2D _playerCollider;
    private Camera _worldCamera;
    private Vector3 _circleCenter;
    private float _circleDiameter;
    private PlayerState _previousState;
    private bool _wasVisible;

    private void Awake()
    {
        if (player == null || view == null || _soundView == null)
            throw new InvalidOperationException("UpdraftTimingDisplay の参照を設定してください。");
        logic = new UIUpdraftTimingLogic(optimalThreshold);
        _playerCollider = player.GetComponent<Collider2D>();
        _worldCamera = Camera.main;
        if (_playerCollider == null || _worldCamera == null)
            throw new InvalidOperationException("タイミング円にはプレイヤーのCollider2DとMainCameraが必要です。");
    }

    private void OnEnable() => player.StateChanged += HandleStateChanged;

    private void OnDisable() => player.StateChanged -= HandleStateChanged;

    private void HandleStateChanged(PlayerState previousState, PlayerState newState)
    {
        if (previousState != PlayerState.Floating || newState != PlayerState.WindJump)
            return;

        // 成立時の強さを使い、描画フレーム間の成功も取りこぼさない。
        UpdraftTimingPresentation presentation = logic.Evaluate(previousState, player.CurrentLiftVelocity,
            player.MaximumLiftVelocity, true);
        if (presentation.Optimal)
        {
            view.PlaySuccess(_playerCollider.bounds.center);
            _soundView.PlaySuccess();
        }
    }

    private void LateUpdate()
    {
        UpdraftTimingPresentation presentation = logic.Evaluate(player.State, player.CurrentLiftVelocity,
            player.MaximumLiftVelocity, player.CanWindJump);
        if (presentation.Visible && (!_wasVisible ||
            player.State == PlayerState.Floating && _previousState != PlayerState.Floating))
        {
            // 浮遊開始位置を保存し、プレイヤーの上下動には追従しない。
            Bounds bounds = _playerCollider.bounds;
            _circleCenter = bounds.center;
            _circleDiameter = Mathf.Max(bounds.size.x, bounds.size.y);
        }
        Vector3 screenCenter = _worldCamera.WorldToScreenPoint(_circleCenter);
        Vector3 screenEdge = _worldCamera.WorldToScreenPoint(_circleCenter + Vector3.up * _circleDiameter);
        view.Render(presentation, screenCenter, Vector2.Distance(screenCenter, screenEdge));
        _wasVisible = presentation.Visible;
        _previousState = player.State;
    }
}
