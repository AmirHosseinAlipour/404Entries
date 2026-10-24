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
    
    protected Button _allTasksBackButton;
    protected Button _currentTaskButton;
    protected GameObject _joyStick;
    protected PlayerController _player;

    protected virtual void Awake()
    {
        FindReferences();
        target = gameObject.GetComponent<Target>();
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
                    UIAnimationManager.Instance.ShowWindow(afterWinDialogue, 0.25f);
                }
            }
            else
            {
                FindReferences();
                PlayerUIModeHelper.PlayerEnterUIMode(_player);
                PlayerUIModeHelper.DisableTasksButton(_allTasksBackButton, _currentTaskButton);
                UIUtils.SetAlpha(_currentTaskButton != null ? _currentTaskButton.gameObject : null, 0f);
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

    private void HandlePanelStateChange(bool isActive)
    {
        if (!isActive)
        {
            GetBackToNormal();
        }
    }

    public virtual void CompleteMission()
    {
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
        GetBackToNormal();

        if (firstFailDialogue != null && UIAnimationManager.Instance != null)
        {
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
        FindReferences();
        PlayerUIModeHelper.PlayerExitUIMode(_player);
        PlayerUIModeHelper.EnableTasksButton(_allTasksBackButton, _currentTaskButton);
        UIUtils.SetAlpha(_currentTaskButton != null ? _currentTaskButton.gameObject : null, 1f);
        if (PlatformHelper.IsMobile())
        {
            UIUtils.SetAlpha(_joyStick, 1f);
        }
    }
}
