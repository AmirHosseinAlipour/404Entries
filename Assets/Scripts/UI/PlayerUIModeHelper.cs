using System;
using UnityEngine;
using UnityEngine.UI;

public static class PlayerUIModeHelper
{
    
    public static void PlayerEnterUIMode(PlayerController player)
    {
        player.isUIActive = true;
    }

    public static void PlayerExitUIMode(PlayerController player)
    {
        player.isUIActive = false;
    }

    public static void DisableTasksButton(Button allTasksBackButton, Button currentTaskButton)
    {
        currentTaskButton.interactable = false;
        allTasksBackButton.interactable = false;
    }
    
    public static void EnableTasksButton(Button allTasksBackButton, Button currentTaskButton)
    {
        currentTaskButton.interactable = true;
        allTasksBackButton.interactable = true;
    }
}