using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class Level
{
    public int[] keys;
}

public class Masoumi : BaseProfessors
{
    public BaseDropZone[] initialZones;
    public BaseDropZone[] zonesToDrop;
    public BaseDraggableItem[] items;

    public List<Level> levelKeys;

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
        StartCoroutine(LoadNextLevelAfterDelay(2.0f));
        Debug.Log("after");
    }

    private IEnumerator LoadNextLevelAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        NextLevel();
    }

    private void NextLevel()
    {
        _currentLevel++;

        if (_currentLevel >= levelKeys.Count)
        {
            return;
        }


        foreach (BaseDraggableItem item in items)
        {
            item.ReturnToOriginalPosition();
        }
    }
}