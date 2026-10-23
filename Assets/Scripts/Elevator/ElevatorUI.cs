using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class ElevatorUI : MonoBehaviour
{
    public Transform player;
    public DoorAnimation doorAnimation;
    public Text messageText;

    public GameObject[] floorPositions; 
    public Button[] floorButtons;
    public int currentFloor = 1;
    public RectTransform buttonsPanel;
    public ElevatorUI AnotherInstace; 
    private MusicChange musicChange;
    private void Start()
    {
        for (int i = 0; i < floorButtons.Length; i++)
        {
            int index = i;
            floorButtons[i].onClick.AddListener(() => OnFloorButtonPressed(index));
        }
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null) player = playerObj.transform;
        //gameObject.SetActive(false);
        if (messageText != null) messageText.gameObject.SetActive(false);
        musicChange = GetComponent<MusicChange>();
    }

    private void OnFloorButtonPressed(int floorIndex)
    {
        if (floorIndex == currentFloor)
        {
            StopCoroutine("ShowMessageRoutine");
            StartCoroutine(ShowMessageRoutine("شما در همین طبقه هستید!"));
            return;
        }

        UIAnimationManager.Instance.StartCoroutine(MoveToFloor(floorIndex));
    }

    private IEnumerator MoveToFloor(int floorIndex)
    {
        if (floorIndex < 0 || floorIndex >= floorPositions.Length || floorPositions[floorIndex] == null)
            yield break;

        if (player == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null) player = playerObj.transform;
        }

        PlayerController p = (player != null) ? player.GetComponent<PlayerController>() : null;
        if (p != null)
        {
            p.SetIdleDirection(new Vector2(0, -1));
        }

        // Close doors first
        if (doorAnimation != null)
            doorAnimation.CloseDoors();

        yield return new WaitForSeconds(1f);

        // Hide UI panels
        if (buttonsPanel != null)
            buttonsPanel.gameObject.SetActive(false);
        
        RectTransform myRect = gameObject.GetComponent<RectTransform>();
        if (myRect != null)
            myRect.gameObject.SetActive(false);

        if (musicChange != null)
            musicChange.ToggleMusic();

        // Teleport player
        if (player != null && floorPositions[floorIndex] != null)
        {
            player.position = floorPositions[floorIndex].transform.position;
        }

        currentFloor = floorIndex;
        if (AnotherInstace != null)
        {
            AnotherInstace.currentFloor = floorIndex;
        }

        yield return new WaitForSeconds(0.2f);

        if (doorAnimation != null)
            doorAnimation.OpenDoors();

        if (musicChange != null)
            musicChange.PlayMainMusic();
    }

    private IEnumerator ShowMessageRoutine(string message)
    {
        if (messageText != null)
        {
            string translated = LocalizationManager.Translate(message);
            messageText.text = LocalizationManager.IsRTL ? ArabicSupport.ArabicFixer.Fix(translated, true, true) : translated;
            messageText.alignment = LocalizationManager.IsRTL ? TextAnchor.MiddleCenter : TextAnchor.MiddleCenter;
            messageText.gameObject.SetActive(true);
            yield return new WaitForSeconds(1.5f);
            messageText.gameObject.SetActive(false);
        }
    }

    private IEnumerator ShowMessage()
    {
        return ShowMessageRoutine("شما در همین طبقه هستید!");
    }
}