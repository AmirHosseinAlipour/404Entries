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

    private int _lastCurrentIndex;
    private int currentIndex;

    private PlayerController _player;

    private void Awake()
    {
        _player = GameObject.FindWithTag("Player").GetComponent<PlayerController>();
        
        _allTasksBackButton = GameObject.FindWithTag("AllTasksBackButton").GetComponent<Button>();
        _currentTaskButton = GameObject.FindWithTag("CurrentTaskButton").GetComponent<Button>();
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
        var currentPhase = dialoguePhases[currentIndex / 2];
    
        for (int i = 0; i < currentPhase.dialogues.Count; i++)
        {
            // If this index exists in the dialogue list, use it
            if (i < currentPhase.dialogues.Count)
            {
                string text = currentPhase.dialogues[i];
                listOfTexts[i].transform.parent.parent.gameObject.SetActive(true);
                listOfTexts[i].text = text;
                listOfTexts[i].GetComponent<FarsiTypewriter>().SetText(text);
                listOfTexts[i].transform.parent.parent.gameObject.SetActive(false);
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
        _lastCurrentIndex = currentIndex;
        currentIndex = TaskManager.Instance.GetCurrentTaskIndex();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            // Disable player movement + UI task interactions
            PlayerUIModeHelper.PlayerEnterUIMode(_player);
            PlayerUIModeHelper.DisableTasksButton(_allTasksBackButton, _currentTaskButton);
            
            if (_lastCurrentIndex != currentIndex)
            {
                SetDialogues();
            }

            UIAnimationManager.Instance.ShowDialogueWindow(
                firstDialogue, 0.5f, listOfTexts[0].GetComponent<FarsiTypewriter>());
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
        return currentIndex / 2;
    }
}
