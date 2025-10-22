using System;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    [Header("Task Triggers")]
    public List<GameObject> taskTriggers; 

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            int currentTaskIndex = TaskManager.Instance.GetCurrentTaskIndex();
            DeactiveTriggerTaskTriggers();
            taskTriggers[currentTaskIndex / 2].GetComponent<Collider2D>().isTrigger = true;
            if (currentTaskIndex % 2 == 0)
            {
                TaskManager.Instance.CompleteTask(currentTaskIndex);   
            }
            if (currentTaskIndex != -1)
            {
                int dialogueIndex = currentTaskIndex / 2;

                // lets  start dialogue in this Method 
                // StartDialogue(dialogueIndex);

                Debug.Log("Player entered trigger. Dialogue index: " + dialogueIndex);
            }
        }
    }

    private void DeactiveTriggerTaskTriggers()
    {
        for (int i = 0; i < taskTriggers.Count; i++)
        {
            taskTriggers[i].GetComponent<Collider2D>().isTrigger = false;
        }
    }
}
