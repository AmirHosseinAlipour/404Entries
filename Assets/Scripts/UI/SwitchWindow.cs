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
    
    [Header("Task UI")]
    public Button allTasksBackButton;
    public Button currentTaskButton;

    private PlayerController _player;
    
    [HideInInspector] public static event Action<bool> OnPanelStateChanged;
    
    private void Awake()
    {
        _player = GameObject.FindWithTag("Player").GetComponent<PlayerController>();
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
        PlayerUIModeHelper.EnableTasksButton(allTasksBackButton, currentTaskButton);
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
}
