using System;
using System.Threading.Tasks;
using Unity.Cinemachine;
using UnityEngine;

public class ExitDoor : MonoBehaviour
{
    public AsgharManager gameManager;
    public TeacherController teacher;
    public Collider2D door;
    [SerializeField] private Collider2D door2;
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
            door2.isTrigger = true;
            teacher.StopLooking();
            gameManager._win = true;
            TaskManager.Instance.CompleteTask(teacher.TaskOrderNumber);
            roomCamera.Priority = 0;
            UIAnimationManager.Instance.ShowDialogueWindow(teacher.firstWinDialogue, 0.5f, teacher.firstWinDialogueFtw);
            musicChange.ToggleMusic();
            musicChange.PlayMainMusic();
        }
    }
}