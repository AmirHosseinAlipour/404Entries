using System;
using System.Collections;
using UnityEngine;

public class ElevatorTrigger : MonoBehaviour
{
    public DoorAnimation doorAnimation;
    public GameObject elevatorUI;
    public GameObject ButoonUI;
    private MusicChange musicChange;

    private void Start()
    {
        musicChange = GetComponent<MusicChange>();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log("Player entered trigger");
            StartCoroutine(ActivateElevator());
        }
    }

    private IEnumerator ActivateElevator()
    {
        if (doorAnimation != null)
            doorAnimation.CloseDoors();

        Debug.Log("Elevator activated");
        yield return new WaitForSeconds(1f); 
        if (elevatorUI != null)
        {
            musicChange.ToggleMusic();
            musicChange.PlayThemeMusic();
            
            UIAnimationManager.Instance.ShowWindow(elevatorUI.GetComponent<RectTransform>() , 0.5f );
            //elevatorUI.SetActive(true);
            Debug.Log("Elevator UI Activated ✅");
        }
        else Debug.LogWarning("Elevator UI is null!");

        if (ButoonUI != null)
        {
            UIAnimationManager.Instance.ShowWindow(ButoonUI.GetComponent<RectTransform>() , 0.5f );
           // ButoonUI.SetActive(true);
            Debug.Log("Button UI Activated ✅");
        }
        else Debug.LogWarning("Button UI is null!");

        yield return null;
    }
}