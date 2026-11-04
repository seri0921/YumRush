using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

[DefaultExecutionOrder(-1000)]
[RequireComponent(typeof(SceneTransitionView))]
public sealed class SceneTransitionPresenter : MonoBehaviour
{
    [SerializeField] private SceneBgmPresenter _bgm;

    private static SceneTransitionPresenter _instance;
    private SceneTransitionView _view;
    private EventSystem _blockedEventSystem;
    private bool _eventSystemWasEnabled;

    public static bool IsTransitioning { get; private set; }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetState()
    {
        _instance = null;
        IsTransitioning = false;
    }

    private void Awake()
    {
        if (_instance != null)
        {
            gameObject.SetActive(false);
            Destroy(gameObject);
            return;
        }
        _instance = this;
        _view = GetComponent<SceneTransitionView>();
        _view.Hide();
        _bgm.Initialize(gameObject.scene.name);
        DontDestroyOnLoad(gameObject);
        SceneManager.sceneLoaded += HandleSceneLoaded;
    }

    public static bool LoadScene(string scenePath)
    {
        if (IsTransitioning)
            return false;
        if (_instance == null)
            throw new System.InvalidOperationException("The scene requires the SceneTransition prefab.");
        if (!Application.CanStreamedLevelBeLoaded(scenePath))
            throw new System.ArgumentException("Scene is not available in Build Settings: " + scenePath, nameof(scenePath));

        IsTransitioning = true;
        _instance.StartCoroutine(_instance.Transition(scenePath));
        return true;
    }

    private IEnumerator Transition(string scenePath)
    {
        try
        {
            BlockUI();
            yield return _view.Close(_bgm.FadeOut);
            // Keep the final closed frame rendered before activating the next scene.
            yield return null;
            yield return SceneManager.LoadSceneAsync(scenePath);
            yield return null;
            yield return _view.Open(_bgm.SetAudibleFraction);
        }
        finally
        {
            RestoreUI();
            _view.Hide();
            _bgm.SetAudibleFraction(1f);
            IsTransitioning = false;
        }
    }

    private void HandleSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (IsTransitioning)
            BlockUI();
        if (mode == LoadSceneMode.Single)
            _bgm.SelectTrack(scene.name);
    }

    private void BlockUI()
    {
        RestoreUI();
        _blockedEventSystem = EventSystem.current;
        if (_blockedEventSystem == null)
            return;
        _eventSystemWasEnabled = _blockedEventSystem.enabled;
        _blockedEventSystem.enabled = false;
    }

    private void RestoreUI()
    {
        if (_blockedEventSystem != null)
            _blockedEventSystem.enabled = _eventSystemWasEnabled;
        _blockedEventSystem = null;
    }

    private void OnDestroy()
    {
        if (_instance != this)
            return;
        SceneManager.sceneLoaded -= HandleSceneLoaded;
        RestoreUI();
        IsTransitioning = false;
        _instance = null;
    }
}
