using System.Collections;
using UnityEngine;

public class MusicChange : MonoBehaviour
{
    public GameObject Music3D;
    public AudioClip MusicToChange;
    public AudioSource ThemeMusic;
    private AudioClip Main_Music;
    public float fadeDuration = 1.5f; 

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
        StartCoroutine(FadeToNewMusic(MusicToChange));
    }

    public void PlayMainMusic()
    {
        StartCoroutine(FadeToNewMusic(Main_Music));
    }

    private IEnumerator FadeToNewMusic(AudioClip newClip)
    {
        float startVolume = ThemeMusic.volume;

        for (float t = 0; t < fadeDuration; t += Time.deltaTime)
        {
            ThemeMusic.volume = Mathf.Lerp(startVolume, 0, t / fadeDuration);
            yield return null;
        }

        ThemeMusic.volume = 0;
        ThemeMusic.clip = newClip;
        ThemeMusic.Play();

        for (float t = 0; t < fadeDuration; t += Time.deltaTime)
        {
            ThemeMusic.volume = Mathf.Lerp(0, startVolume, t / fadeDuration);
            yield return null;
        }

        ThemeMusic.volume = startVolume;
    }
}