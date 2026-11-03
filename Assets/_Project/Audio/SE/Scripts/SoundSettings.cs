using System;
using UnityEngine;
using UnityEngine.Assertions;

[Serializable]
public sealed class SoundSettings
{
    [SerializeField] private AudioClip _clip;
    [SerializeField, Range(0f, 1f)] private float _volume = 1f;

    public AudioClip Clip => _clip;
    public float Volume => _volume;

    public void Validate() => Assert.IsNotNull(_clip);
}

