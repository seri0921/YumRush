using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Video;

// Coordinates the title tutorial sequence and its advance input.
public sealed class TitleTutorialPresenter : MonoBehaviour
{
    [SerializeField] private TitleTutorialView _view;
    [SerializeField] private VideoClip[] _clips;
    [SerializeField, Min(0f)] private float _inputDelaySeconds = 0.3f;

    public IEnumerator Show()
    {
        foreach (VideoClip clip in _clips)
        {
            yield return _view.Play(clip);
            float acceptsInputAt = Time.unscaledTime + _inputDelaySeconds;
            // Always leave the playback-start frame before accepting fresh input.
            do
            {
                yield return null;
            }
            while (Time.unscaledTime < acceptsInputAt || !WasAdvancePressed());
        }
        _view.Hide();
    }

    private static bool WasAdvancePressed()
    {
        Keyboard keyboard = Keyboard.current;
        if (keyboard != null && (keyboard.enterKey.wasPressedThisFrame || keyboard.spaceKey.wasPressedThisFrame))
            return true;
        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
            return true;
        foreach (Gamepad gamepad in Gamepad.all)
            if (gamepad.buttonSouth.wasPressedThisFrame)
                return true;
        return false;
    }

    private void Awake()
    {
        if (_view == null || _clips == null || _clips.Length == 0)
            throw new System.InvalidOperationException("Title tutorial requires a view and video clips.");
        foreach (VideoClip clip in _clips)
            if (clip == null)
                throw new System.InvalidOperationException("Title tutorial contains an unassigned clip.");
    }
}
