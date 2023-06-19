using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using System;
using Unity.Netcode;

public class AudioManager : NetworkBehaviour
{
    public Sound[] sounds;
    // Start is called before the first frame update
    private void Awake()
    {
        foreach (Sound sound in sounds)
        {
            sound.source = gameObject.AddComponent<AudioSource>();
            sound.source.clip = sound.clip;

            sound.source.volume = sound.volume;
            sound.source.pitch = sound.pitch;
            sound.source.outputAudioMixerGroup = sound.outputAudioMixerGroup;
            sound.source.playOnAwake = false;
        }
    }
    public void Play(string name)
    {
        if (IsOwner)
        {
            Sound sound = Array.Find(sounds, sound => sound.name == name);
            sound.source.Play();
        }
    }

    public void Stop()
    {
        AudioSource[] audioSources = GameObject.FindObjectsOfType(typeof(AudioSource)) as AudioSource[];
        foreach (AudioSource audioSource in audioSources)
        {
            audioSource.Stop();
        }
    }

    public float GetAudioLength(string name)
    {
        Sound sound = Array.Find(sounds, sound => sound.name == name);
        return sound.clip.length;
    }
}
