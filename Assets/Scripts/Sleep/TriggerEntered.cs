using System;
using UnityEngine;

public class TriggerEntered : MonoBehaviour
{
    public RectTransform CanvasToShow;
    public GameObject GameManager;

    void Start()
    {
        CanvasToShow.gameObject.SetActive(false);
        GameManager.SetActive(false);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            GameManager.SetActive(true);
            UIAnimationManager.Instance.ShowWindow(CanvasToShow , 0.5f );
        }
     
    }
}
