using System;
using System.Collections;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public sealed class StartCountdownView : MonoBehaviour
{
    [SerializeField] private TMPro.TMP_Text _label;
    [SerializeField] private AudioClip _countdownClip;
    private AudioSource _source;

    private void Awake()
    {
        if (_label == null)
            throw new InvalidOperationException("Start countdown requires a text label.");
        _source = GetComponent<AudioSource>();
        Hide();
    }

    public void Show(string text)
    {
        if (_label.text != text)
            _label.text = text;
        _label.enabled = true;
    }

    public void Hide() => _label.enabled = false;

    public IEnumerator PrepareSound()
    {
        if (_countdownClip == null)
            yield break;

        if (_countdownClip.loadState == AudioDataLoadState.Unloaded)
            _countdownClip.LoadAudioData();

        while (_countdownClip.loadState == AudioDataLoadState.Loading)
            yield return null;

        if (_countdownClip.loadState != AudioDataLoadState.Loaded)
            throw new InvalidOperationException("Failed to load start countdown audio.");
    }

    public void ScheduleSound(double startedAt)
    {
        if (_countdownClip == null)
            return;

        _source.clip = _countdownClip;
        _source.PlayScheduled(startedAt);
    }
}
