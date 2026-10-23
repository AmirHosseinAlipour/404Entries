using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class BaseProfessors : MonoBehaviour
{
    [Header("Base Fields")]
    // Accept challenge message
    public RectTransform StartPanel;
    public RectTransform acceptRect;
    public Text[] listOfTexts;
    public Target target;

    private const int InitialDialogueCount = 3;
    
    // To check the correct index of completedTasks
    public int TaskOrderNumber;
    
    [Header("After mission")]
    public RectTransform afterWinDialogue;
    public RectTransform firstWinDialogue;
    public FarsiTypewriter firstWinDialogueFtw;
    
    [Header("After task not complete")]
    public RectTransform afterFailDialogue;
    public RectTransform firstFailDialogue;
    public FarsiTypewriter firstFailDialogueFtw;
    
    // Task UI
    protected Button _allTasksBackButton;
    protected Button _currentTaskButton;
    protected GameObject _joyStick;
    
    protected PlayerController _player;

    protected virtual void Awake()
    {
        var playerObj = GameObject.FindWithTag("Player");
        if (playerObj != null) _player = playerObj.GetComponent<PlayerController>();
        
        var backBtnObj = GameObject.FindWithTag("AllTasksBackButton");
        if (backBtnObj != null) _allTasksBackButton = backBtnObj.GetComponent<Button>();

        var currBtnObj = GameObject.FindWithTag("CurrentTaskButton");
        if (currBtnObj != null) _currentTaskButton = currBtnObj.GetComponent<Button>();

        _joyStick = GameObject.FindWithTag("JoyStick");

        target = gameObject.GetComponent<Target>();
    }

    protected virtual void Start()
    {
        if (_player == null)
        {
            var playerObj = GameObject.FindWithTag("Player");
            if (playerObj != null) _player = playerObj.GetComponent<PlayerController>();
        }
    }
    
    // If player enters the trigger zone it's input get skipped and the UI panel would show up!
    protected virtual void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            if (_player == null) _player = other.gameObject.GetComponent<PlayerController>();

            if (TaskManager.Instance != null && TaskManager.Instance.taskCompleted != null && TaskOrderNumber < TaskManager.Instance.taskCompleted.Length && TaskManager.Instance.taskCompleted[TaskOrderNumber])
            {
                if (afterWinDialogue != null) UIAnimationManager.Instance.ShowWindow(afterWinDialogue, 0.5f);
            }
            else
            {
                PlayerUIModeHelper.PlayerEnterUIMode(_player);
                PlayerUIModeHelper.DisableTasksButton(_allTasksBackButton, _currentTaskButton);
                UIUtils.SetAlpha(_currentTaskButton != null ? _currentTaskButton.gameObject : null, 0f);
                UIUtils.SetAlpha(_joyStick, 0f);
                
                if (StartPanel != null) UIAnimationManager.Instance.ShowWindow(StartPanel, 0.5f);
                
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
    
    // To show the accept challenge message in a proper type writer order!
    private IEnumerator TextSequence()
    {
        // First of all we write the first 3 text which is the accept message + yes/no option
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
    
    void OnEnable()
    {
        // Subscribe to the event when this script is enabled
        SwitchWindow.OnPanelStateChanged += HandlePanelStateChange;
    }

    void OnDisable()
    {
        // Unsubscribe when this script is disabled to prevent memory leaks!
        SwitchWindow.OnPanelStateChanged -= HandlePanelStateChange;
    }
    
    private void HandlePanelStateChange(bool isActive)
    {
        if (!isActive)
        {
            PlayerUIModeHelper.PlayerExitUIMode(_player);
            PlayerUIModeHelper.EnableTasksButton(_allTasksBackButton, _currentTaskButton);
            UIUtils.SetAlpha(_currentTaskButton != null ? _currentTaskButton.gameObject : null, 1f);
            if (PlatformHelper.IsMobile())
            {
                UIUtils.SetAlpha(_joyStick, 1f);
            }
        }
    }

    public void GetBackToNormal()
    {
        PlayerUIModeHelper.PlayerExitUIMode(_player);
        PlayerUIModeHelper.EnableTasksButton(_allTasksBackButton, _currentTaskButton);
        UIUtils.SetAlpha(_currentTaskButton != null ? _currentTaskButton.gameObject : null, 1f);
        if (PlatformHelper.IsMobile())
        {
            UIUtils.SetAlpha(_joyStick, 1f);
        }
    }
}
