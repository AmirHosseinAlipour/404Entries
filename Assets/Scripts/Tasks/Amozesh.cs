using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class Amozesh : MonoBehaviour
{
    [System.Serializable]
    public class DialoguePhase
    {
        [TextArea]
        public List<string> dialogues;
    }

    [Header("Dialogue Data")]
    public  List<DialoguePhase> dialoguePhases;

    [Header("Dialogue UI")]
    public List<Text> listOfTexts;
    public RectTransform firstDialogue;
    
    
    // Task UI
    private Button _allTasksBackButton;
    private Button _currentTaskButton;

    private int currentIndex;

    private PlayerController _player;

    private void Awake()
    {
        var playerObj = GameObject.FindWithTag("Player");
        if (playerObj != null) _player = playerObj.GetComponent<PlayerController>();
        
        var backBtnObj = GameObject.FindWithTag("AllTasksBackButton");
        if (backBtnObj != null) _allTasksBackButton = backBtnObj.GetComponent<Button>();

        var currBtnObj = GameObject.FindWithTag("CurrentTaskButton");
        if (currBtnObj != null) _currentTaskButton = currBtnObj.GetComponent<Button>();
    }

    private void Start()
    {
        SetDialogues();
        
        if (TaskManager.Instance != null)
        {
            TaskManager.Instance.OnCurrentIndexChange += ChangeCurrentIndex;
        }
    }

    private void OnDestroy()
    {
        if (TaskManager.Instance != null)
        {
            TaskManager.Instance.OnCurrentIndexChange -= ChangeCurrentIndex;
        }
    }

    private void SetDialogues()
    {
        if (dialoguePhases == null || dialoguePhases.Count == 0) return;
        int phaseIdx = getCrrrentIndex();
        var currentPhase = dialoguePhases[phaseIdx];
        if (currentPhase == null || currentPhase.dialogues == null) return;
    
        for (int i = 0; i < listOfTexts.Count; i++)
        {
            if (listOfTexts[i] == null) continue;

            if (i < currentPhase.dialogues.Count)
            {
                string text = currentPhase.dialogues[i];
                var ftw = listOfTexts[i].GetComponent<FarsiTypewriter>();
                if (ftw != null)
                {
                    ftw.SetText(text);
                }
                else
                {
                    listOfTexts[i].text = "";
                }
            }
            else
            {
                // Mark this slot as "empty" so the next-node check can detect it
                listOfTexts[i].text = "";
            }
        }
    }

    
    public void ChangeCurrentIndex()
    {
        if (TaskManager.Instance != null)
        {
            currentIndex = TaskManager.Instance.GetCurrentTaskIndex();
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            if (_player == null) _player = other.gameObject.GetComponent<PlayerController>();

            // Disable player movement + UI task interactions
            PlayerUIModeHelper.PlayerEnterUIMode(_player);
            PlayerUIModeHelper.DisableTasksButton(_allTasksBackButton, _currentTaskButton);
            
            SetDialogues();

            if (firstDialogue != null && listOfTexts != null && listOfTexts.Count > 0 && listOfTexts[0] != null)
            {
                var ftw = listOfTexts[0].GetComponent<FarsiTypewriter>();
                UIAnimationManager.Instance.ShowDialogueWindow(firstDialogue, 0.5f, ftw);
            }
        }
    }

    private bool _hasFinishedEndingDialogue = false;
    private bool _isEndingTriggered = false;

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            if (_hasFinishedEndingDialogue && !_isEndingTriggered)
            {
                TriggerEnding();
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

            if (_hasFinishedEndingDialogue && !_isEndingTriggered)
            {
                TriggerEnding();
            }
        }
    }

    public void OnDialogueFinished(int phaseIndex)
    {
        PlayerUIModeHelper.PlayerExitUIMode(_player);
        PlayerUIModeHelper.EnableTasksButton(_allTasksBackButton, _currentTaskButton);

        if (phaseIndex >= 8)
        {
            _hasFinishedEndingDialogue = true;
            TriggerEnding();
        }
    }

    public void TriggerEnding()
    {
        if (_isEndingTriggered) return;
        _isEndingTriggered = true;

        if (TaskManager.Instance != null)
        {
            if (TaskManager.Instance.taskCompleted != null && TaskManager.Instance.taskCompleted.Length > 16)
            {
                TaskManager.Instance.CompleteTask(16);
            }
            TaskManager.Instance.HandleEnding();
        }
        else
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene("Ending");
        }
    }

    public int getCrrrentIndex()
    {
        if (dialoguePhases == null || dialoguePhases.Count == 0) return 0;

        if (TaskManager.Instance != null && TaskManager.Instance.taskCompleted != null)
        {
            // If Task 15 is completed (Library done), this is Phase 8 (Ending)
            if (TaskManager.Instance.taskCompleted.Length > 15 && TaskManager.Instance.taskCompleted[15])
            {
                return Mathf.Min(8, dialoguePhases.Count - 1);
            }

            // Find highest completed odd task (professors)
            for (int t = 13; t >= 1; t -= 2)
            {
                if (TaskManager.Instance.taskCompleted.Length > t && TaskManager.Instance.taskCompleted[t])
                {
                    return Mathf.Clamp((t + 1) / 2, 0, dialoguePhases.Count - 1);
                }
            }
            return 0;
        }

        if (currentIndex < 0) return 0;
        return Mathf.Clamp(currentIndex / 2, 0, dialoguePhases.Count - 1);
    }
}
