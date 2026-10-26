using UnityEngine;
using System.Collections;

public class Mazaheri : BaseProfessors
{
    [Header("Mini Game")]
    public SpaceInvadersManager game;
    public RectTransform bootPanel; 
    public MusicChange musicChange;
    public GameObject MiniMap; 
    
    public void StartGame()
    {
        StartMinigame();
        if (bootPanel != null) UIAnimationManager.Instance.ShowWindow(bootPanel, 0.5f);
        if (MiniMap != null) MiniMap.SetActive(false);
        if (game != null) game.InitialSettings();
    }

    public void HandleEnding()
    {
        if (bootPanel != null) UIAnimationManager.Instance.HideWindow(bootPanel, 0.5f);
        if (StartPanel != null) UIAnimationManager.Instance.HideWindow(StartPanel, 0.5f);
        CompleteMission();
    }
}