using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[Serializable]
public class Level
{
    public int[] keys;
}

public class Masoumi : BaseProfessors
{
    [Header("Settings")]
    public BaseDropZone[] initialZones;
    public BaseDropZone[] zonesToDrop;
    public BaseDraggableItem[] items;
    [TextArea] public string[] texts;

    public List<Level> levelKeys;

    [Header("References")] 
    public RectTransform speechPanel;
    public RectTransform missionPanel;
    public Text textLevels;

    private int _currentLevel = 0;

    private void OnEnable()
    {
        BaseDraggableItem.OnItemDropCompleted += CheckComplete;
    }

    private void OnDisable()
    {
        BaseDraggableItem.OnItemDropCompleted -= CheckComplete;
    }

    private void CheckComplete()
    {
        if (_currentLevel >= levelKeys.Count) return;
        
        Level currentLevelData = levelKeys[_currentLevel];
        
        if (items.Length != currentLevelData.keys.Length)
        {
            return;
        }

        for (int i = 0; i < items.Length; i++)
        {
            BaseDraggableItem item = items[i];

            int correctZoneIndex = currentLevelData.keys[i];
            BaseDropZone correctZone = zonesToDrop[correctZoneIndex];

            if (item.currentDropZone != correctZone)
            {
                return;
            }
        }
        
        Debug.Log("before");
        StartCoroutine(LoadNextLevelAfterDelay(0.5f));
        Debug.Log("after");
    }

    private IEnumerator LoadNextLevelAfterDelay(float delay)
    {
        UIAnimationManager.Instance.HideWindow(speechPanel, 0.5f);
        yield return new WaitForSeconds(delay);
        NextLevel();
    }

    private void NextLevel()
    {
        _currentLevel++;

        if (_currentLevel >= levelKeys.Count)
        {
            HandleEnding();
            return;
        }
        
        UIAnimationManager.Instance.ShowWindow(speechPanel, 0.5f);

        for (int i = 0; i < initialZones.Length; i++)
        {
            items[i].currentDropZone = null;
            items[i].transform.SetParent(initialZones[i].transform, false);
            
            items[i].rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
            items[i].rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
            items[i].rectTransform.anchoredPosition = Vector2.zero;
        }

        textLevels.text = texts[_currentLevel];
        textLevels.GetComponent<FarsiTypewriter>().StartTyping();
    }

    private void HandleEnding()
    {
        UIAnimationManager.Instance.HideWindow(missionPanel, 0.5f);
        TaskManager.Instance.CompleteTask(TaskOrderNumber);
        UIAnimationManager.Instance.ShowDialogueWindow(firstWinDialogue, 0.3f, firstWinDialogueFtw);
    }
}