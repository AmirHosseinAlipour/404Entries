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
    private bool _hasExited = false;

    private void Start()
    {
        musicChange = GetComponent<MusicChange>();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (_hasExited) return;
        if (other.CompareTag("Player"))
        {
            _hasExited = true;
            if (gameManager != null) gameManager.WinGame();
            if (door != null) door.isTrigger = true;
            if (door2 != null) door2.isTrigger = true;
            if (teacher != null)
            {
                teacher.StopLooking();
                if (TaskManager.Instance != null)
                    TaskManager.Instance.CompleteTask(teacher.TaskOrderNumber);
                if (UIAnimationManager.Instance != null && teacher.firstWinDialogue != null)
                    UIAnimationManager.Instance.ShowDialogueWindow(teacher.firstWinDialogue, 0.5f, teacher.firstWinDialogueFtw);
            }
            if (gameManager != null) gameManager._win = true;
            if (roomCamera != null) roomCamera.Priority = 0;
            if (musicChange != null)
            {
                musicChange.ToggleMusic();
                musicChange.PlayMainMusic();
            }
        }
    }
}