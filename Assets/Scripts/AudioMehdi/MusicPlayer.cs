using System.Collections;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class MusicPlayer : MonoBehaviour
{
    public AudioSource musicSource;

    void  Start()
    {
        DontDestroyOnLoad(gameObject);
        AudioController.Instance?.RegisterMusic(musicSource);
        
        if (AudioController.Instance == null)
            Debug.LogError("AudioController.Instance is null in MusicPlayer");
        else
            AudioController.Instance.RegisterMusic(musicSource);
    }
}