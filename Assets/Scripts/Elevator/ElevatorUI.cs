using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class ElevatorUI : MonoBehaviour
{
    public Transform player;
    public DoorAnimation doorAnimation;
    public Text messageText;

    public Vector3[] floorPositions; 
    public Button[] floorButtons;
    public int currentFloor = 1; 

    private void Start()
    {
        for (int i = 0; i < floorButtons.Length; i++)
        {
            int index = i;
            floorButtons[i].onClick.AddListener(() => OnFloorButtonPressed(index));
        }
        player = GameObject.FindGameObjectWithTag("Player").transform;
        gameObject.SetActive(false);
        messageText.gameObject.SetActive(false);
    }

    private void OnFloorButtonPressed(int floorIndex)
    {
        if (floorIndex == currentFloor)
        {
            
            return;
        }

        if (floorIndex == 2 || floorIndex == 4)
        {
            StartCoroutine((ShowMessage()));
            return;
        }

        StartCoroutine(MoveToFloor(floorIndex));
    }

    private IEnumerator MoveToFloor(int floorIndex)
    {
        UIAnimationManager.Instance.HideWindow(gameObject.GetComponent<RectTransform>() , 0.5f);  
        doorAnimation.OpenDoors(); 
        player.position = floorPositions[floorIndex];
        currentFloor = floorIndex;

        yield return new WaitForSeconds(0.5f);
        
        
    }

    private IEnumerator ShowMessage()
    {
        messageText.gameObject.SetActive(true);
        yield return new WaitForSeconds(1.5f);
        messageText.gameObject.SetActive(false);
    }
}