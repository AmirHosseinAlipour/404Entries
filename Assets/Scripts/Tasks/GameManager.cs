using System;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    
    [Header("Task Triggers")]
    public List<GameObject> taskTriggers;

    private int currentTaskIndex;

    [Header("Testing & Controls")]
    public bool forceShowJoystick = false;

    [Header("Language")]
    public GameLanguage gameLanguage = GameLanguage.Persian;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    private void Start()
    {
        if (forceShowJoystick)
        {
            PlatformHelper.SetForceShowJoystick(true);
        }

        LocalizationManager.SetLanguage(gameLanguage);

        DeactiveTriggerTaskTriggers();
        
        if (TaskManager.Instance != null)
        {
            TaskManager.Instance.OnCurrentIndexChange += ChangeCurrentIndex;
        }
        
        ChangeCurrentIndex();
    }

    private void OnDestroy()
    {
        if (TaskManager.Instance != null)
        {
            TaskManager.Instance.OnCurrentIndexChange -= ChangeCurrentIndex;
        }
    }

    private void Update()
    {
        if (UnityEngine.InputSystem.Keyboard.current != null && UnityEngine.InputSystem.Keyboard.current.f2Key.wasPressedThisFrame)
        {
            forceShowJoystick = !forceShowJoystick;
            PlatformHelper.SetForceShowJoystick(forceShowJoystick);
        }

        if (LocalizationManager.CurrentLanguage != gameLanguage)
        {
            gameLanguage = LocalizationManager.CurrentLanguage;
        }
    }

    private void OnValidate()
    {
        if (Application.isPlaying)
        {
            PlatformHelper.SetForceShowJoystick(forceShowJoystick);
            LocalizationManager.SetLanguage(gameLanguage);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            if (currentTaskIndex >= 0 && currentTaskIndex % 2 == 0 && currentTaskIndex < 16)
            {
                if (TaskManager.Instance != null)
                {
                    TaskManager.Instance.CompleteTask(currentTaskIndex);   
                }
            }
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            DeactiveTriggerTaskTriggers();
            if (taskTriggers != null && currentTaskIndex >= 0 && currentTaskIndex < taskTriggers.Count && taskTriggers[currentTaskIndex] != null)
            {
                Collider2D col = taskTriggers[currentTaskIndex].GetComponent<Collider2D>();
                if (col != null) col.isTrigger = true;
                Target target = taskTriggers[currentTaskIndex].GetComponent<Target>();
                if (target != null) target.EnableTarget();
            }
        }
    }

    private void DeactiveTriggerTaskTriggers()
    {
        if (taskTriggers == null) return;
        for (int i = 0; i < taskTriggers.Count; i++)
        {
            if (taskTriggers[i] != null)
            {
                Collider2D col = taskTriggers[i].GetComponent<Collider2D>();
                if (col != null) col.isTrigger = false;
                Target target = taskTriggers[i].GetComponent<Target>();
                if (target != null) target.DisableTarget();
            }
        }
    }

    public void ChangeCurrentIndex()
    {
        if (TaskManager.Instance != null)
        {
            currentTaskIndex = TaskManager.Instance.GetCurrentTaskIndex();
        }
    }

    public int getCurrentTaskIndex()
    {
        return currentTaskIndex / 2;
    }
}
