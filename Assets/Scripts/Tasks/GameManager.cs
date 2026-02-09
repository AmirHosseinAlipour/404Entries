using System;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    [Header("Task Triggers")]
    public List<GameObject> taskTriggers;

    private int currentTaskIndex;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    private void Start()
    {
        DeactiveTriggerTaskTriggers();
        
        if (TaskManager.Instance != null)
        {
            TaskManager.Instance.OnCurrentIndexChange += ChangeCurrentIndex;
        }
        
        ChangeCurrentIndex();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            if (currentTaskIndex % 2 == 0)
            {
                TaskManager.Instance.CompleteTask(currentTaskIndex);   
            }
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            DeactiveTriggerTaskTriggers();
            if (taskTriggers[currentTaskIndex] != null)
            {
                taskTriggers[currentTaskIndex].GetComponent<Collider2D>().isTrigger = true;
                taskTriggers[currentTaskIndex].GetComponent<Target>().EnableTarget();
            }
        }
    }

    private void DeactiveTriggerTaskTriggers()
    {
        for (int i = 0; i < taskTriggers.Count; i++)
        {
            if (taskTriggers[i] != null)
            {
                taskTriggers[i].GetComponent<Collider2D>().isTrigger = false;
                taskTriggers[i].GetComponent<Target>().DisableTarget();
            }
        }
    }

    public void ChangeCurrentIndex()
    {
        currentTaskIndex = TaskManager.Instance.GetCurrentTaskIndex();
    }

    public int getCurrentTaskIndex()
    {
        return currentTaskIndex/2;
    }
}
