using System;
using UnityEngine;

public class SleepProfessor : BaseProfessors
{
    [Header("Mini Game")]
    public RectTransform MiniGamePanel;
    public SleepManager GameManager;

    private void Start()
    {
        MiniGamePanel.gameObject.SetActive(false);
    }

    public void StartGame()
    {
        UIAnimationManager.Instance.ShowWindow(MiniGamePanel , 0.5f);
    }
}
