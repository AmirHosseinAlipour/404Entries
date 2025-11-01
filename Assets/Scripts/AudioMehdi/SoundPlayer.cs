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
        if (clipMap.TryGetValue(clipName, out var clip))
            audioSource.PlayOneShot(clip, audioSource.volume);
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