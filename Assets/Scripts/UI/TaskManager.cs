using System;
using System.Collections;
using System.Collections.Generic;
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
    public List<TaskData> taskDataList;

    public static TaskManager Instance;
    public Sprite Boy_image;
    public Sprite Girl_image;
    public Image Main_image;
    
    public Action OnCurrentIndexChange;
    public static event Action<int, bool> OnTaskStateChanged;
    public static event Action OnTasksUpdated;
    
    [Header("Task Triggers")]
    public List<GameObject> taskTriggers;

    [Header("Testing & Mobile Controls")]
    public bool forceShowJoystick = false;

    [Header("Language")]
    public GameLanguage gameLanguage = GameLanguage.Persian;

    public int TotalTasks => taskNames != null ? taskNames.Length : (taskDataList != null ? taskDataList.Count : 0);

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        SyncTaskData();
        UpdateActiveTarget();
    }

    private void SyncTaskData()
    {
        if (taskDataList != null && taskDataList.Count > 0)
        {
            if (taskNames == null || taskNames.Length == 0)
            {
                taskNames = new string[taskDataList.Count];
                taskCompleted = new bool[taskDataList.Count];
                for (int i = 0; i < taskDataList.Count; i++)
                {
                    if (taskDataList[i] != null)
                    {
                        taskNames[i] = taskDataList[i].taskName;
                        taskCompleted[i] = taskDataList[i].isCompleted;
                    }
                }
            }
        }
    }

    private void Start()
    {
        if (forceShowJoystick)
        {
            PlatformHelper.SetForceShowJoystick(true);
        }
        LocalizationManager.SetLanguage(gameLanguage);
        SetImage();
        OnTasksUpdated?.Invoke();
    }

    private void Update()
    {
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
            OnCurrentIndexChange?.Invoke();
            OnTasksUpdated?.Invoke();
        }
    }

    public void CompleteTask(int index)
    {
        if (taskCompleted != null && index >= 0 && index < taskCompleted.Length)
        {
            taskCompleted[index] = true;
            if (taskDataList != null && index < taskDataList.Count && taskDataList[index] != null)
            {
                taskDataList[index].isCompleted = true;
            }
            OnCurrentIndexChange?.Invoke();
            OnTaskStateChanged?.Invoke(index, true);
            OnTasksUpdated?.Invoke();
            UpdateActiveTarget();
        }
    }

    public void CompleteTask(TaskData task)
    {
        if (task == null) return;
        if (taskDataList != null && taskDataList.Contains(task))
        {
            int index = taskDataList.IndexOf(task);
            CompleteTask(index);
            return;
        }
        CompleteTaskByName(task.taskName);
    }

    public void CompleteCurrentTask()
    {
        int currentIndex = GetCurrentTaskIndex();
        if (currentIndex != -1)
        {
            CompleteTask(currentIndex);
        }
    }

    public void CompleteTaskByName(string name)
    {
        if (taskNames == null || string.IsNullOrEmpty(name)) return;
        for (int i = 0; i < taskNames.Length; i++)
        {
            if (string.Equals(taskNames[i], name, StringComparison.OrdinalIgnoreCase))
            {
                CompleteTask(i);
                return;
            }
        }
    }

    public bool IsTaskCompleted(int index)
    {
        if (taskCompleted == null || index < 0 || index >= taskCompleted.Length) return false;
        return taskCompleted[index];
    }

    public bool IsTaskCompleted(TaskData task)
    {
        if (task == null) return false;
        if (taskDataList != null && taskDataList.Contains(task))
        {
            return task.isCompleted;
        }
        return false;
    }

    public TaskData GetCurrentTaskData()
    {
        int index = GetCurrentTaskIndex();
        if (taskDataList != null && index >= 0 && index < taskDataList.Count)
        {
            return taskDataList[index];
        }
        return null;
    }

    public bool AreAllTasksCompleted()
    {
        return GetCurrentTaskIndex() == -1;
    }

    public int GetCurrentTaskIndex()
    {
        if (taskCompleted == null) return -1;
        for (int i = 0; i < taskCompleted.Length; i++)
        {
            if (!taskCompleted[i])
                return i;
        }
        return -1; 
    }

    public void UpdateActiveTarget()
    {
        if (taskTriggers == null || taskTriggers.Count == 0) return;

        int activeIndex = GetCurrentTaskIndex();
        for (int i = 0; i < taskTriggers.Count; i++)
        {
            if (taskTriggers[i] == null) continue;
            Target target = taskTriggers[i].GetComponent<Target>();
            if (target != null)
            {
                if (i == activeIndex)
                {
                    target.EnableTarget();
                }
                else
                {
                    target.DisableTarget();
                }
            }
        }
    }

    public void SetImage()
    {
        int index = (SelectionPlayer.instance != null) ? SelectionPlayer.instance.PlayerID : 0;
        if (Main_image != null)
        {
            if (index == 0)
            {
                if (Girl_image != null) Main_image.sprite = Girl_image;
            }
            else
            {
                if (Boy_image != null) Main_image.sprite = Boy_image;
            }
        }
    }

    public void HandleEnding()
    {
        if (screenFaderEnd != null)
        {
            screenFaderEnd.gameObject.SetActive(true);
            StartCoroutine(FadeAndEndRoutine());
        }
        else
        {
            SceneManager.LoadScene("Ending");
        }
    }

    private IEnumerator FadeAndEndRoutine()
    {
        screenFaderEnd.StartFade(1f, 1.5f);
        yield return new WaitForSeconds(1.5f);
        SceneManager.LoadScene("Ending");
    }
}