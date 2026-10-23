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
    
    // Task UI 
    private Button _allTasksBackButton;
    private Button _currentTaskButton;
    private TaskUI _taskUI;

    private PlayerController _player;
    
    [HideInInspector] public static event Action<bool> OnPanelStateChanged;
    
    private void Awake()
    {
        var playerObj = GameObject.FindWithTag("Player");
        if (playerObj != null) _player = playerObj.GetComponent<PlayerController>();

        var backBtnObj = GameObject.FindWithTag("AllTasksBackButton");
        if (backBtnObj != null) _allTasksBackButton = backBtnObj.GetComponent<Button>();

        var currBtnObj = GameObject.FindWithTag("CurrentTaskButton");
        if (currBtnObj != null) _currentTaskButton = currBtnObj.GetComponent<Button>();
        
        var taskUiObj = GameObject.FindWithTag("TaskUI");
        if (taskUiObj != null) _taskUI = taskUiObj.GetComponent<TaskUI>();
    }

    // This method is only for closing the mini game UI!
    public void ExitMission()
    {
        if (windowToHide) // Null check
        {
            // Hide the window
            if (windowToHideAnimation)
            {
                UIAnimationManager.Instance.HideWindow(windowToHide, 0.5f);
            }
            else
            {
                windowToHide.gameObject.SetActive(false);
            }
        }
        
        PlayerUIModeHelper.PlayerExitUIMode(_player);
        PlayerUIModeHelper.EnableTasksButton(_allTasksBackButton, _currentTaskButton);
    }

    public void Toggle()
    {
        HideWindow();
        
        if (windowToShow) // Null check
        {
            // Show the window
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

    public void DialogueToggle()
    {
        HideWindow();
        
        if (dialogueWindowToShow && dialogueTypeWriter) // Null check
        {
            UIAnimationManager.Instance.ShowDialogueWindow(dialogueWindowToShow, 0.5f, dialogueTypeWriter);
        }
    }

    private void HideWindow()
    {
        if (windowToHide) // Null check
        {
            // Hide the window
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

    public void ShowNextDialogue()
    {
        if (amozesh == null)
        {
            Debug.LogError("Amozesh reference is missing!");
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
                return;
            }
        }
        
        if (dialogueWindowToShow != null && dialogueTypeWriter != null)
        {
            UIAnimationManager.Instance.ShowDialogueWindow(dialogueWindowToShow, 0.5f, dialogueTypeWriter);
        }
        
        HideWindow();
    }
}