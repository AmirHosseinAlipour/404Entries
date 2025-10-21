using UnityEngine;

public class ClassEntrance : MonoBehaviour
{
    public AsgharManager gameManager;
    public TeacherController teacher;
    private bool started = false;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") && !started)
        {
            started = true;
            Debug.Log("🎬 Class started!");
            teacher.StartLooking(); 
        }
    }
}