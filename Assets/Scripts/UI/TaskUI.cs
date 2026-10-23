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

    private void OnEnable()
    {
        LocalizationManager.OnLanguageChanged += HandleLanguageChanged;
    }

    private void OnDisable()
    {
        LocalizationManager.OnLanguageChanged -= HandleLanguageChanged;
    }

    private void HandleLanguageChanged(GameLanguage newLang)
    {
        UpdateUI();
    }

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
        if (tm == null) return;

        int currentIndex = tm.GetCurrentTaskIndex();
        if (currentIndex != -1)
        {
            if (_currentTaskFtw != null)
                _currentTaskFtw.SetText(tm.taskNames[currentIndex]);
            else
                currentTaskText.text = LocalizationManager.Translate(tm.taskNames[currentIndex]);
        }
        else
        {
            string doneMsg = "همه مدارک جمع آوری شد!";
            if (_currentTaskFtw != null)
                _currentTaskFtw.SetText(doneMsg);
            else
                currentTaskText.text = LocalizationManager.Translate(doneMsg);
        }
        
        if (_currentTaskDfr != null)
            _currentTaskDfr.AdjustFontSize();
        if (_currentTaskFtw != null)
            _currentTaskFtw.StartTyping();
        
        for (int i = 1; i < allTaskTexts.Length; i += 2)
        {
            if (i < tm.taskNames.Length && allTaskTexts[i] != null)
            {
                string translated = LocalizationManager.Translate(tm.taskNames[i]);
                string _fixedText = LocalizationManager.IsRTL ? ArabicFixer.Fix(translated) : translated;
                allTaskTexts[i].text = _fixedText;
                allTaskTexts[i].alignment = LocalizationManager.IsRTL ? TextAnchor.MiddleRight : TextAnchor.MiddleLeft;
                if (allTaskImages[i] != null)
                    allTaskImages[i].sprite = tm.taskCompleted[i] ? doneSprite : notDoneSprite;
            }
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