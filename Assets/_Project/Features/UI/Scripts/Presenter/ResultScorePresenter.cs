using System;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.InputSystem;
using Unityroom.Client;

[RequireComponent(typeof(ResultScoreView))]
public sealed class ResultScorePresenter : MonoBehaviour
{
    private const string TitleScenePath = "Assets/_Project/Scenes/Title.unity";

    public static int? FinalScore { private get; set; }

    [SerializeField, Min(0f), Tooltip("画面遷移の終了後、入力を無効にする秒数。")]
    private float _inputBlockDuration = 1f;

    [Header("unityroom Ranking")]
    [SerializeField, Min(1)] private int _unityroomScoreboardId = 1;
    [SerializeField] private string _unityroomHmacKey;

    private ResultScoreView _view;
    private SceneBgmPresenter _bgm;
    private InputAction _submit;
    private bool _returnRequested;
    private float _inputBlockRemaining;
    private bool _bgmStarted;
    private bool _maximumAppleSoundHandled;

    private void Start()
    {
        _view = GetComponent<ResultScoreView>();
        _bgm = FindFirstObjectByType<SceneBgmPresenter>();
        if (_bgm == null)
            throw new System.InvalidOperationException("The result scene requires a SceneBgmPresenter.");
        int? finalScore = FinalScore;
        FinalScore = null;
        _view.Play(finalScore.GetValueOrDefault());
        _submit = InputSystem.actions.FindAction("UI/Submit", throwIfNotFound: true);
        _submit.Enable();
        _inputBlockRemaining = _inputBlockDuration;

        if (Application.platform == RuntimePlatform.WebGLPlayer && finalScore.HasValue &&
            _unityroomScoreboardId > 0 && !string.IsNullOrWhiteSpace(_unityroomHmacKey))
        {
            SubmitScoreAsync(finalScore.Value, _unityroomScoreboardId, _unityroomHmacKey.Trim()).Forget();
        }
    }

    private static async UniTask SubmitScoreAsync(int score, int scoreboardId, string hmacKey)
    {
        try
        {
            // The request can finish after the player returns to the title scene.
            using var client = new UnityroomClient { HmacKey = hmacKey };
            await client.Scoreboards.SendAsync(new()
            {
                ScoreboardId = scoreboardId,
                Score = score
            });
        }
        catch (Exception exception)
        {
            Debug.LogWarning($"unityroom score submission failed ({exception.GetType().Name}).");
        }
    }

    private void Update()
    {
        if (_returnRequested || SceneTransitionPresenter.IsTransitioning)
            return;

        if (!_bgmStarted && _view.IsComplete && !_view.IsResultSoundPlaying)
        {
            if (!_maximumAppleSoundHandled && _view.HasReachedMaximumAppleScore)
            {
                _view.PlayMaximumAppleSound();
                _maximumAppleSoundHandled = true;
            }
            else
            {
                _bgm.PlayResultBgm();
                _bgmStarted = true;
            }
        }

        if (_inputBlockRemaining > 0f)
        {
            _inputBlockRemaining -= Time.unscaledDeltaTime;
            return;
        }

        bool submitPressed = _submit.WasPressedThisFrame();
        if (!_view.IsComplete)
        {
            if (submitPressed)
                _view.Complete();
            return;
        }

        if (!submitPressed && Mouse.current?.leftButton.wasPressedThisFrame != true)
            return;

        _returnRequested = true;
        SceneTransitionPresenter.LoadScene(TitleScenePath);
    }
}
