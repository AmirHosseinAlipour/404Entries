using System;
using UnityEngine;
using UnityEngine.UI;

public static class PlayerUIModeHelper
{
    public static void PlayerEnterUIMode(PlayerController player = null)
    {
        if (player == null)
        {
            var pObj = GameObject.FindWithTag("Player");
            if (pObj != null) player = pObj.GetComponent<PlayerController>();
        }
        if (player != null) player.isUIActive = true;
    }

    public static void PlayerExitUIMode(PlayerController player = null)
    {
        if (player == null)
        {
            var pObj = GameObject.FindWithTag("Player");
            if (pObj != null) player = pObj.GetComponent<PlayerController>();
        }
        if (player != null) player.isUIActive = false;
    }

    public static void DisableTasksButton(Button allTasksBackButton, Button currentTaskButton)
    {
        if (currentTaskButton != null) currentTaskButton.interactable = false;
        if (allTasksBackButton != null) allTasksBackButton.interactable = false;
        TaskUI.SetTopUIVisible(false);
    }
    
    public static void EnableTasksButton(Button allTasksBackButton, Button currentTaskButton)
    {
        if (BaseProfessors.IsAnyMinigameActive)
        {
            return;
        }
        if (currentTaskButton != null) currentTaskButton.interactable = true;
        if (allTasksBackButton != null) allTasksBackButton.interactable = true;
        TaskUI.SetTopUIVisible(true);
    }
}