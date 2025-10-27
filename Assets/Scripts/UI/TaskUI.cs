using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using ArabicSupport;

public class TaskUI : MonoBehaviour
{
    [Header("Panels")]
    public GameObject currentTaskPanel;
    public GameObject allTasksPanel;

    [Header("UI Elements")]
    public Text currentTaskText;
    public Text[] allTaskTexts;
    public Image[] allTaskImages;

    [Header("Sprites")]
    public Sprite doneSprite;
    public Sprite notDoneSprite;

    private FarsiTypewriter _currentTaskFtw;
    private DynamicFontResizer _currentTaskDfr;
    
    private bool firstTime = true;

    private void Start()
    {
        _currentTaskFtw = currentTaskText.GetComponent<FarsiTypewriter>();
        _currentTaskDfr = currentTaskText.GetComponent<DynamicFontResizer>();
        
        UpdateUI();
    }

    private void Update()
    {
        if (firstTime)
        {
            firstTime = false;
            allTasksPanel.SetActive(false);
            UpdateUI();
        }
    }

    public void UpdateUI()
    {
        var tm = TaskManager.Instance;


        int currentIndex = tm.GetCurrentTaskIndex();
        if (currentIndex != -1)
        {
            _currentTaskFtw.SetText(tm.taskNames[currentIndex]);
        }
        else
        {
            currentTaskText.text = "همه مدارک جمع آوری شد!";
        }
        
        _currentTaskDfr.AdjustFontSize();
        _currentTaskFtw.StartTyping();
        
        
        for (int i = 1; i < allTaskTexts.Length; i+=2)
        {
            string _fixedText;
            _fixedText = ArabicFixer.Fix(tm.taskNames[i]);
            allTaskTexts[i].text = _fixedText;
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