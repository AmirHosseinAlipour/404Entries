using UnityEngine;
using System.Collections; // حتماً برای Coroutine اضافه شود

public class Mazaheri : BaseProfessors
{
    public SpaceInvadersManager game;
    public void StartGame()
    {
        game.PlayerEnteredTrigger();
    }

    public void HandleEnding()
    {
        RectTransform parent = StartPanel.transform.parent.GetComponent<RectTransform>();
        parent.gameObject.SetActive(false);
        UIAnimationManager.Instance.HideWindow(parent, 0.5f);
        UIAnimationManager.Instance.ShowDialogueWindow(firstDialogue.GetComponent<RectTransform>(), 0.5f, firstDialogueFtw);
    }
}