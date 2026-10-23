using UnityEngine;
using UnityEngine.InputSystem;

public class Pause : MonoBehaviour
{
    public RectTransform pausePanel;
    private bool isPaused = false;

    private void Update()
    {
        if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            TogglePause();
        }
    }

    public void TogglePause()
    {
        if (isPaused)
        {
            OnUnpause();
        }
        else
        {
            OnPause();
        }
    }

    public void OnPause()
    {
        isPaused = true;
        if (pausePanel != null)
        {
            UIAnimationManager.Instance.ShowWindow(pausePanel, 0.5f);
        }
        Time.timeScale = 0;
    }

    public void OnUnpause()
    {
        isPaused = false;
        Time.timeScale = 1;
        if (pausePanel != null)
        {
            UIAnimationManager.Instance.HideWindow(pausePanel, 0.5f);
        }
    }
}
