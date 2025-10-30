using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class BaseProfessors : MonoBehaviour
{
    [Header("Base Fields")]
    // Accept challenge message
    public RectTransform StartPanel;
    public Text[] listOfTexts;

    private const int InitialDialogueCount = 3;
    
    // To check the correct index of completedTasks
    public int TaskOrderNumber;
    
    [Header("After mission")]
    public RectTransform afterEndingDialogue;
    public GameObject firstDialogue;
    public FarsiTypewriter firstDialogueFtw;
    
    // Task UI
    protected Button _allTasksBackButton;
    protected Button _currentTaskButton;
    
    protected PlayerController _player;

    private void Awake()
    {
        _player = GameObject.FindWithTag("Player").GetComponent<PlayerController>();
        
        _allTasksBackButton = GameObject.FindWithTag("AllTasksBackButton").GetComponent<Button>();
        _currentTaskButton = GameObject.FindWithTag("CurrentTaskButton").GetComponent<Button>();
    }

    protected virtual void Start()
    {
        _player = GameObject.FindWithTag("Player").GetComponent<PlayerController>();
    }
    
    // If player enters the trigger zone it's input get skipped and the UI panel would show up!
    protected virtual void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            if (TaskManager.Instance.taskCompleted[TaskOrderNumber])
            {
                UIAnimationManager.Instance.ShowWindow(afterEndingDialogue, 0.5f);
            }
            else
            {
                PlayerUIModeHelper.PlayerEnterUIMode(_player);
                PlayerUIModeHelper.DisableTasksButton(_allTasksBackButton, _currentTaskButton);
                
                UIAnimationManager.Instance.ShowWindow(StartPanel, 0.5f);
                
                for (int i = 0; i < 3; i++)
                {
                    // Reset the text so each time the text would be written in type writer effect!
                    listOfTexts[i].text = "";
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
            FarsiTypewriter text = listOfTexts[i].gameObject.GetComponent<FarsiTypewriter>();
            int len = text.len;
            text.StartTyping();
            yield return new WaitForSeconds(text.typeCharTime * len);
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
        }
    }
}
