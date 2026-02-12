using UnityEngine;

public class MainMusicChanger : MonoBehaviour
{
    public AudioClip[] soudns;
    public AudioSource ThemeMusic;
    public RectTransform MusicPlayer; 
    public void ChangeMusic(int i)
    {
        ThemeMusic.clip = soudns[i];
    }

    public void  ToggleOpenMusic()
    {
        UIAnimationManager.Instance.ShowWindow(MusicPlayer , 0.5f );
    }

    public void ToggleCloseMusic()
    {
        UIAnimationManager.Instance.HideWindow(MusicPlayer , 0.5f );
    }


}
