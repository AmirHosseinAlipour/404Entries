using System;
using UnityEngine;

public class Madrak : MonoBehaviour
{
    public GameObject GameobjectToTrigger; 
    private void OnTriggerEnter2D(Collider2D other)
    {
        GameobjectToTrigger.GetComponent<Collider2D>().isTrigger = true;
        Destroy(gameObject);
    }
}
