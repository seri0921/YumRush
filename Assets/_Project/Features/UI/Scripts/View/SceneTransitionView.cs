using System.Collections;
using UnityEngine;

// Owns only the mouth's visual presentation, including its playback timing.
public sealed class SceneTransitionView : MonoBehaviour
{
    [SerializeField] private UnityEngine.UI.Image mouth;
    [SerializeField] private CanvasGroup visibility;
    [SerializeField] private Sprite[] frames;
    [SerializeField, Min(0.01f)] private float closeSeconds = 1f;
    [SerializeField, Min(0.01f)] private float openSeconds = 1f;

    public void Hide() => visibility.alpha = 0f;

    public IEnumerator Close(System.Action<float> reportProgress) => Animate(true, closeSeconds, reportProgress);
    public IEnumerator Open(System.Action<float> reportProgress) => Animate(false, openSeconds, reportProgress);

    private void Awake()
    {
        if (mouth == null || visibility == null || frames == null || frames.Length == 0)
            throw new System.InvalidOperationException("Transition requires an Image, CanvasGroup and mouth frames.");
        foreach (Sprite frame in frames)
            if (frame == null)
                throw new System.InvalidOperationException("Transition contains an unassigned mouth frame.");
    }

    private IEnumerator Animate(bool closing, float duration, System.Action<float> reportProgress)
    {
        visibility.alpha = 1f;
        float elapsed = 0f;
        while (elapsed < duration)
        {
            float progress = Mathf.Clamp01(elapsed / duration);
            // The supplied first frame already contains teeth; fade it at the open endpoint.
            float closedAmount = closing ? progress : 1f - progress;
            int frameIndex = Mathf.Min(Mathf.FloorToInt(closedAmount * frames.Length), frames.Length - 1);
            mouth.sprite = frames[frameIndex];
            mouth.color = new Color(1f, 1f, 1f, Mathf.Min(closedAmount * frames.Length, 1f));
            reportProgress(progress);
            yield return null;
            // Do not count the previous scene-loading frame as animation playback.
            elapsed += Time.unscaledDeltaTime;
        }
        mouth.sprite = frames[closing ? frames.Length - 1 : 0];
        mouth.color = closing ? Color.white : new Color(1f, 1f, 1f, 0f);
        reportProgress(1f);
        if (!closing)
            Hide();
    }
}
