using UnityEngine;

public class BaseProfessors : MonoBehaviour
{
    public RectTransform taskPanel;
    public int TaskOrderNumber;
    public RectTransform afterEndingDialogue;
    protected PlayerController _player;

    protected virtual void Start()
    {
        _player = GameObject.FindWithTag("Player").GetComponent<PlayerController>();
    }
    
    // If player enters the trigger zone it's input get skipped and the UI panel would show up!
    protected virtual void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            if (TaskManager.Instance.taskCompleted[TaskOrderNumber])
            {
                UIAnimationManager.Instance.ShowWindow(afterEndingDialogue, 0.5f);
            }
            else
            {
                _player.isUIActive = true;
                UIAnimationManager.Instance.ShowWindow(taskPanel, 0.5f);
            }
        }
    }
}
