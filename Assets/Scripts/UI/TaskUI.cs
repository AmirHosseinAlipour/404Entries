using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
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
    private Button _currentTaskBtn;
    
    private bool firstTime = true;

    private void OnEnable()
    {
        LocalizationManager.OnLanguageChanged += HandleLanguageChanged;
        TaskManager.OnTasksUpdated += HandleTasksUpdated;
        TaskManager.OnTaskStateChanged += HandleTaskStateChanged;
        InputReader.OnToggleTasksPressed += HandleToggleTasksInput;
    }

    private void OnDisable()
    {
        LocalizationManager.OnLanguageChanged -= HandleLanguageChanged;
        TaskManager.OnTasksUpdated -= HandleTasksUpdated;
        TaskManager.OnTaskStateChanged -= HandleTaskStateChanged;
        InputReader.OnToggleTasksPressed -= HandleToggleTasksInput;
    }

    private void HandleLanguageChanged(GameLanguage newLang)
    {
        UpdateUI();
    }

    private void HandleTasksUpdated()
    {
        UpdateUI();
    }

    private void HandleTaskStateChanged(int index, bool completed)
    {
        UpdateUI();
    }

    private void Start()
    {
        if (currentTaskText != null)
        {
            _currentTaskFtw = currentTaskText.GetComponent<FarsiTypewriter>();
            if (_currentTaskFtw != null)
            {
                _currentTaskFtw.allowClickToSkip = false;
            }
            _currentTaskDfr = currentTaskText.GetComponent<DynamicFontResizer>();
        }

        if (currentTaskPanel != null)
        {
            _currentTaskBtn = currentTaskPanel.GetComponent<Button>();
            if (_currentTaskBtn == null)
            {
                _currentTaskBtn = currentTaskPanel.GetComponentInChildren<Button>();
            }
        }
        
        UpdateUI();
    }

    private void Update()
    {
        if (firstTime)
        {
            firstTime = false;
            if (allTasksPanel != null)
            {
                allTasksPanel.SetActive(false);
            }
            UpdateUI();
        }
    }

    private void HandleToggleTasksInput()
    {
        bool isTaskDisabled = (_currentTaskBtn != null && !_currentTaskBtn.interactable);
        if (!isTaskDisabled || (allTasksPanel != null && allTasksPanel.activeSelf))
        {
            ToggleAllTasksPanel();
        }
    }

    public void UpdateUI()
    {
        var tm = TaskManager.Instance;
        if (tm == null) return;

        int currentIndex = tm.GetCurrentTaskIndex();
        if (currentTaskText != null)
        {
            string taskName = (currentIndex != -1 && tm.taskNames != null && currentIndex < tm.taskNames.Length)
                ? tm.taskNames[currentIndex]
                : "همه مدارک جمع آوری شد!";

            string translated = LocalizationManager.Translate(taskName);

            if (_currentTaskDfr != null)
            {
                string measureText = LocalizationManager.IsRTL ? ArabicFixer.Fix(translated, true, true) : translated;
                _currentTaskDfr.SetTextAndResize(measureText);
            }

            if (_currentTaskFtw != null)
            {
                _currentTaskFtw.SetText(taskName);
            }
            else
            {
                currentTaskText.text = LocalizationManager.IsRTL ? ArabicFixer.Fix(translated, true, true) : translated;
            }
        }

        if (allTaskTexts == null || tm.taskNames == null || tm.taskCompleted == null) return;

        bool hasNullGaps = false;
        for (int i = 0; i < allTaskTexts.Length; i += 2)
        {
            if (allTaskTexts[i] == null)
            {
                hasNullGaps = true;
                break;
            }
        }

        if (hasNullGaps)
        {
            for (int i = 1; i < allTaskTexts.Length; i += 2)
            {
                if (allTaskTexts[i] == null) continue;
                if (i < tm.taskNames.Length)
                {
                    string translated = LocalizationManager.Translate(tm.taskNames[i]);
                    string fixedText = LocalizationManager.IsRTL ? ArabicFixer.Fix(translated) : translated;
                    allTaskTexts[i].text = fixedText;
                    allTaskTexts[i].alignment = LocalizationManager.IsRTL ? TextAnchor.MiddleRight : TextAnchor.MiddleLeft;
                    if (allTaskImages != null && i < allTaskImages.Length && allTaskImages[i] != null)
                    {
                        bool isDone = i < tm.taskCompleted.Length && tm.taskCompleted[i];
                        allTaskImages[i].sprite = isDone ? doneSprite : notDoneSprite;
                    }
                }
            }
        }
        else
        {
            int missionTaskIndex = 1;
            for (int i = 0; i < allTaskTexts.Length; i++)
            {
                if (allTaskTexts[i] == null) continue;
                while (missionTaskIndex < tm.taskNames.Length && missionTaskIndex % 2 == 0)
                {
                    missionTaskIndex++;
                }
                if (missionTaskIndex < tm.taskNames.Length)
                {
                    string translated = LocalizationManager.Translate(tm.taskNames[missionTaskIndex]);
                    string fixedText = LocalizationManager.IsRTL ? ArabicFixer.Fix(translated) : translated;
                    allTaskTexts[i].text = fixedText;
                    allTaskTexts[i].alignment = LocalizationManager.IsRTL ? TextAnchor.MiddleRight : TextAnchor.MiddleLeft;
                    if (allTaskImages != null && i < allTaskImages.Length && allTaskImages[i] != null)
                    {
                        bool isDone = missionTaskIndex < tm.taskCompleted.Length && tm.taskCompleted[missionTaskIndex];
                        allTaskImages[i].sprite = isDone ? doneSprite : notDoneSprite;
                    }
                    missionTaskIndex += 2;
                }
            }
        }
    }
    
    public void ToggleAllTasksPanel()
    {
        if (allTasksPanel == null || currentTaskPanel == null) return;

        bool isActive = allTasksPanel.activeSelf;
        if (isActive)
        {
            RectTransform allRect = allTasksPanel.GetComponent<RectTransform>();
            UIAnimationManager.Instance.HideWindow(allRect, 0.5f);
            RectTransform currRect = currentTaskPanel.GetComponent<RectTransform>();
            UIAnimationManager.Instance.ShowWindow(currRect, 0.5f);
        }
        else
        {
            UpdateUI();
            RectTransform currRect = currentTaskPanel.GetComponent<RectTransform>();
            UIAnimationManager.Instance.HideWindow(currRect, 0.5f);
            RectTransform allRect = allTasksPanel.GetComponent<RectTransform>();
            UIAnimationManager.Instance.ShowWindow(allRect, 0.5f);
        }
    }
}