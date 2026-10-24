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
        CompleteMission();
        if (StartPanel != null && StartPanel.transform.parent != null)
        {
            RectTransform parent = StartPanel.transform.parent.GetComponent<RectTransform>();
            if (parent != null)
            {
                parent.gameObject.SetActive(false);
                UIAnimationManager.Instance.HideWindow(parent, 0.5f);
            }
        }
    }
}