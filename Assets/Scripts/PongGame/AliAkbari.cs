using Unity.Cinemachine;
using UnityEngine;

public class AliAkbari : BaseProfessors
{
    [Header("Mini Game")] 
    public GameObject pongGame;
    public PongManager fadeActivator;
    private MusicChange musicChange;

    private void Start()
    {
        musicChange = GetComponent<MusicChange>();
    }
    public void StartGame()
    {
        UIUtils.SetAlpha(_currentTaskButton.gameObject, 0f);
        fadeActivator.InitialSettings();
        pongGame.SetActive(true);
        fadeActivator.FadeInAndActivate();
        musicChange.ToggleMusic();
        musicChange.PlayThemeMusic();
    }
}
