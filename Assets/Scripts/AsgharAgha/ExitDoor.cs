using System;
using System.Threading.Tasks;
using Unity.Cinemachine;
using UnityEngine;

public class ExitDoor : MonoBehaviour
{
    public AsgharManager gameManager;
    public TeacherController teacher;
    public Collider2D door;
    public Transform teleportPos;
    public CinemachineCamera roomCamera;
    private MusicChange musicChange;

    private void Start()
    {
        musicChange = GetComponent<MusicChange>();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            gameManager.WinGame();
            door.isTrigger = true;
            teacher.StopLooking();
            TaskManager.Instance.CompleteTask(teacher.TaskOrderNumber);
            roomCamera.Priority = 0;
            other.gameObject.transform.position = teleportPos.position;
            UIAnimationManager.Instance.ShowDialogueWindow(teacher.firstWinDialogue, 0.5f, teacher.firstWinDialogueFtw);
            musicChange.ToggleMusic();
            musicChange.PlayMainMusic();
            
        }
    }
}