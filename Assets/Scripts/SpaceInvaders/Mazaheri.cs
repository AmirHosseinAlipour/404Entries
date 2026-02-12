using UnityEngine;
using System.Collections;
using Unity.VisualScripting; // حتماً برای Coroutine اضافه شود

public class Mazaheri : BaseProfessors
{
    [Header("Mini Game")]
    public SpaceInvadersManager game;
    public RectTransform bootPanel; 
    public  MusicChange musicChange;
    public GameObject MiniMap; 
    
    public void StartGame()
    {
        UIAnimationManager.Instance.ShowWindow(bootPanel, 0.5f);
        MiniMap.SetActive(false);
        game.InitialSettings();
    }

    public void HandleEnding()
    {
        musicChange.PlayMainMusic();
        RectTransform parent = StartPanel.transform.parent.GetComponent<RectTransform>();
        parent.gameObject.SetActive(false);
        UIAnimationManager.Instance.HideWindow(parent, 0.5f);
        UIAnimationManager.Instance.ShowDialogueWindow(firstWinDialogue, 0.5f, firstWinDialogueFtw);
        
    }
}