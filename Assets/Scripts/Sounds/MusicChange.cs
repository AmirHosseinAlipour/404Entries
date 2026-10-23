using System;
using System.Collections;
using UnityEngine;

public class MusicChange : MonoBehaviour
{
    public GameObject Music3D;
    public AudioClip MusicToChange;
    public AudioSource ThemeMusic;
    public MainMusicConfig mainMusicConfig;
    public float fadeDuration = 0.3f;


    private void Start()
    {
        fadeDuration = 0.3f;
        if (ThemeMusic != null && AudioController.Instance != null)
        {
            AudioController.Instance.RegisterMusic(ThemeMusic);
        }
    }

    public void ToggleMusic()
    {
        if (Music3D != null)
        {
            Music3D.SetActive(!Music3D.activeSelf);
        }
    }

    public void PlayThemeMusic()
    {
        if (ThemeMusic == null || MusicToChange == null) return;
        StartCoroutine(FadeToNewMusic(MusicToChange));
    }

    public void PlayMainMusic()
    {
        if (ThemeMusic == null) return;
        ThemeMusic.Stop();
        if (mainMusicConfig != null)
        {
            ThemeMusic.clip = mainMusicConfig.mainMusic;
        }
        ThemeMusic.volume = GetTargetVolume();
        ThemeMusic.Play();
    }

    private float GetTargetVolume()
    {
        if (AudioController.Instance != null)
        {
            return AudioController.Instance.isMusicMuted ? 0f : AudioController.Instance.musicVolume;
        }
        return (ThemeMusic != null) ? Mathf.Clamp01(ThemeMusic.volume) : 1f;
    }

    private IEnumerator FadeToNewMusic(AudioClip newClip)
    {
        if (ThemeMusic == null) yield break;

        float startVolume = ThemeMusic.volume;

        for (float t = 0; t < fadeDuration; t += Time.deltaTime)
        {
            ThemeMusic.volume = Mathf.Lerp(startVolume, 0, t / fadeDuration);
            yield return null;
        }

        ThemeMusic.volume = 0;
        ThemeMusic.clip = newClip;
        ThemeMusic.Play();

        float targetVolume = GetTargetVolume();

        for (float t = 0; t < fadeDuration; t += Time.deltaTime)
        {
            ThemeMusic.volume = Mathf.Lerp(0, targetVolume, t / fadeDuration);
            yield return null;
        }

        ThemeMusic.volume = targetVolume;
    }

    
}