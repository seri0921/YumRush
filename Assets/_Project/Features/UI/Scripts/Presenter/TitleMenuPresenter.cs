using System.Collections;
using UnityEngine;

public sealed class TitleMenuPresenter : MonoBehaviour
{
    private const string GameScenePath = "Assets/_Project/Scenes/Game.unity";

    [SerializeField] private TitleButtonView _startButton;
    [SerializeField] private TitleButtonView _exitButton;
    [SerializeField] private ButtonSoundView _soundView;
    [SerializeField] private TitleTutorialPresenter _tutorial;

    private TitleButtonView _lastSelectedButton;
    private bool _actionRequested;

    private bool CanAcceptAction => !_actionRequested && !SceneTransitionPresenter.IsTransitioning;

    private void OnEnable()
    {
        _startButton.Selected += HandleSelection;
        _exitButton.Selected += HandleSelection;
    }

    private void OnDisable()
    {
        _startButton.Selected -= HandleSelection;
        _exitButton.Selected -= HandleSelection;
    }

    private void HandleSelection(TitleButtonView button)
    {
        if (_lastSelectedButton != null && _lastSelectedButton != button && CanAcceptAction)
            _soundView.PlaySelection();

        _lastSelectedButton = button;
    }

    public void StartGame()
    {
        if (!CanAcceptAction)
            return;

        _actionRequested = SceneTransitionPresenter.LoadScene(GameScenePath, _tutorial.Show());
        if (_actionRequested)
            _soundView.PlayClick();
    }

    public void ExitGame()
    {
        if (!CanAcceptAction)
            return;

        _actionRequested = true;
        _soundView.PlayClick();
        StartCoroutine(ExitAfterSound());
    }

    private IEnumerator ExitAfterSound()
    {
        yield return null;
        while (_soundView.IsPlaying)
            yield return null;

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
