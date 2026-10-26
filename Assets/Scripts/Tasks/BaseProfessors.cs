using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class BaseProfessors : MonoBehaviour
{
    [Header("Base Fields")]
    public RectTransform StartPanel;
    public RectTransform acceptRect;
    public Text[] listOfTexts;
    public Target target;

    private const int InitialDialogueCount = 3;
    
    public int TaskOrderNumber;
    public TaskData assignedTask;
    
    [Header("After mission")]
    public RectTransform afterWinDialogue;
    public RectTransform firstWinDialogue;
    public FarsiTypewriter firstWinDialogueFtw;
    
    [Header("After task not complete")]
    public RectTransform afterFailDialogue;
    public RectTransform firstFailDialogue;
    public FarsiTypewriter firstFailDialogueFtw;
    
    public static bool IsAnyMinigameActive = false;
    public static BaseProfessors ActiveProfessor = null;
    public bool isMinigameActive = false;

    protected Button _allTasksBackButton;
    protected Button _currentTaskButton;
    protected GameObject _joyStick;
    protected PlayerController _player;

    protected virtual void Awake()
    {
        FindReferences();
        target = gameObject.GetComponent<Target>();
        EnsureDialogueSortingOrder(afterWinDialogue);
        EnsureDialogueSortingOrder(firstWinDialogue);
        EnsureDialogueSortingOrder(afterFailDialogue);
        EnsureDialogueSortingOrder(firstFailDialogue);
    }

    public void EnsureDialogueSortingOrder(RectTransform rect)
    {
        if (rect == null) return;
        Canvas c = rect.GetComponentInParent<Canvas>();
        if (c != null && c.sortingOrder < 10)
        {
            c.overrideSorting = true;
            c.sortingOrder = 10;
        }
    }

    protected virtual void Start()
    {
        if (_player == null)
        {
            FindReferences();
        }
    }

    protected void FindReferences()
    {
        if (_player == null)
        {
            var playerObj = GameObject.FindWithTag("Player");
            if (playerObj != null) _player = playerObj.GetComponent<PlayerController>();
        }
        
        if (_allTasksBackButton == null)
        {
            var backBtnObj = GameObject.FindWithTag("AllTasksBackButton");
            if (backBtnObj != null) _allTasksBackButton = backBtnObj.GetComponent<Button>();
        }

        if (_currentTaskButton == null)
        {
            var currBtnObj = GameObject.FindWithTag("CurrentTaskButton");
            if (currBtnObj != null) _currentTaskButton = currBtnObj.GetComponent<Button>();
        }

        if (_joyStick == null)
        {
            _joyStick = GameObject.FindWithTag("JoyStick");
        }
    }

    protected virtual void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            if (isMinigameActive || IsAnyMinigameActive)
            {
                return;
            }

            if (_player == null) _player = other.gameObject.GetComponent<PlayerController>();

            bool isTaskDone = false;
            if (assignedTask != null && TaskManager.Instance != null)
            {
                isTaskDone = TaskManager.Instance.IsTaskCompleted(assignedTask);
            }
            else if (TaskManager.Instance != null && TaskManager.Instance.taskCompleted != null && 
                     TaskOrderNumber >= 0 && TaskOrderNumber < TaskManager.Instance.taskCompleted.Length)
            {
                isTaskDone = TaskManager.Instance.taskCompleted[TaskOrderNumber];
            }

            if (isTaskDone)
            {
                if (afterWinDialogue != null)
                {
                    EnsureDialogueSortingOrder(afterWinDialogue);
                    UIAnimationManager.Instance.ShowWindow(afterWinDialogue, 0.25f);
                }
            }
            else
            {
                ActiveProfessor = this;
                FindReferences();
                PlayerUIModeHelper.PlayerEnterUIMode(_player);
                PlayerUIModeHelper.DisableTasksButton(_allTasksBackButton, _currentTaskButton);
                TaskUI.SetTopUIVisible(false);
                UIUtils.SetAlpha(_joyStick, 0f);
                
                if (StartPanel != null)
                {
                    UIAnimationManager.Instance.ShowWindow(StartPanel, 0.5f);
                }
                
                if (listOfTexts != null)
                {
                    for (int i = 0; i < Mathf.Min(3, listOfTexts.Length); i++)
                    {
                        if (listOfTexts[i] != null)
                        {
                            listOfTexts[i].text = "";
                        }
                    }
                }
            
                StartCoroutine(TextSequence());
            }
        }
    }

    private IEnumerator TextSequence()
    {
        for (int i = 0; i < InitialDialogueCount; i++)
        {
            if (listOfTexts != null && i < listOfTexts.Length && listOfTexts[i] != null)
            {
                FarsiTypewriter text = listOfTexts[i].gameObject.GetComponent<FarsiTypewriter>();
                if (text != null)
                {
                    int len = text.len;
                    text.StartTyping();
                    yield return new WaitForSeconds(text.typeCharTime * len);
                }
            }
        }
    }

    protected virtual void OnEnable()
    {
        SwitchWindow.OnPanelStateChanged += HandlePanelStateChange;
    }

    protected virtual void OnDisable()
    {
        SwitchWindow.OnPanelStateChanged -= HandlePanelStateChange;
    }

    public virtual void StartMinigame()
    {
        isMinigameActive = true;
        IsAnyMinigameActive = true;
        ActiveProfessor = this;
        FindReferences();
        PlayerUIModeHelper.PlayerEnterUIMode(_player);
        PlayerUIModeHelper.DisableTasksButton(_allTasksBackButton, _currentTaskButton);
        TaskUI.SetTopUIVisible(false);
        UIUtils.SetAlpha(_joyStick, 0f);
    }

    private void HandlePanelStateChange(bool isActive)
    {
        if (!isActive)
        {
            if (IsAnyMinigameActive || isMinigameActive)
            {
                return;
            }
            if (ActiveProfessor != null && ActiveProfessor != this)
            {
                return;
            }
            GetBackToNormal();
            if (ActiveProfessor == this)
            {
                ActiveProfessor = null;
            }
        }
    }

    public virtual void CompleteMission()
    {
        isMinigameActive = false;
        IsAnyMinigameActive = false;
        if (ActiveProfessor == this)
        {
            ActiveProfessor = null;
        }

        if (assignedTask != null && TaskManager.Instance != null)
        {
            TaskManager.Instance.CompleteTask(assignedTask);
        }
        else if (TaskManager.Instance != null)
        {
            TaskManager.Instance.CompleteTask(TaskOrderNumber);
        }

        GetBackToNormal();

        if (firstWinDialogue != null && UIAnimationManager.Instance != null)
        {
            EnsureDialogueSortingOrder(firstWinDialogue);
            UIAnimationManager.Instance.ShowDialogueWindow(firstWinDialogue, 0.5f, firstWinDialogueFtw);
        }

        var music = GetComponent<MusicChange>();
        if (music != null)
        {
            music.ToggleMusic();
            music.PlayMainMusic();
        }

        if (target != null)
        {
            target.DisableTarget();
        }
    }

    public virtual void FailMission()
    {
        isMinigameActive = false;
        IsAnyMinigameActive = false;
        if (ActiveProfessor == this)
        {
            ActiveProfessor = null;
        }

        GetBackToNormal();

        if (firstFailDialogue != null && UIAnimationManager.Instance != null)
        {
            EnsureDialogueSortingOrder(firstFailDialogue);
            UIAnimationManager.Instance.ShowDialogueWindow(firstFailDialogue, 0.5f, firstFailDialogueFtw);
        }

        var music = GetComponent<MusicChange>();
        if (music != null)
        {
            music.ToggleMusic();
            music.PlayMainMusic();
        }
    }

    public void GetBackToNormal()
    {
        if (IsAnyMinigameActive || isMinigameActive)
        {
            return;
        }

        FindReferences();
        PlayerUIModeHelper.PlayerExitUIMode(_player);
        PlayerUIModeHelper.EnableTasksButton(_allTasksBackButton, _currentTaskButton);
        TaskUI.SetTopUIVisible(true);
        if (PlatformHelper.IsMobile())
        {
            UIUtils.SetAlpha(_joyStick, 1f);
        }
    }
}
