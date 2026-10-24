using UnityEngine;

[CreateAssetMenu(fileName = "NewTaskData", menuName = "404Entries/Task Data")]
public class TaskData : ScriptableObject
{
    public string taskId;
    public string taskName;
    public string localizedKey;
    public Sprite taskIcon;
    public bool isCompleted;
    public BaseProfessors professor;
}
