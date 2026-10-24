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
            StartCoroutine(ActivateElevator());
        }
    }

    private IEnumerator ActivateElevator()
    {
        if (doorAnimation != null)
            doorAnimation.CloseDoors();

        yield return new WaitForSeconds(1f); 
        PlayerUIModeHelper.PlayerEnterUIMode();
        if (elevatorUI != null)
        {
            if (musicChange != null)
            {
                musicChange.ToggleMusic();
                musicChange.PlayThemeMusic();
            }
            
            UIAnimationManager.Instance.ShowWindow(elevatorUI.GetComponent<RectTransform>(), 0.25f);
        }

        if (ButoonUI != null)
        {
            UIAnimationManager.Instance.ShowWindow(ButoonUI.GetComponent<RectTransform>(), 0.25f);
        }

        yield return null;
    }
}