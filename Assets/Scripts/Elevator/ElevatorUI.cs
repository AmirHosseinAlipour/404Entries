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
        if (messageText != null) messageText.gameObject.SetActive(false);
        musicChange = GetComponent<MusicChange>();
        SyncCurrentFloorWithPlayerPosition();
        UpdateFloorButtonsHighlight();
    }

    private void OnEnable()
    {
        SyncCurrentFloorWithPlayerPosition();
        UpdateFloorButtonsHighlight();
    }

    private void Update()
    {
        UpdateFloorButtonsHighlight();
    }

    public void SyncCurrentFloorWithPlayerPosition()
    {
        if (player == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null) player = playerObj.transform;
        }
        if (player != null)
        {
            currentFloor = MinimapIndicator2D.GetFloorIndex(player.position);
            if (AnotherInstace != null)
            {
                AnotherInstace.currentFloor = currentFloor;
            }
        }
    }

    public void UpdateFloorButtonsHighlight()
    {
        if (floorButtons == null) return;
        int targetFloor = MinimapIndicator2D.TargetFloorIndex;
        bool shouldHighlight = (targetFloor >= 0 && targetFloor < floorButtons.Length && targetFloor != currentFloor);

        for (int i = 0; i < floorButtons.Length; i++)
        {
            if (floorButtons[i] == null) continue;
            ColorBlock cb = floorButtons[i].colors;
            Image img = floorButtons[i].GetComponent<Image>();

            if (shouldHighlight && i == targetFloor)
            {
                Color paleTurquoise = new Color(0.55f, 0.92f, 0.90f, 1f);
                cb.normalColor = paleTurquoise;
                cb.selectedColor = paleTurquoise;
                cb.highlightedColor = new Color(0.70f, 0.97f, 0.95f, 1f);
                cb.pressedColor = new Color(0.40f, 0.82f, 0.80f, 1f);
                floorButtons[i].colors = cb;
                if (img != null) img.color = paleTurquoise;
            }
            else
            {
                cb.normalColor = Color.white;
                cb.selectedColor = Color.white;
                cb.highlightedColor = new Color(0.96f, 0.96f, 0.96f, 1f);
                cb.pressedColor = new Color(0.78f, 0.78f, 0.78f, 1f);
                floorButtons[i].colors = cb;
                if (img != null) img.color = Color.white;
            }
        }
    }

    private void OnFloorButtonPressed(int floorIndex)
    {
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

        // If player pressed the floor they are already on, exit immediately on that floor
        if (floorIndex == currentFloor)
        {
            if (buttonsPanel != null)
                buttonsPanel.gameObject.SetActive(false);
            
            RectTransform myRect = gameObject.GetComponent<RectTransform>();
            if (myRect != null)
                myRect.gameObject.SetActive(false);

            if (musicChange != null)
                musicChange.ToggleMusic();

            if (player != null && floorPositions[floorIndex] != null)
            {
                player.position = floorPositions[floorIndex].transform.position;
            }

            yield return new WaitForSeconds(0.15f);

            if (doorAnimation != null)
                doorAnimation.OpenDoors();

            if (musicChange != null)
                musicChange.PlayMainMusic();

            PlayerUIModeHelper.PlayerExitUIMode();
            yield break;
        }

        if (doorAnimation != null)
            doorAnimation.CloseDoors();

        yield return new WaitForSeconds(1f);

        if (buttonsPanel != null)
            buttonsPanel.gameObject.SetActive(false);
        
        RectTransform myRectTransit = gameObject.GetComponent<RectTransform>();
        if (myRectTransit != null)
            myRectTransit.gameObject.SetActive(false);

        if (musicChange != null)
            musicChange.ToggleMusic();

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

        PlayerUIModeHelper.PlayerExitUIMode();
    }

    private IEnumerator ShowMessageRoutine(string message)
    {
        if (messageText != null)
        {
            string translated = LocalizationManager.Translate(message);
            messageText.text = LocalizationManager.IsRTL ? FarsiTypewriter.FixRTL(translated, messageText) : translated;
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