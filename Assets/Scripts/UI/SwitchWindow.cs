using System;
using UnityEngine;
using UnityEngine.UI;

public class SwitchWindow : MonoBehaviour
{
    [Header("Switch Window Settings")]
    public RectTransform windowToHide;
    public bool windowToHideAnimation;
    public bool shouldInvoke;
    
    public RectTransform windowToShow;
    public bool windowToShowAnimation;

    public RectTransform dialogueWindowToShow;
    public FarsiTypewriter dialogueTypeWriter;
    
    private Button _allTasksBackButton;
    private Button _currentTaskButton;
    private TaskUI _taskUI;
    private PlayerController _player;
    
    [HideInInspector] public static event Action<bool> OnPanelStateChanged;
    
    private void Awake()
    {
        FindReferences();
    }

    private void FindReferences()
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
        
        if (_taskUI == null)
        {
            var taskUiObj = GameObject.FindWithTag("TaskUI");
            if (taskUiObj != null) _taskUI = taskUiObj.GetComponent<TaskUI>();
        }
    }

    public void ExitMission()
    {
        if (windowToHide != null)
        {
            if (windowToHideAnimation)
            {
                UIAnimationManager.Instance.HideWindow(windowToHide, 0.5f);
            }
            else
            {
                windowToHide.gameObject.SetActive(false);
            }
        }
        
        BaseProfessors.IsAnyMinigameActive = false;
        BaseProfessors.ActiveProfessor = null;
        FindReferences();
        PlayerUIModeHelper.PlayerExitUIMode(_player);
        PlayerUIModeHelper.EnableTasksButton(_allTasksBackButton, _currentTaskButton);
    }

    public void Toggle()
    {
        HideWindow();
        
        if (windowToShow != null)
        {
            if (windowToShowAnimation)
            {
                UIAnimationManager.Instance.ShowWindow(windowToShow, 0.5f);
            }
            else
            {
                windowToShow.gameObject.SetActive(true);
            }
        }
    }

    private float _lastSkipTime = -1f;

    private FarsiTypewriter GetCurrentTypewriter()
    {
        if (windowToHide != null)
        {
            FarsiTypewriter ftw = windowToHide.GetComponentInChildren<FarsiTypewriter>(true);
            if (ftw != null) return ftw;
        }
        return GetComponentInChildren<FarsiTypewriter>(true);
    }

    public void DialogueToggle()
    {
        FarsiTypewriter currentFtw = GetCurrentTypewriter();
        if (currentFtw != null && currentFtw.isTyping)
        {
            currentFtw.SkipTyping();
            _lastSkipTime = Time.time;
            return;
        }

        if (Time.time - _lastSkipTime < 0.15f)
        {
            return;
        }

        HideWindow();
        
        if (dialogueWindowToShow != null)
        {
            UIAnimationManager.Instance.ShowDialogueWindow(dialogueWindowToShow, 0.5f, dialogueTypeWriter);
        }
    }

    private void HideWindow()
    {
        if (windowToHide != null)
        {
            if (windowToHideAnimation)
            {
                UIAnimationManager.Instance.HideWindow(windowToHide, 0.3f);
            }
            else
            {
                windowToHide.gameObject.SetActive(false);
            }

            if (OnPanelStateChanged != null && shouldInvoke)
            {
                OnPanelStateChanged.Invoke(false);
            }
        }
    }
    
    public int currentDialogueIndex = 0;
    public Amozesh amozesh;

    private void Update()
    {
        if (windowToHide == null || !windowToHide.gameObject.activeInHierarchy) return;

        bool advance = false;
        if (UnityEngine.InputSystem.Keyboard.current != null)
        {
            if (UnityEngine.InputSystem.Keyboard.current.spaceKey.wasPressedThisFrame ||
                UnityEngine.InputSystem.Keyboard.current.enterKey.wasPressedThisFrame)
            {
                advance = true;
            }
        }
        if (!advance && (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Return)))
        {
            advance = true;
        }

        if (advance)
        {
            ShowNextDialogue();
        }
    }

    public void ShowNextDialogue()
    {
        FarsiTypewriter currentFtw = GetCurrentTypewriter();
        if (currentFtw != null && currentFtw.isTyping)
        {
            currentFtw.SkipTyping();
            _lastSkipTime = Time.time;
            return;
        }

        if (Time.time - _lastSkipTime < 0.15f)
        {
            return;
        }

        if (amozesh == null)
        {
            return;
        }
        
        int pIdx = amozesh.getCrrrentIndex();
        if (amozesh.dialoguePhases != null && pIdx < amozesh.dialoguePhases.Count && amozesh.dialoguePhases[pIdx].dialogues != null)
        {
            if (amozesh.dialoguePhases[pIdx].dialogues.Count <= currentDialogueIndex)
            {
                shouldInvoke = true;
                HideWindow();
                if (_taskUI != null) _taskUI.UpdateUI();
                amozesh.OnDialogueFinished(pIdx);
                return;
            }
        }
        
        if (dialogueWindowToShow != null)
        {
            UIAnimationManager.Instance.ShowDialogueWindow(dialogueWindowToShow, 0.5f, dialogueTypeWriter);
        }
        
        HideWindow();
    }
}