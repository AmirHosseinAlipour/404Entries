using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TaskUI : MonoBehaviour
{
    [Header("Panels")]
    public GameObject currentTaskPanel;
    public GameObject allTasksPanel;

    [Header("UI Elements")]
    public TMP_Text currentTaskText;
    public TMP_Text[] allTaskTexts;
    public Image[] allTaskImages;

    [Header("Sprites")]
    public Sprite doneSprite;      
    public Sprite notDoneSprite;   

    private void Start()
    {
        UpdateUI();
    }

    public void UpdateUI()
    {
        var tm = TaskManager.Instance;

     
        int currentIndex = tm.GetCurrentTaskIndex();
        if (currentIndex != -1)
            currentTaskText.text = tm.taskNames[currentIndex];
        else
            currentTaskText.text = "All tasks completed!";

        
        for (int i = 0; i < allTaskTexts.Length; i++)
        {
            allTaskTexts[i].text = tm.taskNames[i];
            allTaskImages[i].sprite = tm.taskCompleted[i] ? doneSprite : notDoneSprite;
        }
    }
    
    public void ToggleAllTasksPanel()
    {
        bool isActive = allTasksPanel.activeSelf;
        if (isActive)
        {
            RectTransform rectTransform = allTasksPanel.GetComponent<RectTransform>();
            UIAnimationManager.Instance.HideWindow(rectTransform, 0.5f);
            rectTransform = currentTaskPanel.GetComponent<RectTransform>();
            UIAnimationManager.Instance.ShowWindow(rectTransform, 0.5f);
        }
        else
        {
            RectTransform rectTransform = currentTaskPanel.GetComponent<RectTransform>();
            UIAnimationManager.Instance.HideWindow(rectTransform , 0.5f);
            rectTransform = allTasksPanel.GetComponent<RectTransform>();
            UIAnimationManager.Instance.ShowWindow(rectTransform, 0.5f);
        }
    }
}