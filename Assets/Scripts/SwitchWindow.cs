using System;
using UnityEngine;

public class SwitchWindow : MonoBehaviour
{
    public RectTransform windowToHide;
    public bool windowToHideAnimation;
    
    public RectTransform windowToShow;
    public bool windowToShowAnimation;

    private PlayerController _player;

    private void Start()
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
                UIAnimationManager.Instance.HideWindow(windowToHide);
            }
            else
            {
                windowToHide.gameObject.SetActive(false);
            }
        }
        
        // Back player inputs to normal
        _player.isUIActive = false;
    }

    public void Toggle()
    {
        if (windowToHide) // Null check
        {
            // Hide the window
            if (windowToHideAnimation)
            {
                UIAnimationManager.Instance.HideWindow(windowToHide);
            }
            else
            {
                windowToHide.gameObject.SetActive(false);
            }
        }
        
        if (windowToShow) // Null check
        {
            // Show the window
            if (windowToShowAnimation)
            {
                UIAnimationManager.Instance.ShowWindow(windowToShow);
            }
            else
            {
                windowToShow.gameObject.SetActive(false);
            }
        }
    }
}
