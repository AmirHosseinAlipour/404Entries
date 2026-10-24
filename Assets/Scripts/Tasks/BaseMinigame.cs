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
        OnMinigameStarted?.Invoke();
    }

    public virtual void CompleteMinigame()
    {
        OnMinigameWon?.Invoke();
        if (professor != null)
        {
            professor.CompleteMission();
        }
        else if (TaskManager.Instance != null && fallbackTaskIndex >= 0)
        {
            TaskManager.Instance.CompleteTask(fallbackTaskIndex);
        }
        else if (TaskManager.Instance != null)
        {
            TaskManager.Instance.CompleteCurrentTask();
        }
    }

    public virtual void FailMinigame()
    {
        OnMinigameLost?.Invoke();
        if (professor != null)
        {
            professor.FailMission();
        }
    }
}
