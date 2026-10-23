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
        if (bootPanel != null) UIAnimationManager.Instance.ShowWindow(bootPanel, 0.5f);
        if (MiniMap != null) MiniMap.SetActive(false);
        if (game != null) game.InitialSettings();
    }

    public void HandleEnding()
    {
        if (musicChange != null) musicChange.PlayMainMusic();
        if (StartPanel != null && StartPanel.transform.parent != null)
        {
            RectTransform parent = StartPanel.transform.parent.GetComponent<RectTransform>();
            if (parent != null)
            {
                parent.gameObject.SetActive(false);
                UIAnimationManager.Instance.HideWindow(parent, 0.5f);
            }
        }
        if (firstWinDialogue != null)
        {
            UIAnimationManager.Instance.ShowDialogueWindow(firstWinDialogue, 0.5f, firstWinDialogueFtw);
        }
    }
}