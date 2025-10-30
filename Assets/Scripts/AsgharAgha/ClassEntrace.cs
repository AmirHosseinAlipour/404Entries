using System;
using UnityEngine;

public class ClassEntrance : MonoBehaviour
{
    public AsgharManager gameManager;
    public TeacherController teacher;
    
    private GameObject _player;
    private bool started = false;

    private void Start()
    {
        _player = GameObject.FindWithTag("Player");
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") && !started)
        {
            started = true;
            Debug.Log("🎬 Class started!");
            teacher.StartLooking();
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        gameObject.GetComponent<Collider2D>().isTrigger = false;
    }
}