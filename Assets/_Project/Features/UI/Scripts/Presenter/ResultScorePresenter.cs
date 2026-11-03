using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(ResultScoreView))]
public sealed class ResultScorePresenter : MonoBehaviour
{
    private const string TitleScenePath = "Assets/_Project/Scenes/Title.unity";

    public static int FinalScore { private get; set; }

    private ResultScoreView _view;
    private InputAction _submit;
    private bool _returnRequested;

    private void Start()
    {
        _view = GetComponent<ResultScoreView>();
        _view.Play(FinalScore);
        FinalScore = 0;
        _submit = InputSystem.actions.FindAction("UI/Submit", throwIfNotFound: true);
        _submit.Enable();
    }

    private void Update()
    {
        if (_returnRequested)
            return;

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
        SceneManager.LoadScene(TitleScenePath);
    }
}
