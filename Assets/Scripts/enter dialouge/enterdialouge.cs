using System;
using UnityEngine;

public class enterdialouge : MonoBehaviour
{
    public GameObject dialouge;
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            dialouge.SetActive(true);
        }
    }
}
