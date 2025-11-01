using Unity.Cinemachine;
using UnityEngine;

public class AliAkbari : BaseProfessors
{
    [Header("Mini Game")] 
    public GameObject pongGame;
    public PongManager fadeActivator;

    public void StartGame()
    {
        UIUtils.SetAlpha(_currentTaskButton.gameObject, 0f);
        fadeActivator.InitialSettings();
        pongGame.SetActive(true);
        fadeActivator.FadeInAndActivate();
    }
}
