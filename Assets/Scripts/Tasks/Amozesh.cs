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

    private void SetDialogues()
    {
        if (dialoguePhases == null || dialoguePhases.Count == 0) return;
        int phaseIdx = Mathf.Clamp(currentIndex / 2, 0, dialoguePhases.Count - 1);
        var currentPhase = dialoguePhases[phaseIdx];
        if (currentPhase == null || currentPhase.dialogues == null) return;
    
        for (int i = 0; i < listOfTexts.Count; i++)
        {
            if (listOfTexts[i] == null) continue;

            if (i < currentPhase.dialogues.Count)
            {
                string text = currentPhase.dialogues[i];
                if (listOfTexts[i].transform.parent != null && listOfTexts[i].transform.parent.parent != null)
                {
                    listOfTexts[i].transform.parent.parent.gameObject.SetActive(true);
                }
                listOfTexts[i].text = text;
                var ftw = listOfTexts[i].GetComponent<FarsiTypewriter>();
                if (ftw != null) ftw.SetText(text);
                if (listOfTexts[i].transform.parent != null && listOfTexts[i].transform.parent.parent != null)
                {
                    listOfTexts[i].transform.parent.parent.gameObject.SetActive(false);
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
                UIAnimationManager.Instance.ShowDialogueWindow(
                    firstDialogue, 0.5f, listOfTexts[0].GetComponent<FarsiTypewriter>());
            }
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            if (TaskManager.Instance != null && TaskManager.Instance.taskCompleted != null && TaskManager.Instance.taskCompleted.Length > 16)
            {
                if (TaskManager.Instance.taskCompleted[16] == true)
                {
                    TaskManager.Instance.HandleEnding();
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
        }
    }

    public int getCrrrentIndex()
    {
        if (dialoguePhases == null || dialoguePhases.Count == 0) return 0;
        return Mathf.Clamp(currentIndex / 2, 0, dialoguePhases.Count - 1);
    }
}
