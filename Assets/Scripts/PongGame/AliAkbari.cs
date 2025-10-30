using Unity.Cinemachine;
using UnityEngine;

public class AliAkbari : BaseProfessors
{
    [Header("Mini Game")] 
    public GameObject pongGame;
    public PongManager fadeActivator;
    public CinemachineCamera cam;

    public void StartGame()
    {
        PlayerUIModeHelper.PlayerEnterUIMode(_player);
        PlayerUIModeHelper.DisableTasksButton(_allTasksBackButton, _currentTaskButton);
        
        UIUtils.SetAlpha(_currentTaskButton.gameObject, 0f);
        
        pongGame.SetActive(true);
        fadeActivator.FadeInAndActivate();
        cam.Priority = 20;
    }
}
