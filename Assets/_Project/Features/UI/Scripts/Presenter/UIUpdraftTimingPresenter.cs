using System;
using UnityEngine;

public sealed class UIUpdraftTimingPresenter : MonoBehaviour
{
    [Space(8)] 
    [Header("参照")] [Tooltip("気流の状態とジャンプ可否を読み取るプレイヤー。")] [SerializeField] 
    private PlayerPresenter player;
    
    [Tooltip("タイミングゲージを描画するView。")] [SerializeField] 
    private UIUpdraftTimingView view;
    
    [Space(8)]
    [Header("判定")] [Tooltip("最大上昇速度に対する最適タイミングのしきい値（0〜1）。")] [SerializeField, Range(0.01f, 1f)] 
    private float optimalThreshold = 0.8f;

    private UIUpdraftTimingLogic logic;

    private void Awake()
    {
        if (player == null || view == null)
            throw new InvalidOperationException("UpdraftTimingDisplay の参照を設定してください。");
        logic = new UIUpdraftTimingLogic(optimalThreshold);
    }

    private void Update()
    {
        view.Render(logic.Evaluate(player.State, player.CurrentLiftVelocity,
            player.MaximumLiftVelocity, player.CanWindJump));
    }
}
