using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(AudioSource))]
public class SoundPlayer : MonoBehaviour
{
    [System.Serializable]
    public class NamedClip
    {
        public string name;
        public AudioClip clip;
    }

    public List<NamedClip> clips;
    private Dictionary<string, AudioClip> clipMap;

    private AudioSource audioSource;

    void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        clipMap = new Dictionary<string, AudioClip>();
        foreach (var c in clips)
            clipMap[c.name] = c.clip;

        AudioController.Instance?.RegisterSFXPlayer(this);
    }

    public void SetVolume(float volume)
    {
        if (audioSource != null)
            audioSource.volume = volume;
    }

    public void Play(string clipName)
    {
        if (clipMap == null)
        {
            clipMap = new Dictionary<string, AudioClip>();
            if (clips != null)
            {
                foreach (var c in clips)
                {
                    if (c != null && !string.IsNullOrEmpty(c.name))
                        clipMap[c.name] = c.clip;
                }
            }
        }

        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
            if (audioSource == null)
            {
                audioSource = gameObject.AddComponent<AudioSource>();
            }
        }

        if (clipMap.TryGetValue(clipName, out var clip) && clip != null)
        {
            float vol = (audioSource.volume > 0.01f) ? audioSource.volume : 1f;
            audioSource.PlayOneShot(clip, vol);
        }
    }
    
    public AudioClip GetClipByName(string name)
    {
        if (clipMap.TryGetValue(name, out AudioClip clip))
        {
            return clip;
        }
        return null;
    }
}