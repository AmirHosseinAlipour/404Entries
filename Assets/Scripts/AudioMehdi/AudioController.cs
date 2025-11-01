using UnityEngine;
using System.Collections.Generic;

public class AudioController : MonoBehaviour
{
    public static AudioController Instance { get; private set; }

    [Range(0f, 1f)]
    public float sfxVolume = 1f;
    [Range(0f, 1f)]
    public float musicVolume = 1f;
    
    public bool isMusicMuted = false;
    public bool isSFXMuted = false;
    private float lastSFXVolume;
    private float lastMusicVolume;

    private List<SoundPlayer> allSFXPlayers = new();
    private List<AudioSource> musicSources = new();

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void RegisterSFXPlayer(SoundPlayer player)
    {
        if (!allSFXPlayers.Contains(player))
        {
            allSFXPlayers.Add(player);
        }
        player.SetVolume(sfxVolume);
    }

    public void RegisterMusic(AudioSource musicSource)
    {
        if (!musicSources.Contains(musicSource))
        {
            musicSources.Add(musicSource);
        }
        musicSource.volume = musicVolume;
    }

    public void SetSFXVolume(float volume)
    {
        sfxVolume = Mathf.Clamp01(volume);
        UpdateSFXVolume();
    }

    public void SetMusicVolume(float volume)
    {
        musicVolume = Mathf.Clamp01(volume);
        UpdateMusicVolume();
    }

    
    public void ToggleMusicMute(bool mute)
    {
        isMusicMuted = mute;
        UpdateMusicVolume();
    }

    public void ToggleSFXMute(bool mute)
    {
        isSFXMuted = mute;
        UpdateSFXVolume();
    }
    
    private void UpdateMusicVolume()
    {
        float volume = isMusicMuted ? 0f : musicVolume;
        foreach (var music in musicSources)
        {
            music.volume = volume;
        }
    }

    private void UpdateSFXVolume()
    {
        float volume = isSFXMuted ? 0f : sfxVolume;
        foreach (var player in allSFXPlayers)
        {
            player.SetVolume(volume);
        }
    }
}