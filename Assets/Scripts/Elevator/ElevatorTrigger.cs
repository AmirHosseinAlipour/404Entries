using System.Collections;
using UnityEngine;

public class ElevatorTrigger : MonoBehaviour
{
    public DoorAnimation doorAnimation;
    public RectTransform elevatorUI;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            StartCoroutine(ActivateElevator());
        }
    }

    private IEnumerator ActivateElevator()
    {
        doorAnimation.CloseDoors();
        yield return new WaitForSeconds(1.5f); 
        UIAnimationManager.Instance.ShowWindow(elevatorUI , 0.5f);
    }
}