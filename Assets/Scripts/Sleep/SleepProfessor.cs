using System;
using UnityEngine;
using UnityEngine.UI;

public class SleepProfessor : BaseProfessors
{
    public override ProfessorUIType UIType => ProfessorUIType.InteractiveHUD;

    [Header("Mini Game")]
    public RectTransform MiniGamePanel;
    public SleepManager GameManager;

    protected override void Start()
    {
        base.Start();
        HideMinigameUI();
        CleanSleepHUD();
    }

    public override void HideMinigameUI()
    {
        base.HideMinigameUI();
        if (MiniGamePanel != null)
        {
            MiniGamePanel.gameObject.SetActive(false);
        }
    }

    public override void AcceptChallenge()
    {
        StartGame();
    }

    private void CleanSleepHUD()
    {
        if (MiniGamePanel == null) return;
        Image img = MiniGamePanel.GetComponent<Image>();
        if (img != null)
        {
            Color c = img.color;
            c.a = 0f;
            img.color = c;
        }

        Image[] imgs = MiniGamePanel.GetComponentsInChildren<Image>(true);
        for (int i = 0; i < imgs.Length; i++)
        {
            if (imgs[i] == null) continue;
            string n = imgs[i].gameObject.name.ToLower();
            if (n.Contains("clickbutton") || n.Contains("touchbutton") || n.Contains("click_button"))
            {
                Color c = imgs[i].color;
                c.a = 0f;
                imgs[i].color = c;
                imgs[i].raycastTarget = true;
            }
        }
    }

    public void StartGame()
    {
        StartMinigame();
        CleanSleepHUD();
        if (MiniGamePanel != null)
        {
            UIAnimationManager.Instance.ShowWindow(MiniGamePanel, 0.5f);
        }
        if (GameManager != null)
        {
            GameManager.StartGame();
        }
    }
}
