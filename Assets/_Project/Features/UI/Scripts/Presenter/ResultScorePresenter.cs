using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(ResultScoreView))]
public sealed class ResultScorePresenter : MonoBehaviour
{
    private const string TitleScenePath = "Assets/_Project/Scenes/Title.unity";

    public static int FinalScore { private get; set; }

    [SerializeField, Min(0f), Tooltip("画面遷移の終了後、入力を無効にする秒数。")]
    private float _inputBlockDuration = 1f;

    private ResultScoreView _view;
    private SceneBgmPresenter _bgm;
    private InputAction _submit;
    private bool _returnRequested;
    private float _inputBlockRemaining;
    private bool _bgmStarted;

    private void Start()
    {
        _view = GetComponent<ResultScoreView>();
        _bgm = FindFirstObjectByType<SceneBgmPresenter>();
        if (_bgm == null)
            throw new System.InvalidOperationException("The result scene requires a SceneBgmPresenter.");
        _view.Play(FinalScore);
        FinalScore = 0;
        _submit = InputSystem.actions.FindAction("UI/Submit", throwIfNotFound: true);
        _submit.Enable();
        _inputBlockRemaining = _inputBlockDuration;
    }

    private void Update()
    {
        if (_returnRequested || SceneTransitionPresenter.IsTransitioning)
            return;

        if (!_bgmStarted && _view.IsComplete && !_view.IsGrowthSoundPlaying)
        {
            _bgm.PlayResultBgm();
            _bgmStarted = true;
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
