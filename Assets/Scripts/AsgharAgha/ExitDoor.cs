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
        }
    }
}