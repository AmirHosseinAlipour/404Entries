using System;
using UnityEngine;

public class Madrak : MonoBehaviour
{
    public GameObject New_RespawnPoint;
    public GameObject GameobjectToTrigger; 
    private void OnTriggerEnter2D(Collider2D other)
    {
        GameobjectToTrigger.GetComponent<Collider2D>().isTrigger = true;
        AsgharManager asgharManager = GameobjectToTrigger.GetComponent<AsgharManager>();
        asgharManager.RespawnPoint = New_RespawnPoint;
        Destroy(gameObject);
    }
}
