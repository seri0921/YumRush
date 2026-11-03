using UnityEngine;
using UnityEngine.SceneManagement;

public sealed class TitleMenuPresenter : MonoBehaviour
{
    private const string GameScenePath = "Assets/_Project/Scenes/Game.unity";
    private bool _startRequested;

    public void StartGame()
    {
        if (_startRequested)
            return;

        SceneManager.LoadScene(GameScenePath);
        _startRequested = true;
    }

    public void ExitGame()
    {
#if UNITY_EDITOR
        if (!_startRequested)
            UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}
