using System.Threading.Tasks;
using UnityEngine;

public class ExitDoor : MonoBehaviour
{
    public AsgharManager gameManager;
    public TeacherController teacher;
    public Collider2D door;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            gameManager.WinGame();
            door.isTrigger = true;
            teacher.StopLooking();
            TaskManager.Instance.CompleteTask(teacher.TaskOrderNumber);
            UIAnimationManager.Instance.ShowDialogueWindow(teacher.firstWinDialogue, 0.5f, teacher.firstWinDialogueFtw);
        }
    }
}