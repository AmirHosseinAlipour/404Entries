using UnityEngine;

public class FailOrPass : BaseProfessors
{
    [Header("Mini Game")]
    public RectTransform task;

    public void StartGame()
    {
        UIAnimationManager.Instance.ShowWindow(task, 0.5f);
    }
}