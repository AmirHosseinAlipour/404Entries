using System;
using Unity.VisualScripting;
using UnityEngine;

public class LockDoorAfterEnd : MonoBehaviour
{
    public AsgharManager am;
    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.gameObject.CompareTag("Player") && am._win)
        {
            gameObject.GetComponent<Collider2D>().isTrigger = false;
        }
    }
}
