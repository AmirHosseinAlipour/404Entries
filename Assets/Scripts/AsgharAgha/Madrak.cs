using System;
using UnityEngine;

public class Madrak : MonoBehaviour
{
    public GameObject New_RespawnPoint;
    public GameObject GameobjectToTrigger;
    public AsgharManager Manager;
    private void OnTriggerEnter2D(Collider2D other)
    {
        GameobjectToTrigger.GetComponent<Collider2D>().isTrigger = true;
        Manager.RespawnPoint = New_RespawnPoint;
        Destroy(gameObject);
    }
}
