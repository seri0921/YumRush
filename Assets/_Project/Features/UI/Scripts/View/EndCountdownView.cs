using System;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public sealed class EndCountdownView : MonoBehaviour
{
    [SerializeField] private TMPro.TMP_Text _label;
    [SerializeField] private AudioClip _countClip;
    [SerializeField] private AudioClip _timeUpClip;
    private AudioSource _source;

    private void Awake()
    {
        if (_label == null)
            throw new InvalidOperationException("End countdown requires a text label.");
        _source = GetComponent<AudioSource>();
        _label.enabled = false;
    }

    public void ShowCount(int seconds) => Show(seconds.ToString(), _countClip);
    public void ShowTimeUp() => Show("TIME UP!", _timeUpClip);

    private void Show(string text, AudioClip clip)
    {
        _label.text = text;
        _label.enabled = true;
        if (clip != null)
            _source.PlayOneShot(clip);
    }
}
