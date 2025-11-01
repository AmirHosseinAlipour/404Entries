using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class AudioSettingsUI : MonoBehaviour
{
    [Header("Music UI References")]
    public Slider musicSlider;
    public Toggle musicMuteToggle;
    public Image musicMuteIcon;

    [Header("SFX UI References")]
    public Slider sfxSlider;
    public Toggle sfxMuteToggle;
    public Image sfxMuteIcon;

    [Header("Mute Sprites")]
    public Sprite muteSprite;
    public Sprite unmuteSprite;

    private void Start()
    {
        if (AudioController.Instance == null)
        {
            Debug.LogError("AudioController.Instance not found! AudioSettingsUI cannot initialize.", this.gameObject);
            return;
        }

        if (musicSlider != null)
            musicSlider.onValueChanged.AddListener(OnMusicSliderChanged);
        if (sfxSlider != null)
            sfxSlider.onValueChanged.AddListener(OnSfxSliderChanged);
        if (sfxMuteToggle != null)
            sfxMuteToggle.onValueChanged.AddListener(OnSfxMuteToggleChanged);
        if (musicMuteToggle != null)
            musicMuteToggle.onValueChanged.AddListener(OnMusicMuteToggleChanged);

        UpdateAllUI();
    }

    public void OnSfxSliderChanged(float val)
    {
        if (AudioController.Instance == null) return;
        AudioController.Instance.SetSFXVolume(val / 100f);

        if (val > 0 && AudioController.Instance.isSFXMuted)
        {
            AudioController.Instance.ToggleSFXMute(false);
        }
        UpdateAllUI();
    }

    public void OnMusicSliderChanged(float val)
    {
        if (AudioController.Instance == null) return;
        AudioController.Instance.SetMusicVolume(val / 100f);
        if (val > 0 && AudioController.Instance.isMusicMuted)
        {
            AudioController.Instance.ToggleMusicMute(false);
        }
        UpdateAllUI();
    }

    public void OnSfxMuteToggleChanged(bool isMuted)
    {
        if (AudioController.Instance == null) return;
        AudioController.Instance.ToggleSFXMute(isMuted);
        UpdateAllUI();
    }

    public void OnMusicMuteToggleChanged(bool isMuted)
    {
        if (AudioController.Instance == null) return;
        AudioController.Instance.ToggleMusicMute(isMuted);
        UpdateAllUI();
    }

    private void UpdateAllUI()
    {
        if (AudioController.Instance == null) return;
        
        float sfxVol = AudioController.Instance.sfxVolume;
        float musicVol = AudioController.Instance.musicVolume;
        bool isSfxMuted = AudioController.Instance.isSFXMuted;
        bool isMusicMuted = AudioController.Instance.isMusicMuted;

        // Update SFX UI
        if (sfxSlider != null)
            sfxSlider.SetValueWithoutNotify(isSfxMuted ? 0 : sfxVol * 100);
        if (sfxMuteToggle != null)
            sfxMuteToggle.SetIsOnWithoutNotify(isSfxMuted);
        if (sfxMuteIcon != null && muteSprite != null && unmuteSprite != null)
            sfxMuteIcon.sprite = isSfxMuted ? muteSprite : unmuteSprite;

        // Update Music UI
        if (musicSlider != null)
            musicSlider.SetValueWithoutNotify(isMusicMuted ? 0 : musicVol * 100);
        if (musicMuteToggle != null)
            musicMuteToggle.SetIsOnWithoutNotify(isMusicMuted);
        if (musicMuteIcon != null && muteSprite != null && unmuteSprite != null)
            musicMuteIcon.sprite = isMusicMuted ? muteSprite : unmuteSprite;
    }
}