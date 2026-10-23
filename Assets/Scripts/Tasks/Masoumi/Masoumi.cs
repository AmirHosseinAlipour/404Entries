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

[Serializable]
public class Options
{
    [TextArea] public List<string> items;
}

public class Masoumi : BaseProfessors
{
    [Header("Settings")]
    public BaseDropZone[] initialZones;
    public BaseDropZone[] zonesToDrop;
    public BaseDraggableItem[] items;
    public Text[] itemsOptionsTexts;
    [TextArea] public string[] texts;
    public Options[] textOptions;

    public List<Level> levelKeys;

    [Header("References")] 
    public RectTransform speechPanel;
    public RectTransform missionPanel;
    public Text textLevels;

    private int _currentLevel = 0;
    private MusicChange _musicChange;

    protected override void Awake()
    {
        base.Awake();

        _musicChange = GetComponent<MusicChange>();
    }

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
        
        if (speechPanel != null) UIAnimationManager.Instance.ShowWindow(speechPanel, 0.5f);

        if (initialZones != null && items != null)
        {
            for (int i = 0; i < initialZones.Length; i++)
            {
                if (i < items.Length && items[i] != null && initialZones[i] != null)
                {
                    items[i].currentDropZone = null;
                    items[i].transform.SetParent(initialZones[i].transform, false);
                    
                    if (items[i].rectTransform != null)
                    {
                        items[i].rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
                        items[i].rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
                        items[i].rectTransform.anchoredPosition = Vector2.zero;
                    }
                }
            }
        }

        if (textLevels != null && texts != null && _currentLevel < texts.Length)
        {
            var ftw = textLevels.GetComponent<FarsiTypewriter>();
            if (ftw != null) ftw.SetText(texts[_currentLevel]);
            else textLevels.text = texts[_currentLevel];
        }

        if (textOptions != null && _currentLevel < textOptions.Length && textOptions[_currentLevel].items != null)
        {
            for (int i = 0; i < textOptions[_currentLevel].items.Count; i++)
            {
                if (itemsOptionsTexts != null && i < itemsOptionsTexts.Length && itemsOptionsTexts[i] != null)
                {
                    var ftw = itemsOptionsTexts[i].GetComponent<FarsiTypewriter>();
                    if (ftw != null) ftw.SetText(textOptions[_currentLevel].items[i]);
                }
            }
        }
    }

    private void HandleEnding()
    {
        if (missionPanel != null) UIAnimationManager.Instance.HideWindow(missionPanel, 0.5f);
        if (TaskManager.Instance != null) TaskManager.Instance.CompleteTask(TaskOrderNumber);
        if (firstWinDialogue != null) UIAnimationManager.Instance.ShowDialogueWindow(firstWinDialogue, 0.3f, firstWinDialogueFtw);
        if (_musicChange != null)
        {
            _musicChange.ToggleMusic();
            _musicChange.PlayMainMusic();
        }
    }
}