using UnityEngine;

public class BaseProfessors : MonoBehaviour
{
    public RectTransform taskPanel;
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
            _player.isUIActive = true;
            UIAnimationManager.Instance.ShowWindow(taskPanel, 0.5f);
        }
    }
}
