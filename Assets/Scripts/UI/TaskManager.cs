using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class TaskManager : MonoBehaviour
{
    [Header("Ending")] 
    public ScreenFaderEnd screenFaderEnd;
    
    [Header("Task Data")]
    public string[] taskNames;
    public bool[] taskCompleted;

    public static TaskManager Instance;
    public Sprite Boy_image;
    public Sprite Girl_image;
    public Image Main_image;
    
    public Action OnCurrentIndexChange;

    private void Start()
    {
        SetImage();
    }

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }
    public void CompleteTask(int index)
    {
        if (index >= 0 && index < taskCompleted.Length)
        {
            taskCompleted[index] = true;
            
            OnCurrentIndexChange?.Invoke();
        }
    }
    public int GetCurrentTaskIndex()
    {
        for (int i = 0; i < taskCompleted.Length; i++)
        {
            if (!taskCompleted[i])
                return i;
        }
        return -1; 
    }

    public void SetImage()
    {
        int index = SelectionPlayer.instance.PlayerID;
        if (index == 0)
        {
            Main_image.sprite = Girl_image;
        }
        else
        {
            Main_image.sprite = Boy_image;
        }
    }

    public void HandleEnding()
    {
        screenFaderEnd.gameObject.SetActive(true);
        screenFaderEnd.StartFade(0f, 2);
        SceneManager.LoadScene("Ending");
    }
}