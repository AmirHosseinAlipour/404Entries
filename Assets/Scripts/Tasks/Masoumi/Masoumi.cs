using System;
using UnityEngine;

public class Masoumi : MonoBehaviour
{
    public RectTransform taskPanel;
    private PlayerController _player;

    private void Start()
    {
        _player = GameObject.FindWithTag("Player").GetComponent<PlayerController>();
    }
    
    // If player enters the trigger zone it's input get skipped and the UI panel would show up!
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            _player.isUIActive = true;
            UIAnimationManager.Instance.ShowWindow(taskPanel);
        }
    }
}
