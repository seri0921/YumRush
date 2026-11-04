using System;
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

    public void PlaySound()
    {
        if (_countdownClip != null)
            _source.PlayOneShot(_countdownClip);
    }
}
