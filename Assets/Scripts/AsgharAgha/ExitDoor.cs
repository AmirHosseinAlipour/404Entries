using System.Threading.Tasks;
using UnityEngine;

public class ExitDoor : MonoBehaviour
{
    public AsgharManager gameManager;
    public TeacherController teacher;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            gameManager.WinGame();
            teacher.StopLooking();
            TaskManager.Instance.CompleteTask(teacher.TaskOrderNumber);
            UIAnimationManager.Instance.ShowDialogueWindow(teacher.firstDialogue.GetComponent<RectTransform>(), 0.5f, teacher.firstDialogueFtw);
        }
    }
}