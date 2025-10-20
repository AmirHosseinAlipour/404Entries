using UnityEngine;

public class TaskManager : MonoBehaviour
{
    [Header("Task Data")]
    public string[] taskNames;      
    public bool[] taskCompleted;      

    public static TaskManager Instance; 

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
}