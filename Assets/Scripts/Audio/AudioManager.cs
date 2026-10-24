using System;
using System.Collections;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    private AudioSource _bgmSourceA;
    private AudioSource _bgmSourceB;
    private AudioSource _sfxSource;
    private bool _isUsingSourceA = true;

    [Range(0f, 1f)] public float musicVolume = 1f;
    [Range(0f, 1f)] public float sfxVolume = 1f;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Initialize()
    {
        if (Instance == null)
        {
            GameObject go = new GameObject("AudioManager");
            Instance = go.AddComponent<AudioManager>();
            DontDestroyOnLoad(go);
        }
    }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        SetupAudioSources();
    }

    private void SetupAudioSources()
    {
        _bgmSourceA = gameObject.AddComponent<AudioSource>();
        _bgmSourceB = gameObject.AddComponent<AudioSource>();
        _sfxSource = gameObject.AddComponent<AudioSource>();

        _bgmSourceA.loop = true;
        _bgmSourceB.loop = true;
        _sfxSource.loop = false;

        _bgmSourceA.playOnAwake = false;
        _bgmSourceB.playOnAwake = false;
        _sfxSource.playOnAwake = false;
    }

    public void PlayBGM(AudioClip clip, float fadeDuration = 0.5f, bool loop = true)
    {
        if (clip == null) return;

        AudioSource current = _isUsingSourceA ? _bgmSourceA : _bgmSourceB;
        AudioSource next = _isUsingSourceA ? _bgmSourceB : _bgmSourceA;

        if (current.isPlaying && current.clip == clip) return;

        _isUsingSourceA = !_isUsingSourceA;
        next.clip = clip;
        next.loop = loop;
        next.Play();

        StopAllCoroutines();
        StartCoroutine(CrossFadeRoutine(current, next, fadeDuration));
    }

    private IEnumerator CrossFadeRoutine(AudioSource from, AudioSource to, float duration)
    {
        float timer = 0f;
        float startVol = from.volume;
        float targetVol = GetEffectiveMusicVolume();

        while (timer < duration)
        {
            timer += Time.unscaledDeltaTime;
            float progress = Mathf.Clamp01(timer / duration);
            if (from.isPlaying) from.volume = Mathf.Lerp(startVol, 0f, progress);
            to.volume = Mathf.Lerp(0f, targetVol, progress);
            yield return null;
        }

        if (from.isPlaying)
        {
            from.Stop();
            from.volume = 0f;
        }
        to.volume = targetVol;
    }

    public void StopBGM(float fadeDuration = 0.5f)
    {
        AudioSource current = _isUsingSourceA ? _bgmSourceA : _bgmSourceB;
        if (!current.isPlaying) return;

        StopAllCoroutines();
        StartCoroutine(FadeOutRoutine(current, fadeDuration));
    }

    private IEnumerator FadeOutRoutine(AudioSource source, float duration)
    {
        float timer = 0f;
        float startVol = source.volume;

        while (timer < duration)
        {
            timer += Time.unscaledDeltaTime;
            source.volume = Mathf.Lerp(startVol, 0f, timer / duration);
            yield return null;
        }

        source.Stop();
        source.volume = 0f;
    }

    public void PlaySFX(AudioClip clip, float volumeScale = 1f)
    {
        if (clip == null || _sfxSource == null) return;
        float vol = GetEffectiveSFXVolume() * Mathf.Clamp01(volumeScale);
        _sfxSource.PlayOneShot(clip, vol);
    }

    public void SetMusicVolume(float volume)
    {
        musicVolume = Mathf.Clamp01(volume);
        AudioSource current = _isUsingSourceA ? _bgmSourceA : _bgmSourceB;
        if (current.isPlaying)
        {
            current.volume = GetEffectiveMusicVolume();
        }
    }

    public void SetSFXVolume(float volume)
    {
        sfxVolume = Mathf.Clamp01(volume);
    }

    private float GetEffectiveMusicVolume()
    {
        if (AudioController.Instance != null && AudioController.Instance.isMusicMuted)
            return 0f;
        return musicVolume;
    }

    private float GetEffectiveSFXVolume()
    {
        if (AudioController.Instance != null && AudioController.Instance.isSFXMuted)
            return 0f;
        return sfxVolume;
    }
}
