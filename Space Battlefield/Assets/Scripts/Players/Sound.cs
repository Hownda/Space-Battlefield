using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

/// <summary>
/// This class lets the developer customize attributes of audio clips.
/// </summary>
[System.Serializable]
public class Sound
{
    public AudioClip clip;

    public string name;

    [Range(.1f, 3f)]
    public float pitch;

    [Range(0f, 1f)]
    public float volume;

    [HideInInspector] public AudioSource source;
}
