using System;
using UnityEngine;

public abstract class BaseMinigame : MonoBehaviour, IMinigame
{
    public BaseProfessors professor;
    public int fallbackTaskIndex = -1;

    public event Action OnMinigameStarted;
    public event Action OnMinigameWon;
    public event Action OnMinigameLost;

    public virtual void StartMinigame()
    {
        BaseProfessors.IsAnyMinigameActive = true;
        TaskUI.SetTopUIVisible(false);
        OnMinigameStarted?.Invoke();
    }

    public virtual void CompleteMinigame()
    {
        BaseProfessors.IsAnyMinigameActive = false;
        OnMinigameWon?.Invoke();
        if (professor != null)
        {
            professor.CompleteMission();
        }
        else if (TaskManager.Instance != null && fallbackTaskIndex >= 0)
        {
            TaskManager.Instance.CompleteTask(fallbackTaskIndex);
            TaskUI.SetTopUIVisible(true);
        }
        else if (TaskManager.Instance != null)
        {
            TaskManager.Instance.CompleteCurrentTask();
            TaskUI.SetTopUIVisible(true);
        }
        else
        {
            TaskUI.SetTopUIVisible(true);
        }
    }

    public virtual void FailMinigame()
    {
        BaseProfessors.IsAnyMinigameActive = false;
        OnMinigameLost?.Invoke();
        if (professor != null)
        {
            professor.FailMission();
        }
        else
        {
            TaskUI.SetTopUIVisible(true);
        }
    }
}
