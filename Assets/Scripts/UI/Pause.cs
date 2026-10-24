using UnityEngine;
using UnityEngine.UI;

public class Pause : MonoBehaviour
{
    public RectTransform pausePanel;
    private bool isPaused = false;

    private void OnEnable()
    {
        InputReader.OnPausePressed += TogglePause;
    }

    private void OnDisable()
    {
        InputReader.OnPausePressed -= TogglePause;
    }

    private void Start()
    {
        if (pausePanel != null)
        {
            var img = pausePanel.GetComponent<Image>();
            if (img != null)
            {
                img.enabled = false;
            }
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
        PlayerUIModeHelper.PlayerEnterUIMode();
        if (pausePanel != null)
        {
            UIAnimationManager.Instance.ShowWindow(pausePanel, 0.25f);
        }
        Time.timeScale = 0;
    }

    public void OnUnpause()
    {
        isPaused = false;
        Time.timeScale = 1;
        if (pausePanel != null)
        {
            UIAnimationManager.Instance.HideWindow(pausePanel, 0.2f);
        }
        PlayerUIModeHelper.PlayerExitUIMode();
    }
}
