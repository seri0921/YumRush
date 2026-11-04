using UnityEngine;

public sealed class TitleMenuPresenter : MonoBehaviour
{
    private const string GameScenePath = "Assets/_Project/Scenes/Game.unity";
    private bool _startRequested;

    public void StartGame()
    {
        if (_startRequested || SceneTransitionPresenter.IsTransitioning)
            return;

        SceneTransitionPresenter.LoadScene(GameScenePath);
        _startRequested = true;
    }

    public void ExitGame()
    {
#if UNITY_EDITOR
        if (!_startRequested && !SceneTransitionPresenter.IsTransitioning)
            UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}
