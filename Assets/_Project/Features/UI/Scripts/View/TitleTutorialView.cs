using System.Collections;
using UnityEngine;
using UnityEngine.Video;

// Owns video playback and its RawImage presentation.
public sealed class TitleTutorialView : MonoBehaviour
{
    [SerializeField] private Canvas _canvas;
    [SerializeField] private UnityEngine.UI.RawImage _image;
    [SerializeField] private UnityEngine.UI.AspectRatioFitter _aspectRatio;
    [SerializeField] private VideoPlayer _player;

    private string _playbackError;

    private void Awake()
    {
        if (_canvas == null || _image == null || _aspectRatio == null || _player == null)
            throw new System.InvalidOperationException("Title tutorial view requires Canvas, RawImage, aspect ratio and VideoPlayer.");
        _player.playOnAwake = false;
        _player.isLooping = true;
        _player.renderMode = VideoRenderMode.APIOnly;
        _player.errorReceived += HandleError;
        Hide();
    }

    public IEnumerator Play(VideoClip clip)
    {
        _canvas.enabled = true;
        _image.enabled = false;
        _player.Stop();
        _playbackError = null;
        _player.clip = clip;
        _player.Prepare();
        float deadline = Time.realtimeSinceStartup + 15f;
        while (!_player.isPrepared && _playbackError == null && Time.realtimeSinceStartup < deadline)
            yield return null;
        if (!_player.isPrepared || _playbackError != null)
        {
            Debug.LogError("Tutorial video could not prepare: " + clip.name + ": " + _playbackError, this);
            yield break;
        }
        _player.Play();
        while ((_player.texture == null || _player.frame < 0) && _playbackError == null && Time.realtimeSinceStartup < deadline)
            yield return null;
        if (_player.texture == null || _playbackError != null)
        {
            Debug.LogError("Tutorial video could not display: " + clip.name + ": " + _playbackError, this);
            yield break;
        }
        _image.texture = _player.texture;
        _aspectRatio.aspectRatio = (float)_player.texture.width / _player.texture.height;
        _image.enabled = true;
    }

    public void Hide()
    {
        _player.Stop();
        _image.texture = null;
        _canvas.enabled = false;
    }

    private void HandleError(VideoPlayer player, string message)
    {
        _playbackError = message;
        Debug.LogError("Tutorial video playback failed: " + message, this);
    }

    private void OnDestroy() => _player.errorReceived -= HandleError;
}
