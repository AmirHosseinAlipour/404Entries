using UnityEngine;
using System.Collections;

public class Mazaheri : BaseProfessors
{
    public override ProfessorUIType UIType => ProfessorUIType.InWorldGame;

    [Header("Mini Game")]
    public SpaceInvadersManager game;
    public RectTransform bootPanel; 
    public MusicChange musicChange;
    public GameObject MiniMap; 

    protected override void Start()
    {
        base.Start();
        HideMinigameUI();
    }

    public override void HideMinigameUI()
    {
        base.HideMinigameUI();
        if (bootPanel != null) bootPanel.gameObject.SetActive(false);
    }

    public override void AcceptChallenge()
    {
        StartGame();
    }
    
    public void StartGame()
    {
        StartMinigame();
        if (bootPanel != null) UIAnimationManager.Instance.ShowWindow(bootPanel, 0.5f);
        if (game != null) game.InitialSettings();
    }

    public void HandleEnding()
    {
        if (bootPanel != null) UIAnimationManager.Instance.HideWindow(bootPanel, 0.5f);
        if (StartPanel != null) UIAnimationManager.Instance.HideWindow(StartPanel, 0.5f);
        CompleteMission();
    }
}