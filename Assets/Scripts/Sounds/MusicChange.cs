using System;
using UnityEngine;

public class MusicChange : MonoBehaviour
{
    public GameObject Music3D;
    public AudioClip MusicToChange;
    public AudioSource ThemeMusic;
    private AudioClip Main_Music;

    private void Start()
    {
        Main_Music = ThemeMusic.clip;
    }

    public void ToggleMusic()
    {
        Music3D.SetActive(!Music3D.activeSelf);
    }

    public void PlayThemeMusic()
    {
        ThemeMusic.clip = MusicToChange;
    }

    public void PlayMainMusic()
    {
        ThemeMusic.clip = Main_Music;
    }
    
    
    
}
